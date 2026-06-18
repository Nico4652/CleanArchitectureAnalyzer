using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

namespace CleanArchitectureAnalyzer.Test.Utilities
{
    public class CSharpVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public static DiagnosticResult Diagnostic() =>
            CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic();

        public static DiagnosticResult Diagnostic(string diagnosticId) =>
            CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic(diagnosticId);

        public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
            new(descriptor);

        public static DiagnosticResult CompilerError(string errorIdentifier) =>
            new(errorIdentifier, DiagnosticSeverity.Error);

        public static Task VerifyAnalyzerAsyncV2(
            string source,
            params DiagnosticResult[] diagnostics) =>
            VerifyAnalyzerAsyncV2(new[] { source }, diagnostics);

        public static Task VerifyApplicationAnalyzerAsync(
            string source,
            params DiagnosticResult[] diagnostics)
        {
            var test = new TestV2();
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", "is_global = true\r\ncleanarchitecture_application_assembly = TestProject"));

            test.TestState.Sources.Add(source);
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        public static Task VerifyInfrastructureAnalyzerAsync(
            string source,
            params DiagnosticResult[] diagnostics)
        {
            var test = new TestV2();
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", "is_global = true\r\ncleanarchitecture_infrastructure_assembly = TestProject"));

            test.TestState.Sources.Add(source);
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        public static Task VerifyAnalyzerAsyncV2(
            string[] sources,
            params DiagnosticResult[] diagnostics)
        {
            var test = new TestV2();
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", "is_global = true\r\ncleanarchitecture_domain_assembly = TestProject"));

            foreach (var source in sources)
                test.TestState.Sources.Add(source);

            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        public static Task VerifyAnalyzerAsyncV2(
            (string filename, string content)[] sources,
            params DiagnosticResult[] diagnostics)
        {
            var test = new TestV2();
            test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        public static Task VerifyCodeFixAsyncV2(
            string before,
            string after,
            int? codeActionIndex = null,
            params DiagnosticResult[] diagnostics)
        {
            var test = new TestV2
            {
                TestCode = before,
                FixedCode = after,           
                CodeActionIndex = codeActionIndex
            };
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        public class TestV2 : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>
        {
            public TestV2()
            {
                ReferenceAssemblies = CodeAnalyzerHelper.CurrentXunitV2;

                // xunit diagnostics are reported in both normal and generated code
                TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;
            }

            protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
            {
                var analyzer = new TAnalyzer();

                foreach (var provider in CodeFixProviderDiscovery.GetCodeFixProviders(Language))
                    if (analyzer.SupportedDiagnostics.Any(diagnostic => provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
                        yield return provider;
            }
        }
    }
}
