using CleanArchitectureAnalyzer.Constants;
using CleanArchitectureAnalyzer.Diagnostics;
using CleanArchitectureAnalyzer.Extensions;
using CleanArchitectureAnalyzer.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CleanArchitectureAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InfrastructureAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            InfrastructureDiagnosticDescriptors.InfrastructureNoBusinessLogicRule,
            InfrastructureDiagnosticDescriptors.InfrastructureClassSealedRule,
            InfrastructureDiagnosticDescriptors.DbContextNotExposedRule,
            InfrastructureDiagnosticDescriptors.RepositoriesInternalOrSealedRule,
            InfrastructureDiagnosticDescriptors.DatabaseModelsNotExposedRule,
            InfrastructureDiagnosticDescriptors.ParameterizedSqlRule,
            InfrastructureDiagnosticDescriptors.RepositoriesNoValidationRule,
            InfrastructureDiagnosticDescriptors.HttpClientsOnlyInInfrastructureRule,
            InfrastructureDiagnosticDescriptors.InfrastructureServiceThreadSafeRule,
            InfrastructureDiagnosticDescriptors.NoHardcodedSecretsRule,
            InfrastructureDiagnosticDescriptors.DbContextConstructorRule,
            InfrastructureDiagnosticDescriptors.UseLoggerInsteadOfConsoleRule,
            InfrastructureDiagnosticDescriptors.RepositoryInterfaceRequirementRule,
            InfrastructureDiagnosticDescriptors.InfrastructureSuffixRule,
            InfrastructureDiagnosticDescriptors.MigrationsLayerRule,
            InfrastructureDiagnosticDescriptors.ParameterlessConstructorRestrictionRule,
            InfrastructureDiagnosticDescriptors.AsNoTrackingRequirementRule,
            InfrastructureDiagnosticDescriptors.RepositoriesMustNotCommitTransactionsRule,
            InfrastructureDiagnosticDescriptors.InfrastructureDomainInterfaceRule,
            InfrastructureDiagnosticDescriptors.FileOperationsAsyncRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.TypeKind != TypeKind.Class && namedTypeSymbol.TypeKind != TypeKind.Struct)
            {
                return;
            }

            var name = namedTypeSymbol.Name;

            // CAA2015: Migrations must not be in Domain or Application layers
            var isMigration = IsEFMigration(namedTypeSymbol);
            if (isMigration)
            {
                if (ArchitectureHelpers.IsDomain(context, namedTypeSymbol) || ArchitectureHelpers.IsApplication(context, namedTypeSymbol))
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.MigrationsLayerRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2008: HTTP Clients and API calls must only reside in Infrastructure
            var isHttpClientOrApiUsage = namedTypeSymbol.GetFields().Any(f => IsHttpClientType(f.Type)) || 
                                          namedTypeSymbol.GetConstructors().Any(c => c.Parameters.Any(p => IsHttpClientType(p.Type)));
            if (isHttpClientOrApiUsage)
            {
                if (!ArchitectureHelpers.IsInfrastructure(context, namedTypeSymbol))
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.HttpClientsOnlyInInfrastructureRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // From now on, only check classes in Infrastructure layer
            if (!ArchitectureHelpers.IsInfrastructure(context, namedTypeSymbol))
            {
                return;
            }

            var isRepository = name.EndsWith("Repository");
            var isDbContext = inheritsFromDbContext(namedTypeSymbol);

            // CAA2002: Infrastructure classes implementing services should be sealed
            bool shouldBeSealed = namedTypeSymbol.Interfaces.Any() || isRepository || isDbContext || name.EndsWith("Service") || name.EndsWith("Client");
            if (shouldBeSealed && !namedTypeSymbol.IsSealed)
            {
                var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.InfrastructureClassSealedRule, namedTypeSymbol.Locations[0], name);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA2003: DbContext/DbConnection must not be exposed in public members
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member.DeclaredAccessibility == Accessibility.Public)
                {
                    ITypeSymbol typeToCheck = null;
                    if (member is IPropertySymbol prop) typeToCheck = prop.Type;
                    else if (member is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary) typeToCheck = method.ReturnType;

                    if (typeToCheck != null && IsDatabaseConnectionOrContextType(typeToCheck))
                    {
                        var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.DbContextNotExposedRule, member.Locations[0], name, member.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA2004: Repositories must be internal or sealed
            if (isRepository)
            {
                bool isInternalOrSealed = namedTypeSymbol.DeclaredAccessibility == Accessibility.Internal || namedTypeSymbol.IsSealed;
                if (!isInternalOrSealed)
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.RepositoriesInternalOrSealedRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2005: Database models/persistence entities must not be exposed outside Infrastructure
            if (!isRepository && !isDbContext)
            {
                foreach (var method in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary))
                {
                    if (method.DeclaredAccessibility == Accessibility.Public && IsPersistenceModel(method.ReturnType))
                    {
                        var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.DatabaseModelsNotExposedRule, method.Locations[0], name, method.ReturnType.Name, method.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA2009: Infrastructure services must be thread-safe (no public mutable state)
            bool isServiceOrRepo = isRepository || name.EndsWith("Service") || name.EndsWith("Client");
            if (isServiceOrRepo)
            {
                foreach (var prop in namedTypeSymbol.GetProperties())
                {
                    if (prop.DeclaredAccessibility == Accessibility.Public && prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                    {
                        var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.InfrastructureServiceThreadSafeRule, prop.Locations[0], name, prop.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA2010: No hardcoded secrets in fields/constants
            foreach (var field in namedTypeSymbol.GetFields())
            {
                var fieldName = field.Name;
                bool isSecretKey = fieldName.Contains("ConnectionString") || fieldName.Contains("Secret") || fieldName.Contains("ApiKey") || fieldName.Contains("Password");
                if (isSecretKey && field.HasConstantValue && field.ConstantValue is string val && !string.IsNullOrWhiteSpace(val))
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.NoHardcodedSecretsRule, field.Locations[0], name, fieldName);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2011: DbContext should inject DbContextOptions
            if (isDbContext)
            {
                var constructors = namedTypeSymbol.GetConstructors();
                bool hasOptionsCtor = constructors.Any(c => c.Parameters.Any(p => p.Type.Name.Contains("DbContextOptions")));
                if (!hasOptionsCtor && constructors.Any())
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.DbContextConstructorRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2013: Repository must implement a repository interface
            if (isRepository)
            {
                bool implementsRepoInterface = namedTypeSymbol.Interfaces.Any(i => i.Name.EndsWith("Repository") || i.Name.Contains("Reader") || i.Name.Contains("Writer"));
                if (!implementsRepoInterface)
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.RepositoryInterfaceRequirementRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2014: Infrastructure naming suffix rule
            var validSuffixes = new[] { "Repository", "DbContext", "Service", "Client", "Adapter", "Provider", "Factory", "Migrator", "Handler", "Exception", "Configuration" };
            bool hasValidSuffix = validSuffixes.Any(suffix => name.EndsWith(suffix));
            if (!hasValidSuffix && !isMigration)
            {
                var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.InfrastructureSuffixRule, namedTypeSymbol.Locations[0], name);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA2016: Parameterless constructor restriction for services with dependencies
            bool hasDependencies = namedTypeSymbol.GetFields().Any(f => f.Type.TypeKind == TypeKind.Interface || f.Type.Name.EndsWith("Repository") || f.Type.Name.EndsWith("Service"));
            if (hasDependencies)
            {
                var parameterlessCtor = namedTypeSymbol.GetConstructors().FirstOrDefault(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
                if (parameterlessCtor != null)
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.ParameterlessConstructorRestrictionRule, parameterlessCtor.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2019: Infrastructure class must not implement Domain Aggregate/Entity/Event interfaces directly
            foreach (var iface in namedTypeSymbol.Interfaces)
            {
                var ifaceName = iface.Name;
                bool isDomainContract = ifaceName.Equals("IAggregateRoot") || ifaceName.Equals("IEntity") || ifaceName.Equals("IDomainEvent");
                if (isDomainContract)
                {
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.InfrastructureDomainInterfaceRule, namedTypeSymbol.Locations[0], name, ifaceName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null || symbol.ContainingType == null) return;

            var containingClass = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (containingClass == null) return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol == null || !ArchitectureHelpers.IsInfrastructure(context, classSymbol)) return;

            var typeName = symbol.ContainingType.Name;
            var fullType = symbol.ContainingType.ToString();
            var methodName = symbol.Name;

            // CAA2012: System.Console detection
            if (fullType == "System.Console" && (methodName == "Write" || methodName == "WriteLine"))
            {
                var methodDecl = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                var currentMethodName = methodDecl != null ? methodDecl.Identifier.Text : "Unknown";
                var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.UseLoggerInsteadOfConsoleRule, invocation.GetLocation(), classSymbol.Name, currentMethodName);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA2018: Repositories should not Save/Commit directly
            if (classSymbol.Name.EndsWith("Repository"))
            {
                bool isSaveOrCommit = methodName.Equals("SaveChanges") || methodName.Equals("SaveChangesAsync") || 
                                     methodName.Equals("Commit") || methodName.Equals("CommitAsync");
                if (isSaveOrCommit)
                {
                    var methodDecl = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                    var currentMethodName = methodDecl != null ? methodDecl.Identifier.Text : "Unknown";
                    var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.RepositoriesMustNotCommitTransactionsRule, invocation.GetLocation(), classSymbol.Name, currentMethodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA2020: Synchronous DB/IO calls
            bool isSyncDbOrIo = (fullType == "System.IO.File" && (methodName.StartsWith("Read") || methodName.StartsWith("Write") || methodName.StartsWith("Append")) && !methodName.EndsWith("Async")) ||
                                (typeName.Contains("DbCommand") && methodName.StartsWith("Execute") && !methodName.EndsWith("Async"));
            if (isSyncDbOrIo)
            {
                var methodDecl = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                var currentMethodName = methodDecl != null ? methodDecl.Identifier.Text : "Unknown";
                var diagnostic = Diagnostic.Create(InfrastructureDiagnosticDescriptors.FileOperationsAsyncRule, invocation.GetLocation(), classSymbol.Name, currentMethodName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsEFMigration(INamedTypeSymbol symbol)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (current.Name.Equals("Migration") && current.ContainingAssembly?.Name == "Microsoft.EntityFrameworkCore.Relational")
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

        private static bool inheritsFromDbContext(INamedTypeSymbol symbol)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (current.Name.Equals("DbContext")) return true;
                current = current.BaseType;
            }
            return false;
        }

        private static bool IsHttpClientType(ITypeSymbol type)
        {
            if (type == null) return false;
            var name = type.Name;
            return name.Equals("HttpClient") || name.Equals("RestClient") || name.Equals("HttpWebRequest");
        }

        private static bool IsDatabaseConnectionOrContextType(ITypeSymbol type)
        {
            if (type == null) return false;
            var name = type.Name;
            var fullType = type.ToString();
            return name.Equals("DbContext") || name.Equals("DbConnection") || name.Equals("IDbConnection") || 
                   fullType.Contains("EntityFrameworkCore.DbContext") || fullType.Contains("System.Data.Common.DbConnection");
        }

        private static bool IsPersistenceModel(ITypeSymbol type)
        {
            if (type == null) return false;
            var ns = type.ContainingNamespace?.ToString();
            return ns != null && (ns.Contains("Persistence.Models") || ns.Contains("Infrastructure.Entities"));
        }
    }
}
