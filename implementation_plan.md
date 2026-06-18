# Add Clean Architecture Analyzers for Application and Infrastructure Layers

This plan outlines the design and implementation of 40+ static analysis rules (at least 20 for the Application layer and 20 for the Infrastructure layer) to enforce Clean Architecture principles professionally.

## Proposed Changes

### Core Analyzer Project

#### [MODIFY] [ArchitectureHelpers.cs](file:///c:/Users/HP/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/Helpers/ArchitectureHelpers.cs)
- Add `IsApplication` and `IsInfrastructure` helper methods matching the pattern of `IsDomain`.
- Read from `.editorconfig` options `cleanarchitecture_application_assembly` and `cleanarchitecture_infrastructure_assembly`, falling back to suffix/exact matches on `.Application` and `.Infrastructure`.

#### [NEW] [ApplicationDiagnosticDescriptors.cs](file:///c:/Users/HP/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/Diagnostics/ApplicationDiagnosticDescriptors.cs)
Define 20 rules (`CAA1001` - `CAA1020`):
1. **CAA1001**: Application layer must not reference/depend on Infrastructure.
2. **CAA1002**: Application layer must not reference Presentation.
3. **CAA1003**: Use case requests (Commands/Queries/Requests) must be immutable (no public setters).
4. **CAA1004**: Use case handlers must not expose public fields or properties (except injected dependencies or contracts).
5. **CAA1005**: Use case responses/DTOs must not expose Domain Entities/Aggregate Roots.
6. **CAA1006**: Use case requests/DTOs must not accept Domain Entities/Aggregate Roots.
7. **CAA1007**: Application services must be stateless (fields must be readonly).
8. **CAA1008**: Input validation (FluentValidation) must reside in Application.
9. **CAA1009**: Application exceptions should be sealed.
10. **CAA1010**: CQRS Commands should be public and not have behavior.
11. **CAA1011**: CQRS Queries should be public and not have behavior.
12. **CAA1012**: No direct file system access (e.g. `System.IO.File` or `System.IO.Directory`) - use an abstraction.
13. **CAA1013**: No direct use of `DateTime.Now`/`UtcNow` - use an abstraction.
14. **CAA1014**: Command names should start with a verb (e.g., Create, Update, Delete).
15. **CAA1015**: Query names should end with "Query" or start with "Get", "Find", "List", "Search".
16. **CAA1016**: No use of `Task.Delay` or `Thread.Sleep` in handlers.
17. **CAA1017**: Use case handlers should not have empty catch blocks.
18. **CAA1018**: DTO classes should not contain business logic methods.
19. **CAA1019**: Direct dependency on external third-party SDK clients (e.g., StripeClient) is prohibited.
20. **CAA1020**: Use case handlers should be declared as `sealed`.

#### [NEW] [InfrastructureDiagnosticDescriptors.cs](file:///c:/Users/HP/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/Diagnostics/InfrastructureDiagnosticDescriptors.cs)
Define 20 rules (`CAA2001` - `CAA2020`):
1. **CAA2001**: Infrastructure classes must not implement core business logic.
2. **CAA2002**: Infrastructure classes implementing services should be `sealed`.
3. **CAA2003**: DbContext or DbConnection must not be exposed via public members.
4. **CAA2004**: Repositories must be `internal` or `sealed` and implement a domain/application interface.
5. **CAA2005**: Database entities/persistence models must not be exposed outside Infrastructure.
6. **CAA2006**: SQL queries must be parameterized (avoid string interpolation/concatenation in SQL methods).
7. **CAA2007**: Repositories should not perform validation or domain calculations.
8. **CAA2008**: HTTP clients / External API calls must only reside in Infrastructure.
9. **CAA2009**: Infrastructure singletons or services must be thread-safe (no mutable public properties).
10. **CAA2010**: Connection strings or secrets must not be hardcoded.
11. **CAA2011**: DbContext classes should inject DbContextOptions.
12. **CAA2012**: Infrastructure should use ILogger instead of System.Console.
13. **CAA2013**: Classes with "Repository" suffix must implement a repository interface.
14. **CAA2014**: Infrastructure classes should end with standard suffixes (Repository, DbContext, Service, Client, etc.).
15. **CAA2015**: EF Core Migrations should not be in Domain or Application.
16. **CAA2016**: Infrastructure services should not use parameterless constructors if they have external dependencies.
17. **CAA2017**: EF Core queries in Read repositories should use AsNoTracking() where possible.
18. **CAA2018**: Transactions should be managed by the Unit of Work, not inside individual repositories.
19. **CAA2019**: Infrastructure classes must not implement domain interfaces (except repositories and application services).
20. **CAA2020**: Database and network operations must be asynchronous (use Async methods).

#### [NEW] [ApplicationAnalyzer.cs](file:///c:/Users/HP/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/Analyzers/ApplicationAnalyzer.cs)
- Implement `ApplicationAnalyzer` inheriting from `DiagnosticAnalyzer`.
- Check if assembly is in Application layer.
- Execute rules CAA1001-CAA1020 on classes/symbols.

#### [NEW] [InfrastructureAnalyzer.cs](file:///c:/Users/HP/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/CleanArchitectureAnalyzer/Analyzers/InfrastructureAnalyzer.cs)
- Implement `InfrastructureAnalyzer` inheriting from `DiagnosticAnalyzer`.
- Check if assembly is in Infrastructure layer.
- Execute rules CAA2001-CAA2020 on classes/symbols.

## Verification Plan

### Automated Tests
- Run unit tests in `CleanArchitectureAnalyzer.Test` to verify that existing domain analyzer tests still pass.
- Add unit tests inside a new test file `ApplicationAnalyzerTests.cs` and `InfrastructureAnalyzerTests.cs` to test the new rules.
