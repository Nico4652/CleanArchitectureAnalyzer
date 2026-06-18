using CleanArchitectureAnalyzer.Constants;
using CleanArchitectureAnalyzer.Diagnostics;
using CleanArchitectureAnalyzer.Extensions;
using CleanArchitectureAnalyzer.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CleanArchitectureAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DomainAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
                                                                                                            DomainDiagnosticDescriptors.AnemicEntityRule, 
                                                                                                            DomainDiagnosticDescriptors.PublicSettersRule, 
                                                                                                            DomainDiagnosticDescriptors.BadReferencesRule, 
                                                                                                            DomainDiagnosticDescriptors.RequiredIdRule, 
                                                                                                            DomainDiagnosticDescriptors.ExternalInjectionRule, 
                                                                                                            DomainDiagnosticDescriptors.ExposeCollectionRule,
                                                                                                            DomainDiagnosticDescriptors.ExternalInterfacesRule,
                                                                                                            DomainDiagnosticDescriptors.ImmutabilityRule,
                                                                                                            DomainDiagnosticDescriptors.NotHaveIdRule,
                                                                                                            DomainDiagnosticDescriptors.RootReferenceRule,
                                                                                                            DomainDiagnosticDescriptors.ExposeConstructorRule,
                                                                                                            DomainDiagnosticDescriptors.ValueEqualityRule,
                                                                                                            DomainDiagnosticDescriptors.ContainCollectionsRule,
                                                                                                            DomainDiagnosticDescriptors.StatelessDomainServiceRule,
                                                                                                            DomainDiagnosticDescriptors.EntityConstructorWithIdRule,
                                                                                                            DomainDiagnosticDescriptors.ValueObjectIEquatableRule,
                                                                                                            DomainDiagnosticDescriptors.RaiseEventsOnlyInRootRule,
                                                                                                            DomainDiagnosticDescriptors.RootFactoryMethodRule,
                                                                                                            DomainDiagnosticDescriptors.EventHandlerLayerRule,
                                                                                                            DomainDiagnosticDescriptors.ValueObjectReferenceRule,
                                                                                                            DomainDiagnosticDescriptors.DomainServiceMutablePropertyRule,
                                                                                                            DomainDiagnosticDescriptors.DomainServiceInterfaceRule,
                                                                                                            DomainDiagnosticDescriptors.DomainServiceStaticClassRule,
                                                                                                            DomainDiagnosticDescriptors.DomainServiceDtoViewModelRule,
                                                                                                            DomainDiagnosticDescriptors.DomainServiceConcreteRepositoryRule,
                                                                                                            DomainDiagnosticDescriptors.DomainEventImmutabilityRule,
                                                                                                            DomainDiagnosticDescriptors.DomainEventTimestampRule,
                                                                                                            DomainDiagnosticDescriptors.DomainEventReferenceRule,
                                                                                                            DomainDiagnosticDescriptors.DomainEventBusinessLogicRule,
                                                                                                            DomainDiagnosticDescriptors.DomainEventNameRule);

        public override void Initialize(
            AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);

            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(
                AnalyzeSymbol,
                SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(
       SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            
            // Verificamos de forma profesional: debe ser una clase y estar dentro del dominio
            if (namedTypeSymbol.TypeKind != TypeKind.Class || 
                !ArchitectureHelpers.IsDomain(context, namedTypeSymbol))
            {
                return;
            }

            var res = DomainTypeHelper.Type(namedTypeSymbol);
            
            // Si es estático y no es un Domain Service, lo descartamos
            if (namedTypeSymbol.IsStatic && res != DomainType.Service)
            {
                return;
            }

            switch (res)
            {
                case DomainType.Entity:
                    AnalyzeAnemicEntity(namedTypeSymbol,context);
                    AnalyzePublicSetters(namedTypeSymbol,context);
                    AnalyzeIdRequired(namedTypeSymbol,context);
                    AnalyzeRaiseEventsOnlyInRoot(namedTypeSymbol, context);
                    AnalyzeConstructorWithId(namedTypeSymbol, context);
                    break;
                case DomainType.Object:
                    AnalyzeImmutabilityObjets(namedTypeSymbol, context);
                    AnalyzeNotHaveId(namedTypeSymbol, context);
                    AnalyzeValueEquality(namedTypeSymbol, context);
                    AnalyzeContainCollections(namedTypeSymbol, context);
                    AnalyzeValueObjectIEquatable(namedTypeSymbol, context);
                    AnalyzeRaiseEventsOnlyInRoot(namedTypeSymbol, context);
                    AnalyzeValueObjectReference(namedTypeSymbol, context);
                    break;
                case DomainType.Root:
                    AnalyzeAnemicEntity(namedTypeSymbol, context);
                    AnalyzePublicSetters(namedTypeSymbol, context);
                    AnalyzeIdRequired(namedTypeSymbol, context);
                    AnalyzeRootReference(namedTypeSymbol, context);
                    AnalyzeConstructorWithId(namedTypeSymbol, context);
                    AnalyzeRaiseEventsOnlyInRoot(namedTypeSymbol, context);
                    AnalyzeRootFactoryMethod(namedTypeSymbol, context);
                    break;
                case DomainType.Service:
                    AnalyzeStatelessDomainService(namedTypeSymbol, context);
                    AnalyzeRaiseEventsOnlyInRoot(namedTypeSymbol, context);
                    AnalyzeDomainServiceStaticClass(namedTypeSymbol, context);
                    AnalyzeDomainServiceMutableProperties(namedTypeSymbol, context);
                    AnalyzeDomainServiceInterface(namedTypeSymbol, context);
                    AnalyzeDomainServiceDtoViewModel(namedTypeSymbol, context);
                    AnalyzeDomainServiceConcreteRepository(namedTypeSymbol, context);
                    break;
                case DomainType.Event:
                    AnalyzeDomainEventImmutability(namedTypeSymbol, context);
                    AnalyzeDomainEventTimestamp(namedTypeSymbol, context);
                    AnalyzeDomainEventReference(namedTypeSymbol, context);
                    AnalyzeDomainEventBusinessLogic(namedTypeSymbol, context);
                    AnalyzeDomainEventName(namedTypeSymbol, context);
                    break;
            }

            AnalyzeEventHandlerLayer(
                namedTypeSymbol,
                context);

            AnalyzeExposeConstructors(
                namedTypeSymbol,
                context);

            AnalyzeBadReferences(
                namedTypeSymbol,
                context);
            
            AnalyzeExternalInjection(
                namedTypeSymbol,
                context);
            AnalyzeExposeCollection(
                namedTypeSymbol,
                context);
            AnalyzeExternalInterfaces(
                namedTypeSymbol,
                context);
        }


        //Generic domain rules
        private static void AnalyzeBadReferences(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var badreferences = namedTypeSymbol.GetFields();
            var currentAssembly = context.Compilation.Assembly;

            foreach (var field in badreferences)
            {
                if (ReferenceHelper.isExternalReference(field.Type, currentAssembly))
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.BadReferencesRule, field.Locations[0], namedTypeSymbol.Name, field.Type.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeExternalInjection(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var contructors = namedTypeSymbol.GetConstructors();
            var currentAssembly = context.Compilation.Assembly;
            foreach (var contructor in contructors)
            {
                var parameters = contructor.Parameters;
                bool hasExternalParameters = parameters.Any(p => ReferenceHelper.isExternalReference(p.Type, currentAssembly));
                if (hasExternalParameters)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ExternalInjectionRule, contructor.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeExposeCollection(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (property.Type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.DeclaredAccessibility == Accessibility.Public)
                {
                    var genericType = namedType.ConstructedFrom;
                    if (genericType.ToString() == "System.Collections.Generic.IEnumerable<T>" ||
                        genericType.ToString() == "System.Collections.Generic.ICollection<T>" ||
                        genericType.ToString() == "System.Collections.Generic.List<T>")
                    {
                        var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ExposeCollectionRule, property.Locations[0], namedTypeSymbol.Name, property.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
        private static void AnalyzeExternalInterfaces(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var interfaces = namedTypeSymbol.GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (ReferenceHelper.isExternalReference(iface, context.Compilation.Assembly))
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ExternalInterfacesRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name, iface.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeConstructorWithId(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var constructors = namedTypeSymbol.GetConstructors();

            foreach (var constructor in constructors)
            {
                if(constructor.DeclaredAccessibility == Accessibility.Public)
                {
                    var parameters = constructor.Parameters;
                    bool hasIdParameter = parameters.Any(p => p.Type.Name.Equals("Guid", StringComparison.OrdinalIgnoreCase))||(parameters.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) 
                                            || p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) 
                                            || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)));
                    if (hasIdParameter)
                    {
                        var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.EntityConstructorWithIdRule, constructor.Locations[0], namedTypeSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
        private static void AnalyzeExposeConstructors(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            if(DomainTypeHelper.Type(namedTypeSymbol) == DomainType.Root)
                return;
            var constructors = namedTypeSymbol.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (constructor.IsImplicitlyDeclared)
                    continue;
                if (constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length <= 0)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ExposeConstructorRule, constructor.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeEventHandlerLayer(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            
            var interfaces = namedTypeSymbol.GetInterfaces();
            foreach(var interfaceSymbol in interfaces)
            {
                var isHandler = interfaceSymbol.Name.EndsWith("Handler") || interfaceSymbol.Name.EndsWith("EventHandler") || interfaceSymbol.Name.EndsWith("NotificationHandler");
                if (isHandler)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.EventHandlerLayerRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }

        //Domain entity rules
        private static void AnalyzeAnemicEntity(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var businessMethods = namedTypeSymbol.GetBusinessMethods().Count();
            var properties = namedTypeSymbol.GetProperties().Count();


            if (properties >= 4 && businessMethods == 0)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.AnemicEntityRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
        private static void AnalyzePublicSetters(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var businessMethods = namedTypeSymbol.GetBusinessMethods();
            var properties = namedTypeSymbol.GetProperties();

            foreach (var property in properties)
            {
                bool hasPublicSetter = property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;

                if (hasPublicSetter)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.PublicSettersRule, property.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeIdRequired(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var propertys = namedTypeSymbol.GetProperties();
            var hasId = propertys.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
            
            if(!hasId)
            {
                var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.RequiredIdRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        

        //Domain object rules
        private static void AnalyzeImmutabilityObjets(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();

            foreach (var property in properties)
            {
                bool hasSetter = property.SetMethod != null ;
                if (hasSetter)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ImmutabilityRule, property.Locations[0], namedTypeSymbol.Name, property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        private static void AnalyzeNotHaveId(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            var hasId = properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
            if (hasId)
            {
                var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.NotHaveIdRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            
        }
        private static void AnalyzeValueEquality(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var methods = namedTypeSymbol.GetBusinessMethods();
            bool hasEquals = methods.Any(m => m.IsOverride && (m.Name == "Equals" || m.Name == "GetHashCode"));
            if (!hasEquals)
            {
                var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ValueEqualityRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        private static void AnalyzeContainCollections(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
           
            
            var properties = namedTypeSymbol.GetProperties();
            
            foreach (var property in properties)
            {
                if (property.Type is INamedTypeSymbol propertyType && propertyType.IsGenericType)
                {
                    if (ReferenceHelper.isExternalReference(propertyType, context.Compilation.Assembly))
                    {
                        return;
                    }
                    var isBadCollection = ReferenceHelper.TypeReference(propertyType,  DomainType.Object);
                    if ( isBadCollection)
                    {
                        var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ContainCollectionsRule, property.Locations[0], namedTypeSymbol.Name, property.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
        private static void AnalyzeValueObjectReference(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (property.Type is INamedTypeSymbol propertyType && !propertyType.IsGenericType)
                {
                    if (ReferenceHelper.isExternalReference(propertyType, context.Compilation.Assembly))
                    {
                        return;
                    }
                    var isBadCollection = ReferenceHelper.TypeReference(propertyType, DomainType.Object);
                    if (isBadCollection)
                    {
                        var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ValueObjectReferenceRule, property.Locations[0], namedTypeSymbol.Name, property.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static void AnalyzeValueObjectIEquatable(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var interfaces = namedTypeSymbol.GetInterfaces();
            bool implementsIEquatable = interfaces.Any(i => i.Name == "IEquatable" && i.IsGenericType && SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], namedTypeSymbol));
            if (!implementsIEquatable)
            {
                var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.ValueObjectIEquatableRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }


        //Domain root rules
        private static void AnalyzeRootReference(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (property.Type is INamedTypeSymbol propertyType)
                {
                    var res = DomainTypeHelper.Type(propertyType) == DomainType.Root;
                    if (res)
                    {
                        var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.RootReferenceRule, property.Locations[0], namedTypeSymbol.Name, property.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                
            }
        }
        private static void AnalyzeRaiseEventsOnlyInRoot(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var methods = namedTypeSymbol.GetBusinessMethods();

            foreach (var method in methods)
            {
                if((method.Name.Equals("Event", StringComparison.OrdinalIgnoreCase) 
                      || method.Name.EndsWith("Event", StringComparison.OrdinalIgnoreCase) 
                      || method.Name.StartsWith("Event", StringComparison.OrdinalIgnoreCase)))
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.RaiseEventsOnlyInRootRule, method.Locations[0], namedTypeSymbol.Name, method.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                


            }

        }
        private static void AnalyzeRootFactoryMethod(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var constructors = namedTypeSymbol.GetConstructors();
            foreach (var constructor in constructors)
            {
                if(constructor.IsImplicitlyDeclared)
                    continue;
                if (constructor.DeclaredAccessibility == Accessibility.Public)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.RootFactoryMethodRule, constructor.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }


        //Domain service rules
        private static void AnalyzeStatelessDomainService(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
           var fields = namedTypeSymbol.GetFields();
            foreach (var field in fields)
            {
                if (field.IsImplicitlyDeclared)
                    continue;
                if(!field.IsReadOnly ||  field.DeclaredAccessibility == Accessibility.Public)
                {
                    var diagnostic = Diagnostic.Create(DomainDiagnosticDescriptors.StatelessDomainServiceRule, field.Locations[0], namedTypeSymbol.Name, field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }

        private static void AnalyzeDomainServiceMutableProperties(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (property.SetMethod != null && property.DeclaredAccessibility == Accessibility.Public)
                {
                    var diagnostic = Diagnostic.Create(
                        DomainDiagnosticDescriptors.DomainServiceMutablePropertyRule,
                        property.Locations[0],
                        namedTypeSymbol.Name,
                        property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeDomainServiceInterface(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            if (namedTypeSymbol.IsStatic)
                return;

            var interfaces = namedTypeSymbol.GetInterfaces();
            if (!interfaces.Any())
            {
                var diagnostic = Diagnostic.Create(
                    DomainDiagnosticDescriptors.DomainServiceInterfaceRule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeDomainServiceStaticClass(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            if (namedTypeSymbol.IsStatic)
            {
                var diagnostic = Diagnostic.Create(
                    DomainDiagnosticDescriptors.DomainServiceStaticClassRule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeDomainServiceDtoViewModel(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var methods = namedTypeSymbol.GetBusinessMethods();
            foreach (var method in methods)
            {
                // Check return type
                if (IsDtoOrViewModel(method.ReturnType, out string returnTypeName))
                {
                    var diagnostic = Diagnostic.Create(
                        DomainDiagnosticDescriptors.DomainServiceDtoViewModelRule,
                        method.Locations[0],
                        namedTypeSymbol.Name,
                        method.Name,
                        returnTypeName);
                    context.ReportDiagnostic(diagnostic);
                }

                // Check parameters
                foreach (var parameter in method.Parameters)
                {
                    if (IsDtoOrViewModel(parameter.Type, out string paramTypeName))
                    {
                        var diagnostic = Diagnostic.Create(
                            DomainDiagnosticDescriptors.DomainServiceDtoViewModelRule,
                            parameter.Locations[0],
                            namedTypeSymbol.Name,
                            method.Name,
                            paramTypeName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsDtoOrViewModel(ITypeSymbol typeSymbol, out string typeName)
        {
            typeName = string.Empty;
            if (typeSymbol == null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                foreach (var arg in namedType.TypeArguments)
                {
                    if (IsDtoOrViewModel(arg, out typeName))
                    {
                        return true;
                    }
                }
            }

            var name = typeSymbol.Name;
            if (name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Request", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Response", StringComparison.OrdinalIgnoreCase))
            {
                typeName = name;
                return true;
            }

            return false;
        }

        private static void AnalyzeDomainServiceConcreteRepository(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var constructors = namedTypeSymbol.GetConstructors();
            foreach (var constructor in constructors)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    var paramType = parameter.Type;
                    var name = paramType.Name;
                    if (name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("I", StringComparison.OrdinalIgnoreCase))
                    {
                        var diagnostic = Diagnostic.Create(
                            DomainDiagnosticDescriptors.DomainServiceConcreteRepositoryRule,
                            parameter.Locations[0],
                            namedTypeSymbol.Name,
                            name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        // Domain event rules
        private static void AnalyzeDomainEventImmutability(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (property.SetMethod != null)
                {
                    var diagnostic = Diagnostic.Create(
                        DomainDiagnosticDescriptors.DomainEventImmutabilityRule,
                        property.Locations[0],
                        namedTypeSymbol.Name,
                        property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeDomainEventTimestamp(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            bool hasTimestamp = false;
            foreach (var property in properties)
            {
                var typeName = property.Type.Name;
                bool isTimeType = typeName.Equals("DateTime", StringComparison.OrdinalIgnoreCase) ||
                                  typeName.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase);

                if (isTimeType)
                {
                    var name = property.Name;
                    if (name.Equals("OccurredOn", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Timestamp", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase))
                    {
                        hasTimestamp = true;
                        break;
                    }
                }
            }

            if (!hasTimestamp)
            {
                var diagnostic = Diagnostic.Create(
                    DomainDiagnosticDescriptors.DomainEventTimestampRule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeDomainEventReference(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var properties = namedTypeSymbol.GetProperties();
            foreach (var property in properties)
            {
                if (IsEntityOrAggregateRoot(property.Type, out string typeName))
                {
                    var diagnostic = Diagnostic.Create(
                        DomainDiagnosticDescriptors.DomainEventReferenceRule,
                        property.Locations[0],
                        namedTypeSymbol.Name,
                        typeName,
                        property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            var fields = namedTypeSymbol.GetFields();
            foreach (var field in fields)
            {
                if (field.IsImplicitlyDeclared)
                    continue;
                if (IsEntityOrAggregateRoot(field.Type, out string typeName))
                {
                    var diagnostic = Diagnostic.Create(
                        DomainDiagnosticDescriptors.DomainEventReferenceRule,
                        field.Locations[0],
                        namedTypeSymbol.Name,
                        typeName,
                        field.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsEntityOrAggregateRoot(ITypeSymbol typeSymbol, out string typeName)
        {
            typeName = string.Empty;
            if (typeSymbol == null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        if (IsEntityOrAggregateRoot(arg, out typeName))
                        {
                            return true;
                        }
                    }
                }

                var type = DomainTypeHelper.Type(namedType);
                if (type == DomainType.Entity || type == DomainType.Root)
                {
                    typeName = namedType.Name;
                    return true;
                }
            }
            return false;
        }

        private static void AnalyzeDomainEventBusinessLogic(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var businessMethods = namedTypeSymbol.GetBusinessMethods();
            foreach (var method in businessMethods)
            {
                var diagnostic = Diagnostic.Create(
                    DomainDiagnosticDescriptors.DomainEventBusinessLogicRule,
                    method.Locations[0],
                    namedTypeSymbol.Name,
                    method.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeDomainEventName(INamedTypeSymbol namedTypeSymbol, SymbolAnalysisContext context)
        {
            var name = namedTypeSymbol.Name;
            bool validName = name.EndsWith("Event", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Created", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Updated", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Deleted", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Added", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Removed", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Sent", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Received", StringComparison.OrdinalIgnoreCase) ||
                             name.EndsWith("Processed", StringComparison.OrdinalIgnoreCase);

            if (!validName)
            {
                var diagnostic = Diagnostic.Create(
                    DomainDiagnosticDescriptors.DomainEventNameRule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }



    }
}
