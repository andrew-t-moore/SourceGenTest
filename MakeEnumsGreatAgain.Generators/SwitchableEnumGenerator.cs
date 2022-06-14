using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MakeEnumsGreatAgain.Generators;

[Generator]
public class SwitchableEnumGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var enums = GetEnums(context);

        var methods = string.Join("", enums.Select(e => GenerateSwitchMethod(
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
        File.WriteAllText("C:/Output/SwitchableEnumGenerator.output.cs", source);
        
        context.AddSource("SwitchableEnumGenerator", source);
    }
    
    private static IEnumerable<EnumDeclarationSyntax> GetEnums(GeneratorExecutionContext context)
    {
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semantic = context.Compilation.GetSemanticModel(tree);
    
            foreach (var foundEnum in tree.GetRoot().DescendantNodesAndSelf().OfType<EnumDeclarationSyntax>())
            {
                var enumSymbol = semantic.GetDeclaredSymbol(foundEnum);
    
                if (enumSymbol != null && enumSymbol.GetAttributes()
                        .Any(attribute => attribute.AttributeClass?.Name == "SwitchableAttribute"))
                {
                    yield return foundEnum;
                }
            }
        }
    }

    private static string GenerateSwitchMethod(
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
                argName: CamelCase.ToCamelCase(valueName)
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
}