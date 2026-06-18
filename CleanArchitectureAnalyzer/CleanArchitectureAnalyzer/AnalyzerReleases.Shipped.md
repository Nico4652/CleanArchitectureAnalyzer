## Release 1.0

### New Rules

Rule ID | Category	   | Severity | Notes
--------|--------------|----------|------------------------------------------------------------------------
CAA0001 | Architecture | Warning  | Domain elements depends on External Classes
CAA0002 | Architecture | Warning  | Domain elements must not contain dependency injection in a constructor
CAA0003 | Architecture | Warning  | Domain elements should not expose collections as List<T>
CAA0004 | Architecture | Warning  | Domain elements should not depend on external interfaces
CAA0005 | Architecture | Warning  | The entity should not expose public builders without parameters
CAA0006 | Architecture | Warning  | Entities should not expose public constructors accepting an ID
CAA0007 | Architecture | Warning  | Domain event handlers should not be placed in the Domain layer
CAA0101 | Architecture | Warning  | Possible Anemic Entity
CAA0102 | Architecture | Warning  | Entity with Public Setters
CAA0103 | Architecture | Warning  | Entity should have an Id
CAA0201 | Architecture | Warning  | Value object should be immutable
CAA0202 | Architecture | Warning  | Value object must not have an ID
CAA0203 | Architecture | Warning  | Value object should implement value equality
CAA0204 | Architecture | Warning  | Value object should not contain collections
CAA0205 | Architecture | Warning  | Value objects must implement IEquatable<T>
CAA0206 | Architecture | Warning  | Value object collections must be ReadOnly
CAA0207 | Architecture | Warning  | Value object must not reference entities or aggregate roots
CAA0301 | Architecture | Warning  | Aggregate root cannot have properties of a type other than aggregate root
CAA0302 | Architecture | Warning  | Domain events should only be raised by Aggregate Roots
CAA0303 | Architecture | Warning  | Aggregate roots should use Factory methods instead of public constructors
CAA0401 | Architecture | Warning  | Domain services should be stateless
CAA0402 | Architecture | Warning  | Domain services should not expose mutable properties
CAA0403 | Architecture | Warning  | Domain services should implement at least one interface
CAA0404 | Architecture | Warning  | Domain services should not be static classes
CAA0405 | Architecture | Warning  | Domain service methods must not accept or return DTOs or ViewModels
CAA0406 | Architecture | Warning  | Domain services must not depend on concrete repository implementations
CAA0501 | Architecture | Warning  | Domain events must be immutable
CAA0502 | Architecture | Warning  | Domain events must contain a timestamp property
CAA0503 | Architecture | Warning  | Domain events must not reference mutable entities or aggregate roots directly
CAA0504 | Architecture | Warning  | Domain events should not contain business logic methods
CAA0505 | Architecture | Warning  | Domain events should end with Event or be named in the past tense
CAA1001 | Architecture | Warning  | Application layer must not reference Infrastructure
CAA1002 | Architecture | Warning  | Application layer must not reference Presentation
CAA1003 | Architecture | Warning  | Use case request must be immutable
CAA1004 | Architecture | Warning  | Use case handler should not expose public properties or fields
CAA1005 | Architecture | Warning  | Use case output must not expose Domain Entities or Aggregate Roots
CAA1006 | Architecture | Warning  | Use case input must not accept Domain Entities or Aggregate Roots
CAA1007 | Architecture | Warning  | Application services must be stateless
CAA1008 | Architecture | Warning  | Validators must reside in the Application layer
CAA1009 | Architecture | Warning  | Application exceptions should be sealed
CAA1010 | Architecture | Warning  | CQRS Commands should not contain business logic or behavior
CAA1011 | Architecture | Warning  | CQRS Queries should not contain business logic or behavior
CAA1012 | Architecture | Warning  | Application layer should not access the file system directly
CAA1013 | Architecture | Warning  | Application layer should not use DateTime.Now or DateTime.UtcNow directly
CAA1014 | Architecture | Warning  | Command name should start with a verb
CAA1015 | Architecture | Warning  | Query name should end with Query or start with Get/Find/List/Search
CAA1016 | Architecture | Warning  | Do not use Task.Delay or Thread.Sleep inside Use Cases
CAA1017 | Architecture | Warning  | Use case handlers must not contain empty catch blocks
CAA1018 | Architecture | Warning  | DTO classes should not contain behavior/business logic methods
CAA1019 | Architecture | Warning  | Application layer must not reference third-party SDK clients directly
CAA1020 | Architecture | Warning  | Use case handlers should be sealed
CAA2001 | Architecture | Warning  | Infrastructure classes must not contain core business logic
CAA2002 | Architecture | Warning  | Infrastructure implementation classes should be sealed
CAA2003 | Architecture | Warning  | DbContext or DbConnection must not be exposed
CAA2004 | Architecture | Warning  | Repositories must be internal or sealed
CAA2005 | Architecture | Warning  | Database entities/persistence models must not be exposed outside Infrastructure
CAA2006 | Architecture | Warning  | Avoid raw/interpolated SQL strings in Infrastructure without parametrization
CAA2007 | Architecture | Warning  | Repositories should not perform business validation
CAA2008 | Architecture | Warning  | HTTP Clients and External API calls must only reside in Infrastructure
CAA2009 | Architecture | Warning  | Infrastructure services must be thread-safe
CAA2010 | Architecture | Warning  | Connection strings or API secrets must not be hardcoded
CAA2011 | Architecture | Warning  | DbContext classes should inject DbContextOptions
CAA2012 | Architecture | Warning  | Use ILogger instead of System.Console
CAA2013 | Architecture | Warning  | Classes with Repository suffix must implement a repository interface
CAA2014 | Architecture | Warning  | Infrastructure classes should have recognized suffixes
CAA2015 | Architecture | Warning  | Migrations must not be in Domain or Application layers
CAA2016 | Architecture | Warning  | Infrastructure services must not expose parameterless constructors if dependencies exist
CAA2017 | Architecture | Warning  | EF Core queries in read repositories should use AsNoTracking
CAA2018 | Architecture | Warning  | Repositories should not save or commit transactions internally
CAA2019 | Architecture | Warning  | Infrastructure classes must not implement Domain Event or Entity interfaces directly
CAA2020 | Architecture | Warning  | Database and I/O operations should be asynchronous
