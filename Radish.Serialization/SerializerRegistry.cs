using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Radish.Serialization.Serializers;

namespace Radish.Serialization;

public static class SerializerRegistry
{
    private static readonly Dictionary<Type, object> PrimitiveSerializers = [];

    static SerializerRegistry()
    {
        RegisterPrimitiveSerializer<string, StringSerializer>();

        RegisterPrimitiveSerializer<byte, NumberSerializer<byte>>();
        RegisterPrimitiveSerializer<sbyte, NumberSerializer<sbyte>>();
        RegisterPrimitiveSerializer<char, NumberSerializer<char>>();

        RegisterPrimitiveSerializer<short, NumberSerializer<short>>();
        RegisterPrimitiveSerializer<ushort, NumberSerializer<ushort>>();
        RegisterPrimitiveSerializer<int, NumberSerializer<int>>();
        RegisterPrimitiveSerializer<uint, NumberSerializer<uint>>();
        RegisterPrimitiveSerializer<long, NumberSerializer<long>>();
        RegisterPrimitiveSerializer<ulong, NumberSerializer<ulong>>();
        RegisterPrimitiveSerializer<float, NumberSerializer<float>>();
        RegisterPrimitiveSerializer<double, NumberSerializer<double>>();

        RegisterPrimitiveSerializer<Vector2, Vector2Serializer>();
    }

    public static void RegisterPrimitiveSerializer<T, TSerializer>() 
        where TSerializer : IPrimitiveSerializer<T>, new()
    {
        PrimitiveSerializers.Add(typeof(T), new TSerializer());
    }

    internal static bool TryFindSerializer<T>([NotNullWhen(true)] out IPrimitiveSerializer<T>? serializer)
    {
        if (PrimitiveSerializers.TryGetValue(typeof(T), out var value))
        {
            if (value is IPrimitiveSerializer<T> s)
            {
                serializer = s;
                return true;
            }
        }

        serializer = null;
        return false;
    }
}
