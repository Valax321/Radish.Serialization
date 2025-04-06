using System.Numerics;

namespace Radish.Serialization.Serializers;

internal sealed class NumberSerializer<T> : IPrimitiveSerializer<T> where T : INumber<T>
{
    public void Serialize(in T value, string name, IObjectNode parent)
    {
        var valueNode = parent.AddChildValue(name);
        valueNode.SetValue(in value);
    }
}
