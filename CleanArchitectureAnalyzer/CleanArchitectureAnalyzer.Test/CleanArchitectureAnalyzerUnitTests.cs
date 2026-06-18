using System.Threading.Tasks;
using Xunit;
using CleanArchitectureAnalyzer.Analyzers;
using CleanArchitectureAnalyzer.Test.Utilities;

namespace CleanArchitectureAnalyzer.Test
{
    public class CleanArchitectureAnalyzerUnitTest
    {
        [Fact]
        public async Task BadReferencesRule_AllowsDomainAndSystemTypes()
        {
            var testCode = @"
using System;
using System.Collections.Generic;

namespace MyProject.Domain
{
    public class Ingredient
    {
        public void Update() { }
    }

    public class Recipe
    {
        private List<Ingredient> _ingredients;
        private Ingredient[] _ingredientsArray;
        private Ingredient _ingredient;
        private int _count;
        private string _name;

        public void Cook() { }
    }
}";

            await CSharpVerifier<DomainAnalyzer>.VerifyAnalyzerAsyncV2(testCode);
        }

        [Fact]
        public async Task BadReferencesRule_WarnsOnExternalTypes()
        {
            var testCode = @"
using System;

namespace MyProject.Domain
{
    public class Recipe
    {
        private Xunit.Assert _assert;

        public void Cook() { }
    }
}";
            // The location will be at line 8, column 30 (private Xunit.Assert _assert;)
            var expected = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0001")
                .WithSpan(8, 30, 8, 37)
                .WithArguments("Recipe", "Assert");

            await CSharpVerifier<DomainAnalyzer>.VerifyAnalyzerAsyncV2(testCode, expected);
        }

        [Fact]
        public async Task DomainServiceRules_WarnsOnViolations()
        {
            var testCode = @"
using System;

namespace MyProject.Domain
{
    // Violation of CAA0403 (No interface) and CAA0402 (Mutable property)
    public class PaymentService
    {
        public string Status { get; set; } // CAA0402
    }

    // Violation of CAA0404 (Static service)
    public static class DeliveryService
    {
        public static void Deliver() { }
    }

    public interface IPureService { }

    public class UserDto { }
    public class OrderRepository { }

    // Violation of CAA0405 (Uses DTO) and CAA0406 (Concrete repository)
    public class PureService : IPureService
    {
        public PureService(OrderRepository repo) { } // CAA0406

        public UserDto GetUser() => null; // CAA0405
    }
}";
            var expected1 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0402")
                .WithSpan(9, 23, 9, 29)
                .WithArguments("PaymentService", "Status");

            var expected2 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0403")
                .WithSpan(7, 18, 7, 32)
                .WithArguments("PaymentService");

            var expected3 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0404")
                .WithSpan(13, 25, 13, 40)
                .WithArguments("DeliveryService");

            var expected4 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0406")
                .WithSpan(26, 44, 26, 48)
                .WithArguments("PureService", "OrderRepository");

            var expected5 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0405")
                .WithSpan(28, 24, 28, 31)
                .WithArguments("PureService", "GetUser", "UserDto");

            await CSharpVerifier<DomainAnalyzer>.VerifyAnalyzerAsyncV2(testCode, expected1, expected2, expected3, expected4, expected5);
        }

        [Fact]
        public async Task DomainEventRules_WarnsOnViolations()
        {
            var testCode = @"
using System;

namespace MyProject.Domain
{
    public class OrderEntity
    {
        public Guid Id { get; }
    }
}

namespace MyProject.Domain.Events
{
    using MyProject.Domain;

    // Violation of CAA0501 (Mutable property), CAA0502 (No timestamp), CAA0503 (References entity), CAA0504 (Business method), CAA0505 (Invalid name)
    public class OrderCreatedBadName
    {
        public int Id { get; set; } // CAA0501
        public OrderEntity Order { get; set; } // CAA0503

        public void ProcessEvent() // CAA0504
        {
        }
    }

    // Valid Domain Event
    public class OrderCreatedEvent
    {
        public int Id { get; }
        public DateTime OccurredOn { get; }

        public OrderCreatedEvent(int id, DateTime occurredOn)
        {
            Id = id;
            OccurredOn = occurredOn;
        }
    }
}
";
            var expected1 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0501")
                .WithSpan(19, 20, 19, 22)
                .WithArguments("OrderCreatedBadName", "Id");

            var expected2 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0502")
                .WithSpan(17, 18, 17, 37)
                .WithArguments("OrderCreatedBadName");

            var expected3 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0503")
                .WithSpan(20, 28, 20, 33)
                .WithArguments("OrderCreatedBadName", "OrderEntity", "Order");

            var expected4 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0504")
                .WithSpan(22, 21, 22, 33)
                .WithArguments("OrderCreatedBadName", "ProcessEvent");

            var expected5 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0505")
                .WithSpan(17, 18, 17, 37)
                .WithArguments("OrderCreatedBadName");

            var expected6 = CSharpVerifier<DomainAnalyzer>.Diagnostic("CAA0501")
                .WithSpan(20, 28, 20, 33)
                .WithArguments("OrderCreatedBadName", "Order");

            await CSharpVerifier<DomainAnalyzer>.VerifyAnalyzerAsyncV2(testCode, expected1, expected2, expected3, expected4, expected5, expected6);
        }
    }
}
