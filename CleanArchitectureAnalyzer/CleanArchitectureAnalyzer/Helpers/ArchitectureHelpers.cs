using CleanArchitectureAnalyzer.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace CleanArchitectureAnalyzer.Helpers
{
    public static class ArchitectureHelpers
    {
        public static bool IsDomain(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsDomain(context.Options, symbol);
        }

        public static bool IsDomain(SyntaxNodeAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsDomain(context.Options, symbol);
        }

        public static bool IsDomain(AnalyzerOptions options, INamedTypeSymbol symbol)
        {
            if (symbol == null || symbol.ContainingAssembly == null)
            {
                return false;
            }

            var assemblyName = symbol.ContainingAssembly.Name;

            // 1. Try to read from .editorconfig
            if (symbol.Locations.Length > 0 && symbol.Locations[0].SourceTree != null)
            {
                var sourceTree = symbol.Locations[0].SourceTree;
                var config = options.AnalyzerConfigOptionsProvider.GetOptions(sourceTree);

                if (config.TryGetValue(AnalyzerOptionNames.DomainAssembly, out var domainAssembly) &&
                    !string.IsNullOrWhiteSpace(domainAssembly))
                {
                    domainAssembly = domainAssembly.Trim();

                    // Match exactly or as suffix (e.g., if domainAssembly is "Domain" and assemblyName is "MyProject.Domain")
                    if (assemblyName.Equals(domainAssembly, StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.EndsWith("." + domainAssembly, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // 2. Fallback: convention-based detection (if .editorconfig is not present or doesn't match)
            return assemblyName.Equals("Domain", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Domain", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsApplication(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsApplication(context.Options, symbol);
        }

        public static bool IsApplication(SyntaxNodeAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsApplication(context.Options, symbol);
        }

        public static bool IsApplication(AnalyzerOptions options, INamedTypeSymbol symbol)
        {
            if (symbol == null || symbol.ContainingAssembly == null)
            {
                return false;
            }

            var assemblyName = symbol.ContainingAssembly.Name;

            // 1. Try to read from .editorconfig
            if (symbol.Locations.Length > 0 && symbol.Locations[0].SourceTree != null)
            {
                var sourceTree = symbol.Locations[0].SourceTree;
                var config = options.AnalyzerConfigOptionsProvider.GetOptions(sourceTree);

                if (config.TryGetValue(AnalyzerOptionNames.ApplicationAssembly, out var appAssembly) &&
                    !string.IsNullOrWhiteSpace(appAssembly))
                {
                    appAssembly = appAssembly.Trim();

                    if (assemblyName.Equals(appAssembly, StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.EndsWith("." + appAssembly, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // 2. Fallback: convention-based detection
            return assemblyName.Equals("Application", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Application", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInfrastructure(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsInfrastructure(context.Options, symbol);
        }

        public static bool IsInfrastructure(SyntaxNodeAnalysisContext context, INamedTypeSymbol symbol)
        {
            return IsInfrastructure(context.Options, symbol);
        }

        public static bool IsInfrastructure(AnalyzerOptions options, INamedTypeSymbol symbol)
        {
            if (symbol == null || symbol.ContainingAssembly == null)
            {
                return false;
            }

            var assemblyName = symbol.ContainingAssembly.Name;

            // 1. Try to read from .editorconfig
            if (symbol.Locations.Length > 0 && symbol.Locations[0].SourceTree != null)
            {
                var sourceTree = symbol.Locations[0].SourceTree;
                var config = options.AnalyzerConfigOptionsProvider.GetOptions(sourceTree);

                if (config.TryGetValue(AnalyzerOptionNames.InfrastructureAssembly, out var infraAssembly) &&
                    !string.IsNullOrWhiteSpace(infraAssembly))
                {
                    infraAssembly = infraAssembly.Trim();

                    if (assemblyName.Equals(infraAssembly, StringComparison.OrdinalIgnoreCase) ||
                        assemblyName.EndsWith("." + infraAssembly, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // 2. Fallback: convention-based detection
            return assemblyName.Equals("Infrastructure", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.EndsWith(".Infrastructure", StringComparison.OrdinalIgnoreCase);
        }
    }
}
