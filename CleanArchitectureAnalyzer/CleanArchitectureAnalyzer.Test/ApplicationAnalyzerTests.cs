using System.Threading.Tasks;
using Xunit;
using CleanArchitectureAnalyzer.Analyzers;
using CleanArchitectureAnalyzer.Test.Utilities;

namespace CleanArchitectureAnalyzer.Test
{
    public class ApplicationAnalyzerTests
    {
        [Fact]
        public async Task UseCaseImmutableRule_WarnsOnMutableProperties()
        {
            var testCode = @"
using System;

namespace MyProject.Application
{
    // Violation of CAA1003 (public setter) and CAA1020 (not sealed)
    public class CreateProductCommand
    {
        public string Name { get; set; } // CAA1003
    }
}";
            var expected1 = CSharpVerifier<ApplicationAnalyzer>.Diagnostic("CAA1003")
                .WithSpan(9, 23, 9, 27)
                .WithArguments("CreateProductCommand", "Name");

            await CSharpVerifier<ApplicationAnalyzer>.VerifyApplicationAnalyzerAsync(testCode, expected1);
        }

        [Fact]
        public async Task CommandNamingRule_WarnsOnBadName()
        {
            var testCode = @"
using System;

namespace MyProject.Application
{
    public sealed class ProductCommand
    {
        public string Name { get; }
    }
}";
            var expected = CSharpVerifier<ApplicationAnalyzer>.Diagnostic("CAA1014")
                .WithSpan(6, 25, 6, 39)
                .WithArguments("ProductCommand");

            await CSharpVerifier<ApplicationAnalyzer>.VerifyApplicationAnalyzerAsync(testCode, expected);
        }
    }
}
