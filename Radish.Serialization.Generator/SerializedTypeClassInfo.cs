using System;
using System.Collections.Generic;

namespace Radish.Serialization;

internal class SerializedTypeClassInfo
{
    public string TypeName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string MetadataName { get; set; } = string.Empty;
    public string FullyQualifiedName { get; set; } = string.Empty;
    public string? BaseTypeName { get; set; }
    public bool IsStruct { get; set; }
    public string Tag { get; set; } = string.Empty;
    public List<SerializedFieldInfo> Fields { get; } = [];
    public List<string> TypeParameters { get; } = [];

    public SerializedFieldInfo MakeSpecializedCopy()
    {
        if (TypeParameters.Count == 0)
            throw new InvalidOperationException();

        throw new NotImplementedException();
    }
}
