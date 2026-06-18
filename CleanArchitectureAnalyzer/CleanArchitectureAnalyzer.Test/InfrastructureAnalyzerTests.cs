using System.Threading.Tasks;
using Xunit;
using CleanArchitectureAnalyzer.Analyzers;
using CleanArchitectureAnalyzer.Test.Utilities;

namespace CleanArchitectureAnalyzer.Test
{
    public class InfrastructureAnalyzerTests
    {
        [Fact]
        public async Task InfrastructureClassSealedRule_WarnsOnUnsealedClasses()
        {
            var testCode = @"
using System;

namespace MyProject.Infrastructure
{
    // Violation of CAA2002 (should be sealed)
    public class SQLServerService
    {
    }
}";
            var expected1 = CSharpVerifier<InfrastructureAnalyzer>.Diagnostic("CAA2002")
                .WithSpan(7, 18, 7, 34)
                .WithArguments("SQLServerService");

            await CSharpVerifier<InfrastructureAnalyzer>.VerifyInfrastructureAnalyzerAsync(testCode, expected1);
        }

        [Fact]
        public async Task RepositoryInterfaceRequirementRule_WarnsOnMissingInterface()
        {
            var testCode = @"
using System;

namespace MyProject.Infrastructure
{
    // Violation of CAA2013 (does not implement repository interface)
    public sealed class ProductRepository
    {
    }
}";
            var expected = CSharpVerifier<InfrastructureAnalyzer>.Diagnostic("CAA2013")
                .WithSpan(7, 25, 7, 42)
                .WithArguments("ProductRepository");

            await CSharpVerifier<InfrastructureAnalyzer>.VerifyInfrastructureAnalyzerAsync(testCode, expected);
        }
    }
}
