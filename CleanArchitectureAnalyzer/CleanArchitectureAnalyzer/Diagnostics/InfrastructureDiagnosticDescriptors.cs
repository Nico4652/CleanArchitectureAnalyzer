using Microsoft.CodeAnalysis;
using System;

namespace CleanArchitectureAnalyzer.Diagnostics
{
    public class InfrastructureDiagnosticDescriptors
    {
        public readonly static DiagnosticDescriptor InfrastructureNoBusinessLogicRule = new DiagnosticDescriptor(
            id: "CAA2001",
            title: "Infrastructure classes must not contain core business logic",
            messageFormat: "Infrastructure class '{0}' implements business/domain logic pattern in '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Core business logic belongs in the Domain layer, not inside Infrastructure implementations."
        );

        public readonly static DiagnosticDescriptor InfrastructureClassSealedRule = new DiagnosticDescriptor(
            id: "CAA2002",
            title: "Infrastructure implementation classes should be sealed",
            messageFormat: "Infrastructure class '{0}' should be sealed",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Infrastructure implementations (like repositories, external services) should be sealed to prevent inheritance and enforce single responsibility."
        );

        public readonly static DiagnosticDescriptor DbContextNotExposedRule = new DiagnosticDescriptor(
            id: "CAA2003",
            title: "DbContext or DbConnection must not be exposed",
            messageFormat: "Infrastructure type '{0}' exposes DbContext/DbConnection in public member '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "To enforce encapsulation and prevent persistence leaks, database connection and context objects must remain internal or private."
        );

        public readonly static DiagnosticDescriptor RepositoriesInternalOrSealedRule = new DiagnosticDescriptor(
            id: "CAA2004",
            title: "Repositories must be internal or sealed",
            messageFormat: "Repository class '{0}' should be sealed or internal to prevent leaking implementations",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Repositories should only be accessed via their interfaces, so implementations should be internal or sealed."
        );

        public readonly static DiagnosticDescriptor DatabaseModelsNotExposedRule = new DiagnosticDescriptor(
            id: "CAA2005",
            title: "Database entities/persistence models must not be exposed outside Infrastructure",
            messageFormat: "Infrastructure service '{0}' exposes persistence model type '{1}' in public method/property '{2}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Database/ORM models must be mapped to domain entities or DTOs and not leaked to the application or domain layers."
        );

        public readonly static DiagnosticDescriptor ParameterizedSqlRule = new DiagnosticDescriptor(
            id: "CAA2006",
            title: "Avoid raw/interpolated SQL strings in Infrastructure without parametrization",
            messageFormat: "Infrastructure method '{0}.{1}' contains potential non-parameterized SQL execution",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid executing dynamic non-parameterized SQL queries to protect against SQL injection and performance issues."
        );

        public readonly static DiagnosticDescriptor RepositoriesNoValidationRule = new DiagnosticDescriptor(
            id: "CAA2007",
            title: "Repositories should not perform business validation",
            messageFormat: "Repository '{0}' contains business validation logic in method '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Repositories are solely responsible for data persistence, not validating domain rules or application invariants."
        );

        public readonly static DiagnosticDescriptor HttpClientsOnlyInInfrastructureRule = new DiagnosticDescriptor(
            id: "CAA2008",
            title: "HTTP Clients and External API calls must only reside in Infrastructure",
            messageFormat: "HTTP client / REST call '{0}' is used outside the Infrastructure layer",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All HTTP calls and external integration clients should be encapsulated inside the Infrastructure layer."
        );

        public readonly static DiagnosticDescriptor InfrastructureServiceThreadSafeRule = new DiagnosticDescriptor(
            id: "CAA2009",
            title: "Infrastructure services must be thread-safe",
            messageFormat: "Infrastructure class '{0}' exposes mutable instance field or property '{1}' which may cause race conditions",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Infrastructure services are typically registered as Singletons or Scoped and must be stateless/thread-safe."
        );

        public readonly static DiagnosticDescriptor NoHardcodedSecretsRule = new DiagnosticDescriptor(
            id: "CAA2010",
            title: "Connection strings or API secrets must not be hardcoded",
            messageFormat: "Infrastructure class '{0}' contains a hardcoded connection string or secret keyword in field/constant '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Secrets, passwords, and connection strings should be loaded via configuration providers, never hardcoded."
        );

        public readonly static DiagnosticDescriptor DbContextConstructorRule = new DiagnosticDescriptor(
            id: "CAA2011",
            title: "DbContext classes should inject DbContextOptions",
            messageFormat: "DbContext '{0}' does not expose a constructor accepting DbContextOptions",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DbContext subclasses should receive their configuration via constructor injection using DbContextOptions."
        );

        public readonly static DiagnosticDescriptor UseLoggerInsteadOfConsoleRule = new DiagnosticDescriptor(
            id: "CAA2012",
            title: "Use ILogger instead of System.Console",
            messageFormat: "Infrastructure type '{0}' uses Console.Write/WriteLine in method '{1}'; use ILogger instead",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "System.Console should not be used for logging; inject and use Microsoft.Extensions.Logging.ILogger."
        );

        public readonly static DiagnosticDescriptor RepositoryInterfaceRequirementRule = new DiagnosticDescriptor(
            id: "CAA2013",
            title: "Classes with Repository suffix must implement a repository interface",
            messageFormat: "Repository class '{0}' does not implement an interface with 'Repository' or 'Reader/Writer' suffix",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "To satisfy the Dependency Inversion Principle, all repository implementations must implement a corresponding abstraction interface."
        );

        public readonly static DiagnosticDescriptor InfrastructureSuffixRule = new DiagnosticDescriptor(
            id: "CAA2014",
            title: "Infrastructure classes should have recognized suffixes",
            messageFormat: "Infrastructure type '{0}' should end with a standard suffix (Repository, DbContext, Service, Client, Adapter, Provider, Factory)",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Infrastructure classes should be named clearly according to their role using standard suffixes."
        );

        public readonly static DiagnosticDescriptor MigrationsLayerRule = new DiagnosticDescriptor(
            id: "CAA2015",
            title: "Migrations must not be in Domain or Application layers",
            messageFormat: "Migration class '{0}' should reside in Infrastructure",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Database migrations are specific to the persistence mechanism and belong in Infrastructure or a database project."
        );

        public readonly static DiagnosticDescriptor ParameterlessConstructorRestrictionRule = new DiagnosticDescriptor(
            id: "CAA2016",
            title: "Infrastructure services must not expose parameterless constructors if dependencies exist",
            messageFormat: "Infrastructure service '{0}' has a public parameterless constructor but contains injection candidates",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prevent creation of services in an uninitialized state when they rely on dependency injection."
        );

        public readonly static DiagnosticDescriptor AsNoTrackingRequirementRule = new DiagnosticDescriptor(
            id: "CAA2017",
            title: "EF Core queries in read repositories should use AsNoTracking",
            messageFormat: "Read method '{0}.{1}' executes query without AsNoTracking()",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "For read-only operations, using AsNoTracking() improves performance and reduces memory footprint."
        );

        public readonly static DiagnosticDescriptor RepositoriesMustNotCommitTransactionsRule = new DiagnosticDescriptor(
            id: "CAA2018",
            title: "Repositories should not save or commit transactions internally",
            messageFormat: "Repository method '{0}.{1}' calls SaveChanges/Commit directly; delegate this to the Unit of Work",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "To support transactional boundaries across multiple repository calls, transaction commits should be managed by the Unit of Work."
        );

        public readonly static DiagnosticDescriptor InfrastructureDomainInterfaceRule = new DiagnosticDescriptor(
            id: "CAA2019",
            title: "Infrastructure classes must not implement Domain Event or Entity interfaces directly",
            messageFormat: "Infrastructure class '{0}' implements domain contract '{1}' which should only belong to Domain objects",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Infrastructure classes should not pose as domain aggregates, entities, or event contracts."
        );

        public readonly static DiagnosticDescriptor FileOperationsAsyncRule = new DiagnosticDescriptor(
            id: "CAA2020",
            title: "Database and I/O operations should be asynchronous",
            messageFormat: "Infrastructure method '{0}.{1}' performs synchronous I/O or DB execution; use Async methods",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All database queries and network/file I/O in modern systems should use asynchronous methods to prevent thread-pool starvation."
        );
    }
}
