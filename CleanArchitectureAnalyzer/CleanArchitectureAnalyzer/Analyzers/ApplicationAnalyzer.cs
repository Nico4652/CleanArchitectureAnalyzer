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
    public class ApplicationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            ApplicationDiagnosticDescriptors.ApplicationReferencesInfrastructureRule,
            ApplicationDiagnosticDescriptors.ApplicationReferencesPresentationRule,
            ApplicationDiagnosticDescriptors.UseCaseImmutableRule,
            ApplicationDiagnosticDescriptors.UseCaseHandlerPublicMembersRule,
            ApplicationDiagnosticDescriptors.UseCaseResponseExposesDomainRule,
            ApplicationDiagnosticDescriptors.UseCaseRequestAcceptsDomainRule,
            ApplicationDiagnosticDescriptors.ApplicationServiceStatelessRule,
            ApplicationDiagnosticDescriptors.ValidatorLayerRule,
            ApplicationDiagnosticDescriptors.ApplicationExceptionSealedRule,
            ApplicationDiagnosticDescriptors.CommandNoBehaviorRule,
            ApplicationDiagnosticDescriptors.QueryNoBehaviorRule,
            ApplicationDiagnosticDescriptors.NoDirectFileSystemAccessRule,
            ApplicationDiagnosticDescriptors.NoDirectDateTimeAccessRule,
            ApplicationDiagnosticDescriptors.CommandNamingRule,
            ApplicationDiagnosticDescriptors.QueryNamingRule,
            ApplicationDiagnosticDescriptors.NoArbitraryDelaysRule,
            ApplicationDiagnosticDescriptors.EmptyCatchBlockRule,
            ApplicationDiagnosticDescriptors.DtoNoBehaviorRule,
            ApplicationDiagnosticDescriptors.NoDirectThirdPartySdkRule,
            ApplicationDiagnosticDescriptors.UseCaseHandlerSealedRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.TypeKind != TypeKind.Class && namedTypeSymbol.TypeKind != TypeKind.Struct && namedTypeSymbol.TypeKind != TypeKind.Interface)
            {
                return;
            }

            // CAA1008 Check Validators anywhere in the solution
            var isValidator = namedTypeSymbol.Name.EndsWith("Validator") || 
                              (namedTypeSymbol.BaseType != null && namedTypeSymbol.BaseType.Name.Contains("AbstractValidator"));
            if (isValidator)
            {
                if (!ArchitectureHelpers.IsApplication(context, namedTypeSymbol))
                {
                    var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.ValidatorLayerRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // From now on, only check types inside the Application layer
            if (!ArchitectureHelpers.IsApplication(context, namedTypeSymbol))
            {
                return;
            }

            var name = namedTypeSymbol.Name;
            var isCommand = name.EndsWith("Command") || name.EndsWith("CreateRequest") || name.EndsWith("UpdateRequest") || name.EndsWith("DeleteRequest");
            var isQuery = name.EndsWith("Query") || name.EndsWith("GetRequest") || name.EndsWith("ListRequest");
            var isRequest = name.EndsWith("Request") && !isCommand && !isQuery;
            var isDto = name.EndsWith("Dto") || name.EndsWith("ViewModel") || name.EndsWith("Response");
            var isUseCase = isCommand || isQuery || isRequest;

            var isHandler = name.EndsWith("Handler") || name.EndsWith("CommandHandler") || name.EndsWith("QueryHandler") ||
                            namedTypeSymbol.Interfaces.Any(i => i.Name.Contains("RequestHandler") || i.Name.Contains("CommandHandler") || i.Name.Contains("QueryHandler"));

            // CAA1020: Use case handlers should be sealed
            if (isHandler && namedTypeSymbol.TypeKind == TypeKind.Class && !namedTypeSymbol.IsSealed)
            {
                var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.UseCaseHandlerSealedRule, namedTypeSymbol.Locations[0], name);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA1003: Use case requests must be immutable
            if (isUseCase)
            {
                foreach (var prop in namedTypeSymbol.GetProperties())
                {
                    if (prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.UseCaseImmutableRule, prop.Locations[0], name, prop.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA1004: Use case handler should not expose public properties or fields
            if (isHandler)
            {
                foreach (var prop in namedTypeSymbol.GetProperties())
                {
                    if (prop.DeclaredAccessibility == Accessibility.Public && prop.SetMethod != null)
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.UseCaseHandlerPublicMembersRule, prop.Locations[0], name, prop.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                foreach (var field in namedTypeSymbol.GetFields())
                {
                    if (field.DeclaredAccessibility == Accessibility.Public && !field.IsReadOnly && !field.IsConst)
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.UseCaseHandlerPublicMembersRule, field.Locations[0], name, field.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA1005 & CAA1006: DTO / Request / Response should not accept/expose domain entities
            if (isUseCase || isDto)
            {
                foreach (var prop in namedTypeSymbol.GetProperties())
                {
                    if (IsDomainType(prop.Type, context))
                    {
                        var rule = isUseCase && (isCommand || isRequest) ? 
                            ApplicationDiagnosticDescriptors.UseCaseRequestAcceptsDomainRule : 
                            ApplicationDiagnosticDescriptors.UseCaseResponseExposesDomainRule;
                        var diagnostic = Diagnostic.Create(rule, prop.Locations[0], name, prop.Type.Name, prop.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA1007: Application services must be stateless
            var isService = name.EndsWith("Service") && !isHandler;
            if (isService && namedTypeSymbol.TypeKind == TypeKind.Class)
            {
                foreach (var field in namedTypeSymbol.GetFields())
                {
                    if (!field.IsReadOnly && !field.IsConst && field.DeclaredAccessibility == Accessibility.Public)
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.ApplicationServiceStatelessRule, field.Locations[0], name, field.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA1009: Application exceptions should be sealed
            var inheritsException = namedTypeSymbol.BaseType != null && 
                                    (namedTypeSymbol.BaseType.Name.Equals("Exception") || namedTypeSymbol.BaseType.Name.EndsWith("Exception"));
            if (inheritsException && !namedTypeSymbol.IsSealed)
            {
                var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.ApplicationExceptionSealedRule, namedTypeSymbol.Locations[0], name);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA1010, CAA1011, CAA1018: CQRS Commands, Queries, and DTOs should not contain business logic
            if (isUseCase || isDto)
            {
                var methods = namedTypeSymbol.GetBusinessMethods();
                foreach (var method in methods)
                {
                    if (method.DeclaredAccessibility == Accessibility.Public && !method.IsStatic)
                    {
                        var descriptor = isCommand ? ApplicationDiagnosticDescriptors.CommandNoBehaviorRule :
                                         isQuery ? ApplicationDiagnosticDescriptors.QueryNoBehaviorRule :
                                         ApplicationDiagnosticDescriptors.DtoNoBehaviorRule;
                        var diagnostic = Diagnostic.Create(descriptor, method.Locations[0], name, method.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // CAA1014: Command naming (must start with recognized verb)
            if (isCommand)
            {
                var verbs = new[] { "Create", "Update", "Delete", "Add", "Remove", "Complete", "Cancel", "Send", "Post", "Process", "Set", "Ship", "Publish", "Submit", "Register" };
                bool startsWithVerb = verbs.Any(v => name.StartsWith(v, StringComparison.OrdinalIgnoreCase));
                if (!startsWithVerb)
                {
                    var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.CommandNamingRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA1015: Query naming
            if (isQuery)
            {
                bool isValidQueryName = name.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
                                       name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) ||
                                       name.StartsWith("Find", StringComparison.OrdinalIgnoreCase) ||
                                       name.StartsWith("List", StringComparison.OrdinalIgnoreCase) ||
                                       name.StartsWith("Search", StringComparison.OrdinalIgnoreCase);
                if (!isValidQueryName)
                {
                    var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.QueryNamingRule, namedTypeSymbol.Locations[0], name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // CAA1001 & CAA1002 & CAA1019: References checks on Fields and Constructor parameters
            var currentAssembly = context.Compilation.Assembly;
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                ITypeSymbol memberType = null;
                Location memberLocation = null;

                if (member is IFieldSymbol field)
                {
                    memberType = field.Type;
                    memberLocation = field.Locations.FirstOrDefault();
                }
                else if (member is IPropertySymbol prop)
                {
                    memberType = prop.Type;
                    memberLocation = prop.Locations.FirstOrDefault();
                }
                else if (member is IMethodSymbol method && method.MethodKind == MethodKind.Constructor)
                {
                    foreach (var param in method.Parameters)
                    {
                        CheckTypeReferences(context, namedTypeSymbol, param.Type, param.Locations.FirstOrDefault());
                    }
                }

                if (memberType != null && memberLocation != null)
                {
                    CheckTypeReferences(context, namedTypeSymbol, memberType, memberLocation);
                }
            }
        }

        private static void CheckTypeReferences(SymbolAnalysisContext context, INamedTypeSymbol parentType, ITypeSymbol typeSymbol, Location location)
        {
            if (typeSymbol == null || location == null) return;

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        CheckTypeReferences(context, parentType, arg, location);
                    }
                }

                // Check third-party SDK dependencies (CAA1019)
                var ns = namedType.ContainingNamespace?.ToString();
                if (ns != null && (ns.StartsWith("Stripe", StringComparison.OrdinalIgnoreCase) || 
                                   ns.StartsWith("SendGrid", StringComparison.OrdinalIgnoreCase) || 
                                   ns.StartsWith("Amazon", StringComparison.OrdinalIgnoreCase) || 
                                   ns.StartsWith("Azure", StringComparison.OrdinalIgnoreCase) ||
                                   ns.StartsWith("Firebase", StringComparison.OrdinalIgnoreCase)))
                {
                    var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.NoDirectThirdPartySdkRule, location, parentType.Name, namedType.Name);
                    context.ReportDiagnostic(diagnostic);
                }

                // Check layer dependencies (CAA1001 & CAA1002)
                var assembly = namedType.ContainingAssembly;
                if (assembly != null)
                {
                    if (ArchitectureHelpers.IsInfrastructure(context, namedType))
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.ApplicationReferencesInfrastructureRule, location, parentType.Name, namedType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                    else if (IsPresentationAssembly(assembly.Name))
                    {
                        var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.ApplicationReferencesPresentationRule, location, parentType.Name, namedType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsPresentationAssembly(string assemblyName)
        {
            return assemblyName.Equals("Presentation", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Presentation", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.Equals("Web", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Web", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.Equals("Api", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Api", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDomainType(ITypeSymbol typeSymbol, SymbolAnalysisContext context)
        {
            if (typeSymbol is INamedTypeSymbol namedType)
            {
                return ArchitectureHelpers.IsDomain(context, namedType);
            }
            return false;
        }

        // CAA1013: DateTime checks
        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol != null && symbol.ContainingType != null && symbol.ContainingType.Name == "DateTime")
            {
                if (symbol.Name == "Now" || symbol.Name == "UtcNow")
                {
                    var containingClass = memberAccess.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    if (containingClass != null)
                    {
                        var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
                        if (classSymbol != null && ArchitectureHelpers.IsApplication(context, classSymbol))
                        {
                            var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.NoDirectDateTimeAccessRule, memberAccess.GetLocation(), classSymbol.Name, symbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        // CAA1017: Empty catch blocks in use case handlers
        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            if (catchClause.Block == null || catchClause.Block.Statements.Count == 0)
            {
                var containingClass = catchClause.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (containingClass != null)
                {
                    var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
                    if (classSymbol != null && ArchitectureHelpers.IsApplication(context, classSymbol))
                    {
                        var isHandler = classSymbol.Name.EndsWith("Handler") || classSymbol.Name.EndsWith("CommandHandler") || classSymbol.Name.EndsWith("QueryHandler");
                        if (isHandler)
                        {
                            var method = catchClause.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                            var methodName = method != null ? method.Identifier.Text : "Unknown";
                            var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.EmptyCatchBlockRule, catchClause.CatchKeyword.GetLocation(), classSymbol.Name, methodName);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        // CAA1012: Direct File System Access & CAA1016: No Task.Delay / Thread.Sleep
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null || symbol.ContainingType == null) return;

            var containingClass = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (containingClass == null) return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol == null || !ArchitectureHelpers.IsApplication(context, classSymbol)) return;

            var typeName = symbol.ContainingType.Name;
            var fullType = symbol.ContainingType.ToString();

            // CAA1012: System.IO.File or System.IO.Directory
            if (fullType == "System.IO.File" || fullType == "System.IO.Directory")
            {
                var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.NoDirectFileSystemAccessRule, invocation.GetLocation(), classSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            // CAA1016: Task.Delay or Thread.Sleep
            bool isDelay = (fullType == "System.Threading.Tasks.Task" && symbol.Name == "Delay") ||
                           (fullType == "System.Threading.Thread" && symbol.Name == "Sleep");
            if (isDelay)
            {
                var isHandler = classSymbol.Name.EndsWith("Handler") || classSymbol.Name.EndsWith("CommandHandler") || classSymbol.Name.EndsWith("QueryHandler");
                if (isHandler)
                {
                    var diagnostic = Diagnostic.Create(ApplicationDiagnosticDescriptors.NoArbitraryDelaysRule, invocation.GetLocation(), classSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
