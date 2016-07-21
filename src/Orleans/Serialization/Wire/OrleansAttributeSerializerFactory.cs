using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Orleans.CodeGeneration;
using Wire;
using Wire.SerializerFactories;
using Wire.ValueSerializers;

namespace Orleans.Serialization.Wire
{
    public class OrleansAttributeSerializerFactory : ValueSerializerFactory
    {
        private MethodInfo _serializerMethod;
        private MethodInfo _deserializerMethod;

        public override bool CanSerialize(Serializer serializer, Type type)
        {
            //_copierMethod = type.GetTypeInfo()
            //    .GetMethods(BindingFlagsEx.All)
            //    .FirstOrDefault(m => m.IsDefined(typeof(CopierMethodAttribute)));

            _serializerMethod = type.GetTypeInfo()
               .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
               .FirstOrDefault(m => m.IsDefined(typeof(SerializerMethodAttribute)));

            _deserializerMethod = type.GetTypeInfo()
              .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
              .FirstOrDefault(m => m.IsDefined(typeof(DeserializerMethodAttribute)));

            return _serializerMethod != null && _deserializerMethod != null;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer,type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var os = new ObjectSerializer(type);
            typeMapping.TryAdd(type, os);

            ObjectReader reader = (stream, session) =>
            {
                var bytes = stream.ReadLengthEncodedByteArray(session);
                var btr = new BinaryTokenStreamReader(bytes);
                var res = _deserializerMethod.Invoke(null, new object[] {type, btr});
                return res;
            };
            ObjectWriter writer = (stream, value, session) =>
            {
                var btw = new BinaryTokenStreamWriter();
                _serializerMethod.Invoke(null, new[] {value, btw, type});
                var bytes = btw.ToByteArray();
                stream.WriteLengthEncodedByteArray(bytes);
            };
            
            os.Initialize(reader, writer);
            return os;
        }
    }
}