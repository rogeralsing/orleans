using System;
using System.Collections.Concurrent;
using System.Linq;
using Wire;
using Wire.SerializerFactories;
using Wire.ValueSerializers;

namespace Orleans.Serialization.Wire
{
    //Just to show that types can be customized on the wire if needed
    public class ByteArraySegmentSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type == typeof(ArraySegment<byte>);
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer,type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var s = new ObjectSerializer(type);
            ObjectReader reader = (stream, session) =>
            {
                var bytes = stream.ReadLengthEncodedByteArray(session);
                return new ArraySegment<byte>(bytes);
            };
            ObjectWriter writer = (stream, value, session) =>
            {
                var segment = (ArraySegment<byte>) value;
                var bytes = segment.ToArray();
                stream.WriteLengthEncodedByteArray(bytes);
            };
            typeMapping.TryAdd(type, s);
            s.Initialize(reader, writer);
            return s;
        }
    }
}