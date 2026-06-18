using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CleanArchitectureAnalyzer.Extensions
{
    public static class SymbolExtensions
    {
        public static IEnumerable<IPropertySymbol>
            GetProperties(
                this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                         .OfType<IPropertySymbol>();
        }

        public static IEnumerable<IMethodSymbol>
            GetBusinessMethods(
                this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                            .OfType<IMethodSymbol>()
                            .Where(m => m.MethodKind == MethodKind.Ordinary);
        }

        public static IEnumerable<IMethodSymbol>
           GetConstructors(
               this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                            .OfType<IMethodSymbol>()
                            .Where(m => m.MethodKind == MethodKind.Constructor);
        }


        public static IEnumerable<IFieldSymbol> GetFields(this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                         .OfType<IFieldSymbol>();
        }

        public static IEnumerable<INamedTypeSymbol> GetInterfaces(this INamedTypeSymbol symbol)
        {
            return symbol.Interfaces;
        }

       


    }
}
