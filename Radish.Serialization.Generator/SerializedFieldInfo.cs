namespace Radish.Serialization;

internal class SerializedFieldInfo
{
    public string MemberName { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public string SerializedName { get; set; } = string.Empty;
    public bool TypeIsAlsoSerializable { get; set; }
    public bool HasSetter { get; set; }
    public bool IsValueType { get; set; }
    public bool IsList { get; set; }
}
