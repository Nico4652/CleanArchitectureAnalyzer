using CleanArchitectureAnalyzer.Constants;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitectureAnalyzer.Helpers
{
    public class ReferenceHelper
    {
        public static bool isExternalReference(ITypeSymbol typeSymbol, IAssemblySymbol currentAssembly)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return isExternalReference(arrayType.ElementType, currentAssembly);
            }

            if (typeSymbol is IPointerTypeSymbol pointerType)
            {
                return isExternalReference(pointerType.PointedAtType, currentAssembly);
            }

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        if (isExternalReference(arg, currentAssembly))
                        {
                            return true;
                        }
                    }
                }

                var assembly = namedType.ContainingAssembly;
                if (assembly == null)
                {
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(assembly, currentAssembly))
                {
                    return false;
                }

                if (namedType.SpecialType != SpecialType.None)
                {
                    return false;
                }

                var assemblyName = assembly.Name;
                bool isSystem = assemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                                assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                                assemblyName.StartsWith("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase);

                if (isSystem)
                {
                    return false;
                }

                return true;
            }

            return false;
        }


        public static bool TypeReference(ITypeSymbol typeSymbol, string domainType)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            

            if (typeSymbol is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        if (TypeReference(arg, domainType))
                        {
                            return true;
                        }
                    }
                }

               

                if (namedType.SpecialType != SpecialType.None)
                {
                    return false;
                }
                var assembly = namedType.ContainingAssembly;
                if (assembly != null)
                {
                    var assemblyName = assembly.Name;
                    bool isSystem = assemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                                    assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                                    assemblyName.StartsWith("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase);
                    if (isSystem)
                    {
                        return false; // Es un tipo del sistema válido
                    }
                }
                if (DomainTypeHelper.Type(namedType) == domainType)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
