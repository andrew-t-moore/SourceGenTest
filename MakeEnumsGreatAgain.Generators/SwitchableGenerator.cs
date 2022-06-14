using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MakeEnumsGreatAgain.Generators;

[Generator]
public class SwitchableGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        Console.WriteLine("Initialising");
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var enums = GetEnums(context);

        var methods = string.Join("", enums.Select(e => GenerateEnumSwitch(
            context,
            e
        )));

        var source = @$"
namespace EnumsAreNowGreat {{
    public static class EnumSwitchableExtensions {{
        {methods}
    }}
}}";
        
        // Only for debugging purposes.
        File.WriteAllText("C:/Output/generated.cs", source);
        
        context.AddSource("SwitchableGenerator", source);
    }
    
    private string GenerateEnumSwitch(
        GeneratorExecutionContext context,
        EnumDeclarationSyntax @enum
    )
    {
        var semantic = context.Compilation.GetSemanticModel(@enum.SyntaxTree);
        var symbol = semantic.GetDeclaredSymbol(@enum);
        var enumName = @enum.Identifier.Text;
        var containingNamespace = symbol.ContainingNamespace;
        var containingTypeName = symbol.ContainingType;

        var typeNameSuffix = containingTypeName != null
            ? $"{containingTypeName}.{enumName}"
            : enumName;

        var typeName = $"{containingNamespace}.{typeNameSuffix}";

        var members = @enum.Members
            .Select(member => member.Identifier.Text)
            .Select(valueName => (
                valueName,
                argName: ToCamelCase(valueName)
            ))
            .ToArray();
        
        var args = string.Join(", ", members
            .Select(m => $"Func<T> {m.argName}")
        );

        var cases = string.Join("\r\n", members
            .Select(m => $"{typeName}.{m.valueName} => {m.argName}(),")
            .Concat(new[]{ "_ => throw new ArgumentOutOfRangeException()" })
            .Select(m => $"                {m}")
        );
        
        return @$"
        public static T Switch<T>(this {typeName} value, {args}){{
            return value switch {{
{cases}
            }};
        }}
";
    }

    private static string ToCamelCase(string input)
    {
        // This is not a great camel case algorithm,
        // but it's just for demonstration purposes.
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new Exception("Input string was null or whitespace");
        }

        var prefix = new string(input
            .TakeWhile(char.IsUpper)
            .ToArray())
            .ToLowerInvariant();

        return prefix.Length == input.Length
            ? prefix
            : prefix + input.Substring(prefix.Length);
    }
    
    private static IEnumerable<EnumDeclarationSyntax> GetEnums(GeneratorExecutionContext context) {
        foreach (var tree in context.Compilation.SyntaxTrees) {
            var semantic = context.Compilation.GetSemanticModel(tree);
    
            foreach (var foundEnum in tree.GetRoot().DescendantNodesAndSelf().OfType<EnumDeclarationSyntax>()) {
                var enumSymbol = semantic.GetDeclaredSymbol(foundEnum);
    
                if (enumSymbol != null && enumSymbol.GetAttributes()
                        .Any(attribute => attribute.AttributeClass?.Name == "SwitchableAttribute")) {
                    yield return foundEnum;
                }
            }
        }
    }
}
