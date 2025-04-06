namespace Radish.Serialization;

[Flags]
public enum SerializationHint
{
    None = 0,
    TextSingleLine = 1 << 0,
    XmlAttribute = 1 << 1,
}
