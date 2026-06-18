using Microsoft.CodeAnalysis;
using System;
using CleanArchitectureAnalyzer.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleanArchitectureAnalyzer.Helpers
{
    public  class DomainTypeHelper
    {
        

        public static string Type(INamedTypeSymbol namedTypeSymbol)
        {
            var name = namedTypeSymbol.Name;
            var detected = DomainType.types.FirstOrDefault(x => name.Equals(x, StringComparison.OrdinalIgnoreCase) 
                                        || name.StartsWith(x, StringComparison.OrdinalIgnoreCase) 
                                        || name.EndsWith(x, StringComparison.OrdinalIgnoreCase));
            if (detected != null)
            {
                return detected;
            }

            var ns = namedTypeSymbol.ContainingNamespace;
            while (ns != null)
            {
                if (ns.Name.Equals("Events", StringComparison.OrdinalIgnoreCase) || ns.Name.Equals("Event", StringComparison.OrdinalIgnoreCase))
                {
                    return DomainType.Event;
                }
                ns = ns.ContainingNamespace;
            }

            return null;
        }
    }
}
