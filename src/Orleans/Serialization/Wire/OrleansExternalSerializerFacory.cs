using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wire;
using Wire.SerializerFactories;
using Wire.ValueSerializers;

namespace Orleans.Serialization.Wire
{
    public class OrleansExternalSerializerFacory : ValueSerializerFactory
    {
        private readonly IReadOnlyList<IExternalSerializer> _serializers;

        public OrleansExternalSerializerFacory(IReadOnlyList<IExternalSerializer> serializers)
        {
            _serializers = serializers;
        }
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return _serializers.Any(es => es.IsSupportedType(type));
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer,type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var ex = _serializers.First(es => es.IsSupportedType(type));
            var s = new ObjectSerializer(type);
            ObjectReader reader = (stream, session) =>
            {
                var bytes = stream.ReadLengthEncodedByteArray(session);
                var btr = new BinaryTokenStreamReader(bytes);
                var obj = ex.Deserialize(type, btr);
                return obj;
            };
            ObjectWriter writer = (stream, value, session) =>
            {
                var btw = new BinaryTokenStreamWriter();
                ex.Serialize(value,btw,type);
                var bytes = btw.ToByteArray();
                stream.WriteLengthEncodedByteArray(bytes);
            };
            typeMapping.TryAdd(type, s);
            s.Initialize(reader,writer);
            return s;
        }
    }
}