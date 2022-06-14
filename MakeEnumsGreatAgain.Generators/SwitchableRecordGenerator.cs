using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MakeEnumsGreatAgain.Generators;

[Generator]
public class SwitchableRecordGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var records = GetRecords(context);

        var methods = string.Join("", records.Select(r => GenerateSwitchMethod(
            context,
            r
        )));

        var source = @$"
namespace RecordsAreNowGreat {{
    public static class RecordSwitchableExtensions {{
        {methods}
    }}
}}";
        // Only for debugging purposes.
        File.WriteAllText("C:/Output/SwitchableRecordGenerator.output.cs", source);
        
        context.AddSource("SwitchableRecordGenerator", source);
    }

    private static IEnumerable<RecordDeclarationSyntax> GetRecords(GeneratorExecutionContext context)
    {
        foreach (var tree in context.Compilation.SyntaxTrees)
        {
            var semantic = context.Compilation.GetSemanticModel(tree);
    
            foreach (var foundRecord in tree.GetRoot().DescendantNodesAndSelf().OfType<RecordDeclarationSyntax>())
            {
                var recordSymbol = semantic.GetDeclaredSymbol(foundRecord);
    
                if (recordSymbol != null && recordSymbol.GetAttributes()
                        .Any(attribute => attribute.AttributeClass?.Name == "SwitchableAttribute"))
                {
                    yield return foundRecord;
                }
            }
        }
    }
    
    private static string GenerateSwitchMethod(
        GeneratorExecutionContext context,
        RecordDeclarationSyntax record
    )
    {
        var semantic = context.Compilation.GetSemanticModel(record.SyntaxTree);
        var symbol = semantic.GetDeclaredSymbol(record);
        var enumName = record.Identifier.Text;
        var containingNamespace = symbol.ContainingNamespace;
        var containingTypeName = symbol.ContainingType;

        var typeNameSuffix = containingTypeName != null
            ? $"{containingTypeName}.{enumName}"
            : enumName;

        var typeName = $"{containingNamespace}.{typeNameSuffix}";

        // TODO: assumption: the sub-types are defined inside the base type.
        // TODO: assumption: that you haven't nested types any further.
        var subTypes = record.DescendantNodes()
            .OfType<RecordDeclarationSyntax>()
            .Select(s =>
            {
                var subSemantic = context.Compilation.GetSemanticModel(s.SyntaxTree);
                var subSymbol = subSemantic.GetDeclaredSymbol(s);
                var subTypeName = $"{typeName}.{subSymbol.Name}";

                return (
                    typeName: subTypeName,
                    camelCaseName: CamelCase.ToCamelCase(subSymbol.Name)
                );
            })
            .ToImmutableArray();
        
        var args = string.Join(", ", subTypes
            .Select(s => $"Func<{s.typeName}, T> {s.camelCaseName}")
        );

        var cases = string.Join("\r\n", subTypes
            .Select((m,i) => $"{m.typeName} v{i} => {m.camelCaseName}(v{i}),")
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
