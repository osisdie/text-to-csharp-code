using Microsoft.CodeAnalysis;

namespace TextToCode.Infrastructure.Compilation;

internal static class CompilationReferenceResolver
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> DefaultReferences = new(LoadDefaultReferences);

    public static IReadOnlyList<MetadataReference> GetDefaultReferences() => DefaultReferences.Value;

    private static IReadOnlyList<MetadataReference> LoadDefaultReferences()
    {
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        var references = new List<MetadataReference>();

        foreach (var path in trustedAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (IsCommonReference(fileName))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }

    private static bool IsCommonReference(string assemblyName) =>
        assemblyName switch
        {
            "System.Runtime" => true,
            "System.Console" => true,
            "System.Collections" => true,
            "System.Linq" => true,
            "System.Threading" => true,
            "System.Threading.Tasks" => true,
            "System.Text.RegularExpressions" => true,
            "System.ComponentModel" => true,
            "System.Runtime.Extensions" => true,
            "System.Collections.Generic" => true,
            "System.Private.CoreLib" => true,
            "netstandard" => true,
            "System.ObjectModel" => true,
            "System.Text.Json" => true,
            "System.Memory" => true,
            "System.Numerics.Vectors" => true,
            "System.Runtime.Numerics" => true,
            "System.Globalization" => true,
            _ => false
        };
}
