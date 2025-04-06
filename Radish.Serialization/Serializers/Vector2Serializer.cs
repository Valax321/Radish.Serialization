using System.Numerics;

namespace Radish.Serialization.Serializers;

internal sealed class Vector2Serializer : IPrimitiveSerializer<Vector2>
{
    public void Serialize(in Vector2 value, string name, IObjectNode parent)
    {
        var vec = parent.AddChildObject(name, "vec2");
        vec.Hints |= SerializationHint.TextSingleLine;

        var x = vec.AddChildValue("x");
        x.Hints |= SerializationHint.XmlAttribute;
        x.SetValue(value.X);

        var y = vec.AddChildValue("y");
        y.Hints |= SerializationHint.XmlAttribute;
        y.SetValue(value.Y);
    }
}
