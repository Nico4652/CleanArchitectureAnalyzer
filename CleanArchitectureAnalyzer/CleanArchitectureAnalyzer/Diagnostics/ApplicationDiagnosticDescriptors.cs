using Microsoft.CodeAnalysis;
using System;

namespace CleanArchitectureAnalyzer.Diagnostics
{
    public class ApplicationDiagnosticDescriptors
    {
        public readonly static DiagnosticDescriptor ApplicationReferencesInfrastructureRule = new DiagnosticDescriptor(
            id: "CAA1001",
            title: "Application layer must not reference Infrastructure",
            messageFormat: "Application type '{0}' references Infrastructure type '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The Application layer must be independent of the Infrastructure layer. Dependencies should point inwards towards the Domain."
        );

        public readonly static DiagnosticDescriptor ApplicationReferencesPresentationRule = new DiagnosticDescriptor(
            id: "CAA1002",
            title: "Application layer must not reference Presentation",
            messageFormat: "Application type '{0}' references Presentation type '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The Application layer must not depend on the Presentation layer (e.g., Controllers, Views, API endpoints)."
        );

        public readonly static DiagnosticDescriptor UseCaseImmutableRule = new DiagnosticDescriptor(
            id: "CAA1003",
            title: "Use case request must be immutable",
            messageFormat: "Use case request '{0}' property '{1}' has a public setter or is mutable",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use case requests (Commands/Queries) should be immutable to guarantee thread safety and represent data snapshots."
        );

        public readonly static DiagnosticDescriptor UseCaseHandlerPublicMembersRule = new DiagnosticDescriptor(
            id: "CAA1004",
            title: "Use case handler should not expose public properties or fields",
            messageFormat: "Use case handler '{0}' exposes public member '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use case handlers should encapsulate their state and only expose the handler entry point."
        );

        public readonly static DiagnosticDescriptor UseCaseResponseExposesDomainRule = new DiagnosticDescriptor(
            id: "CAA1005",
            title: "Use case output must not expose Domain Entities or Aggregate Roots",
            messageFormat: "Use case response/DTO '{0}' exposes Domain type '{1}' in member '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use case responses and DTOs should not expose Domain models directly to avoid leaking Domain representation to external layers."
        );

        public readonly static DiagnosticDescriptor UseCaseRequestAcceptsDomainRule = new DiagnosticDescriptor(
            id: "CAA1006",
            title: "Use case input must not accept Domain Entities or Aggregate Roots",
            messageFormat: "Use case request/DTO '{0}' accepts Domain type '{1}' in member '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use case requests should only accept primitives, value objects or DTOs, not Domain entities directly."
        );

        public readonly static DiagnosticDescriptor ApplicationServiceStatelessRule = new DiagnosticDescriptor(
            id: "CAA1007",
            title: "Application services must be stateless",
            messageFormat: "Application service '{0}' has mutable field/property '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Application services should represent stateless logic and should not maintain mutable state between requests."
        );

        public readonly static DiagnosticDescriptor ValidatorLayerRule = new DiagnosticDescriptor(
            id: "CAA1008",
            title: "Validators must reside in the Application layer",
            messageFormat: "Validator type '{0}' must be defined in the Application layer",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Input and request validators (e.g. FluentValidation) belong in the Application layer where the use cases reside."
        );

        public readonly static DiagnosticDescriptor ApplicationExceptionSealedRule = new DiagnosticDescriptor(
            id: "CAA1009",
            title: "Application exceptions should be sealed",
            messageFormat: "Application exception '{0}' should be sealed",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Application-specific exceptions should be sealed to prevent inheritance unless there is a strong architectural reason."
        );

        public readonly static DiagnosticDescriptor CommandNoBehaviorRule = new DiagnosticDescriptor(
            id: "CAA1010",
            title: "CQRS Commands should not contain business logic or behavior",
            messageFormat: "Command '{0}' contains business method '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CQRS Command objects are DTOs and should only carry data, not behavior."
        );

        public readonly static DiagnosticDescriptor QueryNoBehaviorRule = new DiagnosticDescriptor(
            id: "CAA1011",
            title: "CQRS Queries should not contain business logic or behavior",
            messageFormat: "Query '{0}' contains business method '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CQRS Query objects are DTOs and should only carry data, not behavior."
        );

        public readonly static DiagnosticDescriptor NoDirectFileSystemAccessRule = new DiagnosticDescriptor(
            id: "CAA1012",
            title: "Application layer should not access the file system directly",
            messageFormat: "Application layer uses direct File/Directory access in '{0}'; use an abstraction instead",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Application layer should remain independent of I/O details; use an abstraction for file operations."
        );

        public readonly static DiagnosticDescriptor NoDirectDateTimeAccessRule = new DiagnosticDescriptor(
            id: "CAA1013",
            title: "Application layer should not use DateTime.Now or DateTime.UtcNow directly",
            messageFormat: "Application layer uses DateTime.{1} directly in '{0}'; use an IDateTimeProvider abstraction instead",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use an IDateTimeProvider or similar abstraction to allow deterministic testing and decouple from system time."
        );

        public readonly static DiagnosticDescriptor CommandNamingRule = new DiagnosticDescriptor(
            id: "CAA1014",
            title: "Command name should start with a verb",
            messageFormat: "Command name '{0}' does not start with a recognized verb (e.g., Create, Update, Delete, Ship, Add, Remove)",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Commands represent actions and their names should start with a verb indicating the action."
        );

        public readonly static DiagnosticDescriptor QueryNamingRule = new DiagnosticDescriptor(
            id: "CAA1015",
            title: "Query name should end with Query or start with Get/Find/List/Search",
            messageFormat: "Query name '{0}' should start with Get/Find/List/Search or end with Query",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Queries request data and should be named appropriately to convey retrieval."
        );

        public readonly static DiagnosticDescriptor NoArbitraryDelaysRule = new DiagnosticDescriptor(
            id: "CAA1016",
            title: "Do not use Task.Delay or Thread.Sleep inside Use Cases",
            messageFormat: "Use case handler '{0}' uses arbitrary delays or sleeps",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Thread.Sleep or Task.Delay should not be used inside application layer handlers to control flow."
        );

        public readonly static DiagnosticDescriptor EmptyCatchBlockRule = new DiagnosticDescriptor(
            id: "CAA1017",
            title: "Use case handlers must not contain empty catch blocks",
            messageFormat: "Use case handler '{0}' has an empty catch block in method '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Swallowing exceptions silently in application handlers hides bugs and critical errors."
        );

        public readonly static DiagnosticDescriptor DtoNoBehaviorRule = new DiagnosticDescriptor(
            id: "CAA1018",
            title: "DTO classes should not contain behavior/business logic methods",
            messageFormat: "DTO class '{0}' contains business method '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DTOs are intended for data transfer and should not implement business rules or behavior."
        );

        public readonly static DiagnosticDescriptor NoDirectThirdPartySdkRule = new DiagnosticDescriptor(
            id: "CAA1019",
            title: "Application layer must not reference third-party SDK clients directly",
            messageFormat: "Application class '{0}' references third-party client type '{1}' directly; use an abstraction",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Direct dependency on external payment, email, or cloud SDK clients couples the Application layer to third parties."
        );

        public readonly static DiagnosticDescriptor UseCaseHandlerSealedRule = new DiagnosticDescriptor(
            id: "CAA1020",
            title: "Use case handlers should be sealed",
            messageFormat: "Use case handler '{0}' should be declared as sealed",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use case handlers represent specific business execution flows and should be sealed to prevent inheritance."
        );
    }
}
