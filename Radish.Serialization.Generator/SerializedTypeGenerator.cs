using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Radish.Serialization;

[Generator]
public class SerializedTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var flaggedTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Radish.Serialization.SerializedTypeAttribute", FilterProcessedSyntaxNodes, MakeClassInfo);

        context.RegisterSourceOutput(flaggedTypes, ProcessClasses);
    }

    private static void ProcessClasses(SourceProductionContext context, SerializedTypeClassInfo info)
    {
        using var dest = new StringWriter();
        using var writer = new IndentedTextWriter(dest);
        writer.WriteLine("using System;");
        writer.WriteLine("using Radish.Serialization;");
        writer.WriteLine();
        
        writer.WriteLine($"namespace {info.Namespace}");
        writer.WriteLine("{");
        writer.Indent++;
        WriteClass(in context, info, writer);
        writer.Indent--;
        writer.WriteLine("}");
        context.AddSource($"{info.Namespace}.{info.MetadataName}_Serializer.g.cs", dest.ToString());
    }

    private static void WriteClass(in SourceProductionContext context, SerializedTypeClassInfo info, IndentedTextWriter writer)
    {
        writer.WriteLine($"partial {(info.IsStruct ? "struct" : "class")} {info.TypeName}");
        writer.WriteLine($" : ISerializable<{info.TypeName}>");
        writer.WriteLine("{");
        writer.Indent++;

        #region Constants

        writer.WriteLine($"public const string Tag = {info.Tag};");
        writer.WriteLine($"public const int FieldCount = {info.Fields.Count};");
        writer.WriteLine();

        #endregion


        #region Serialize Method

        writer.WriteLine($"public static void Serialize({info.TypeName} me, IDocumentNode parent, string name, SerializationContext context)");
        writer.WriteLine("{");
        writer.Indent++;

        writer.WriteLine("var node = parent switch");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("IObjectNode objN => objN.AddChildObject(name, Tag),");
        writer.WriteLine("IListNode objL => objL.AddChildObject(Tag),");
        writer.WriteLine("_ => throw new SerializerException($\"Unsupported node type {parent.GetType().FullName}\")");
        writer.Indent--;
        writer.WriteLine("};");
        writer.WriteLine();

        writer.WriteLine("if (me is IPreSerializeCallback pc)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("pc.OnSerialize(context, node);");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        foreach (var field in info.Fields)
        {
            WriteFieldSerializer(in context, field, info, writer);
        }

        writer.Indent--;
        writer.WriteLine("}");

        #endregion

        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void WriteFieldSerializer(in SourceProductionContext context, SerializedFieldInfo info, SerializedTypeClassInfo owner, IndentedTextWriter writer)
    {
        if (!info.HasSetter)
        {
            writer.WriteLine($"// WARNING: could not generate for {info.MemberName} as it has no setter");
            return;
        }

        // writes serialization methods for the field
        //writer.WriteLine($"private static void SerializeField_{info.MemberName}({info.MemberType} me, IDocumentNode parent, SerializationContext context)");
        writer.WriteLine($"// Field: {info.MemberType} {info.MemberName}");
        writer.WriteLine("{");
        writer.Indent++;

        if (info.IsList)
        {
            writer.WriteLine($"var nn = node.AddChildList({info.SerializedName});");
            writer.WriteLine($"var num = me.{info.MemberName}.Count;");
            writer.WriteLine("for (var i = 0; i < num; ++i)");
            writer.WriteLine("{");
            writer.Indent++;

            WriteField($"{info.MemberName}[i]", "string.Empty", "nn");

            writer.Indent--;
            writer.WriteLine("}");
        }
        else
        {
            WriteField(info.MemberName, info.SerializedName, "node");
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        return;

        void WriteField(string name, string serializedName, string nodeName)
        {
            if (!info.TypeIsAlsoSerializable)
            {
                writer.WriteLine($"var primSerializer = context.GetPrimitiveSerializer<{info.MemberType}>();");

                writer.WriteLine("if (primSerializer != null)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"primSerializer.Serialize(me.{name}, {serializedName}, {nodeName});");
                writer.Indent--;
                writer.WriteLine("}");

                writer.WriteLine("else");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"throw new SerializerException($\"No serializer found for type {{typeof({info.MemberType}).FullName}}\");");
                writer.Indent--;
                writer.WriteLine("}");
            }
            else
            {
                //todo: generate generic type specializations (only once) for uses of generics

                writer.WriteLine($"{info.MemberType}.Serialize(me.{name}, {nodeName}, {serializedName}, context);");
            }
        }
    }

    private static bool FilterProcessedSyntaxNodes(SyntaxNode node, CancellationToken token)
    {
        return node is TypeDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static SerializedTypeClassInfo MakeClassInfo(GeneratorAttributeSyntaxContext syntaxContext,
        CancellationToken token)
    {
        var type = (INamedTypeSymbol)syntaxContext.TargetSymbol;
        var info = new SerializedTypeClassInfo
        {
            TypeName = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            MetadataName = type.MetadataName,
            FullyQualifiedName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            BaseTypeName = type.BaseType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsStruct = type.IsValueType,
            Namespace = type.ContainingNamespace.ToDisplayString()
        };
        
        var attr = syntaxContext.Attributes.FirstOrDefault(x =>
            x.AttributeClass?.MetadataName.Equals("SerializedTypeAttribute") ?? false);
        if (attr != null)
        {
            info.Tag = attr.FindArgument("tag").Value.ToCSharpString();
        }
        
        info.TypeParameters.AddRange(type.TypeParameters.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            SerializedFieldInfo fieldInfo;

            if (field.AssociatedSymbol is IPropertySymbol prop)
            {
                var attributes = prop.GetAttributes();
                var fieldAttr = attributes.FirstOrDefault(x =>
                    x.AttributeClass?.MetadataName.Equals("SerializedFieldAttribute") ?? false);

                if (fieldAttr == null)
                    continue;

                var name = fieldAttr.FindArgument("name").Value.ToCSharpString();

                var isList = prop.Type.AllInterfaces.Any(x => x.MetadataName == "IList`1");

                fieldInfo = new SerializedFieldInfo
                {
                    MemberName = prop.Name,
                    HasSetter = prop.SetMethod != null,
                    IsValueType = prop.Type.IsValueType,
                    MemberType = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    SerializedName = name,
                    IsList = isList,
                    TypeIsAlsoSerializable = prop.Type.GetAttributes().Any(x =>
                        x.AttributeClass?.MetadataName.Equals("SerializedTypeAttribute") ?? false)
                };

                if (isList && prop.Type is INamedTypeSymbol t)
                {
                    fieldInfo.MemberType = t.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    fieldInfo.TypeIsAlsoSerializable = t.TypeArguments[0].GetAttributes().Any(x =>
                        x.AttributeClass?.MetadataName.Equals("SerializedTypeAttribute") ?? false);
                }
            }
            else
            {
                var fieldAttr = field.GetAttributes().FirstOrDefault(x =>
                    x.AttributeClass?.MetadataName.Equals("SerializedFieldAttribute") ?? false);

                if (fieldAttr == null)
                    continue;

                var name = fieldAttr.FindArgument("name").Value.ToCSharpString();

                var isList = field.Type.AllInterfaces.Any(x => x.MetadataName == "IList`1");

                fieldInfo = new SerializedFieldInfo
                {
                    MemberName = field.Name,
                    HasSetter = true,
                    IsValueType = field.Type.IsValueType,
                    MemberType = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    SerializedName = name,
                    TypeIsAlsoSerializable = field.Type.GetAttributes().Any(x =>
                        x.AttributeClass?.MetadataName.Equals("SerializedTypeAttribute") ?? false)
                };

                if (isList && field.Type is INamedTypeSymbol t)
                {
                    fieldInfo.MemberType = t.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    fieldInfo.TypeIsAlsoSerializable = t.TypeArguments[0].GetAttributes().Any(x =>
                        x.AttributeClass?.MetadataName.Equals("SerializedTypeAttribute") ?? false);
                }
            }

            info.Fields.Add(fieldInfo);
        }

        return info;
    }
}
