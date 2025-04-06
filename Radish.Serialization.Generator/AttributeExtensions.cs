using Microsoft.CodeAnalysis;

namespace Radish.Serialization;

public static class AttributeExtensions
{
    public static TypedConstant? FindArgument(this AttributeData attr, string name)
    {
        foreach (var namedArg in attr.NamedArguments)
        {
            if (name.Equals(namedArg.Key))
                return namedArg.Value;
        }

        var ctor = attr.AttributeConstructor;
        if (ctor != null)
        {
            for (var i = 0; i < ctor.Parameters.Length; ++i)
            {
                if (ctor.Parameters[i].Name.Equals(name))
                    return attr.ConstructorArguments[i];
            }
        }

        return null;
    }
}