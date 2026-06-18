using Microsoft.CodeAnalysis;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;


namespace CleanArchitectureAnalyzer.Diagnostics
{
    public class DomainDiagnosticDescriptors
    {
        //Errores generales de domain
        public readonly static DiagnosticDescriptor BadReferencesRule = new DiagnosticDescriptor(
           id: "CAA0001",
           title: "Domain elements depends on External Classes",
           messageFormat: "Domain elements '{0}' depends on External classes type '{1}'",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Domain elements should not depend on the implementations of other classes. Dependencies should be inward-pointing, in accordance with Clean Architecture principles."
        );

        public readonly static DiagnosticDescriptor ExternalInjectionRule = new DiagnosticDescriptor(
           id: "CAA0002",
           title: "Domain elements must not contain dependency injection in a constructor",
           messageFormat: "Domain elements '{0}' must not contain a constructor with dependency injection",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Domain elements must not contain a constructor with dependency injection, to ensure Dependencies should be inward-pointing."
       );

        public readonly static DiagnosticDescriptor ExposeCollectionRule = new DiagnosticDescriptor(
           id: "CAA0003",
           title: "Domain elements should not expose collections as List<T>",
           messageFormat: "Domain elements '{0}' should not expose collections as List<T>",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Domain elements should expose collections as IReadOnlyCollection<T> to preserve encapsulation."
        );

        public readonly static DiagnosticDescriptor ExternalInterfacesRule = new DiagnosticDescriptor(
           id: "CAA0004",
           title: "Domain elements should not depend on external interfaces",
           messageFormat: "Domain elements '{0}' should not depend on external interfaces",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Domain elements must not implement interfaces from infrastructure or application."
        );
        public readonly static DiagnosticDescriptor ExposeConstructorRule = new DiagnosticDescriptor(
          id: "CAA0005",
          title: "The entity should not expose public builders without parameters",
          messageFormat: "Entity '{0}' should not expose public builders without parameters",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Entities should not expose public parameterless constructors that allow creation in an invalid state."
        );
        public readonly static DiagnosticDescriptor EntityConstructorWithIdRule = new DiagnosticDescriptor(
           id: "CAA0006",
           title: "Entities should not expose public constructors accepting an ID",
           messageFormat: "Entity '{0}' exposes a public constructor that accepts an ID parameter",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Entities should not allow public manual assignment of their identity during instantiation to avoid breaking invariants."
        );
        public readonly static DiagnosticDescriptor EventHandlerLayerRule = new DiagnosticDescriptor(
            id: "CAA0007",
            title: "Domain event handlers should not be placed in the Domain layer",
            messageFormat: "Domain event handler '{0}' should not reside within the Domain layer",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Event handlers often trigger side effects (DB writes, external APIs) and should be placed in Application or Infrastructure."
        );

        //Errores específicos de entidades
        public readonly static DiagnosticDescriptor AnemicEntityRule = new DiagnosticDescriptor(
            id: "CAA0101",
            title: "Possible Anemic Entity",
            messageFormat: "Type '{0}' contains multiple properties but no business behavior",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Detects entities that contain data but no business behavior."
        );

        public readonly static DiagnosticDescriptor PublicSettersRule = new DiagnosticDescriptor(
            id: "CAA0102",
            title: "Entity with Public Setters",
            messageFormat: "Type '{0}' exposes public setters",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Entity exposes public setters."
        );

        public readonly static DiagnosticDescriptor RequiredIdRule = new DiagnosticDescriptor(
          id: "CAA0103",
          title: "Entity should have an Id",
          messageFormat: "Entity '{0}' should have an Id",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Domain entities should have an Id."
        );

        

        //Errores específicos de Value Objects
        public readonly static DiagnosticDescriptor ImmutabilityRule = new DiagnosticDescriptor(
           id: "CAA0201",
           title: "Value object should be immutable",
           messageFormat: "Value object '{0}' should be immutable",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "Value objects should be immutable to ensure consistency and thread-safety."
        );
        public readonly static DiagnosticDescriptor NotHaveIdRule = new DiagnosticDescriptor(
          id: "CAA0202",
          title: "Value object must not have an ID",
          messageFormat: "Value object '{0}' must not have an Id",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Value objects should not define their own identifier, as their identity is determined exclusively by the values ​​of their attributes."
        );
        public readonly static DiagnosticDescriptor ValueEqualityRule = new DiagnosticDescriptor(
          id: "CAA0203",
          title: "Value object should implement value equality",
          messageFormat: "Value object '{0}' should implement value equality",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Value objects should implement equality semantics based on their attribute values."
        );
        public readonly static DiagnosticDescriptor ContainCollectionsRule = new DiagnosticDescriptor(
          id: "CAA0204",
          title: "Value object should not contain collections",
          messageFormat: "Value object '{0}' should not contain collections of entities or aggregates",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Value objects should not contain collections of entities or aggregates."
        );
        public readonly static DiagnosticDescriptor ValueObjectIEquatableRule = new DiagnosticDescriptor(
            id: "CAA0205",
            title: "Value objects must implement IEquatable<T>",
            messageFormat: "Value object '{0}' should implement IEquatable<{0}>",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Value objects should implement IEquatable<T> to avoid boxing and improve comparison performance."
        );
        public readonly static DiagnosticDescriptor ReadOnlyCollectionsRule = new DiagnosticDescriptor(
          id: "CAA0206",
          title: "Value object collections must be ReadOnly",
          messageFormat: "Value object '{0}' should not contain collections of entities or aggregates",
          category: "Architecture",
          defaultSeverity: DiagnosticSeverity.Warning,
          isEnabledByDefault: true,
          description: "Value objects should not contain collections of entities or aggregates."
        );
        public readonly static DiagnosticDescriptor ValueObjectReferenceRule = new DiagnosticDescriptor(
            id: "CAA0207",
            title: "Value object must not reference entities or aggregate roots",
            messageFormat: "Value Object '{0}' references Entity or Aggregate Root '{1}' in property '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Value objects must be self-contained and should not hold references to objects with lifecycles (Entities or Aggregate Roots)."
        );



        //Errores específicos de Aggregates roots
        public readonly static DiagnosticDescriptor RootReferenceRule = new DiagnosticDescriptor(
         id: "CAA0301",
         title: "Aggregate root cannot have properties of a type other than aggregate root",
         messageFormat: "Aggregate root '{0}' cannot have properties of a type other than aggregate root",
         category: "Architecture",
         defaultSeverity: DiagnosticSeverity.Warning,
         isEnabledByDefault: true,
         description: "To maintain clear boundaries of consistency and transactionality, an Aggregate should not contain direct navigation references to entities that belong to other Aggregates."
        );
        public readonly static DiagnosticDescriptor RaiseEventsOnlyInRootRule = new DiagnosticDescriptor(
           id: "CAA0302",
           title: "Domain events should only be raised by Aggregate Roots",
           messageFormat: "Entity '{0}' cannot raise domain events directly; only Aggregate Roots are allowed to do so",
           category: "Architecture",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true,
           description: "To maintain consistency boundaries, only Aggregate Roots should be responsible for registering or raising domain events."
       );

        public readonly static DiagnosticDescriptor RootFactoryMethodRule = new DiagnosticDescriptor(
            id: "CAA0303",
            title: "Aggregate roots should use Factory methods instead of public constructors",
            messageFormat: "Aggregate Root '{0}' should not expose public constructors; use factory methods to enforce invariants",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Aggregate roots should enforce invariant creation through static factory methods and keep constructors non-public."
        );

        //Errores específicos de Domain Services
        public readonly static DiagnosticDescriptor StatelessDomainServiceRule = new DiagnosticDescriptor(
            id: "CAA0401",
            title: "Domain services should be stateless",
            messageFormat: "Domain service '{0}' should not contain mutable state",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services should represent behavior and not maintain mutable state between invocations."
        );

        public readonly static DiagnosticDescriptor DomainServiceMutablePropertyRule = new DiagnosticDescriptor(
            id: "CAA0402",
            title: "Domain services should not expose mutable properties",
            messageFormat: "Domain service '{0}' should not expose mutable property '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services must remain stateless, meaning they should not expose public properties with setters."
        );

        public readonly static DiagnosticDescriptor DomainServiceInterfaceRule = new DiagnosticDescriptor(
            id: "CAA0403",
            title: "Domain services should implement at least one interface",
            messageFormat: "Domain service '{0}' should implement at least one interface to represent its contract",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services should implement interfaces to facilitate dependency injection and mockability in tests."
        );

        public readonly static DiagnosticDescriptor DomainServiceStaticClassRule = new DiagnosticDescriptor(
            id: "CAA0404",
            title: "Domain services should not be static classes",
            messageFormat: "Domain service '{0}' should not be a static class",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services should not be static to allow dependency injection, polymorphism, and mockability."
        );

        public readonly static DiagnosticDescriptor DomainServiceDtoViewModelRule = new DiagnosticDescriptor(
            id: "CAA0405",
            title: "Domain service methods must not accept or return DTOs or ViewModels",
            messageFormat: "Domain service method '{0}.{1}' should not accept or return DTO/ViewModel type '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services should only operate on domain entities, value objects, and primitive types to maintain domain purity."
        );

        public readonly static DiagnosticDescriptor DomainServiceConcreteRepositoryRule = new DiagnosticDescriptor(
            id: "CAA0406",
            title: "Domain services must not depend on concrete repository implementations",
            messageFormat: "Domain service '{0}' depends on concrete repository '{1}' instead of its interface",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain services should only depend on repository interfaces (e.g. starting with I) rather than concrete implementations to adhere to dependency inversion."
        );

        //Errores específicos de Domain Events
        public readonly static DiagnosticDescriptor DomainEventImmutabilityRule = new DiagnosticDescriptor(
            id: "CAA0501",
            title: "Domain events must be immutable",
            messageFormat: "Domain event '{0}' should be immutable; property '{1}' exposes a setter",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain events represent facts that have occurred in the past and should not be modified after creation."
        );

        public readonly static DiagnosticDescriptor DomainEventTimestampRule = new DiagnosticDescriptor(
            id: "CAA0502",
            title: "Domain events must contain a timestamp property",
            messageFormat: "Domain event '{0}' must contain a timestamp property indicating when it occurred",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Every domain event must have a property of type DateTime or DateTimeOffset representing the occurrence timestamp (e.g. OccurredOn, Timestamp, or CreatedAt)."
        );

        public readonly static DiagnosticDescriptor DomainEventReferenceRule = new DiagnosticDescriptor(
            id: "CAA0503",
            title: "Domain events must not reference mutable entities or aggregate roots directly",
            messageFormat: "Domain event '{0}' references Entity or Aggregate Root '{1}' in property or field '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain events should be lightweight and serializable; they should reference identifiers (IDs) instead of referencing mutable entity/aggregate root instances directly."
        );

        public readonly static DiagnosticDescriptor DomainEventBusinessLogicRule = new DiagnosticDescriptor(
            id: "CAA0504",
            title: "Domain events should not contain business logic methods",
            messageFormat: "Domain event '{0}' contains business method '{1}'; events should only contain data",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain events are data carriers that report occurrences. They should not contain ordinary business methods or logic."
        );

        public readonly static DiagnosticDescriptor DomainEventNameRule = new DiagnosticDescriptor(
            id: "CAA0505",
            title: "Domain events should end with Event or be named in the past tense",
            messageFormat: "Domain event '{0}' should end with 'Event' or use a past tense name",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Domain event names should end with 'Event' or be named using past tense (e.g., Created, Updated, Deleted, Completed) to denote past facts."
        );
        

        
    }
}
