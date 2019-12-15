using System.IO;
using NUnit.Framework;
using WebSocketSharp.Serializer;

namespace WebSocketSharp.UnitTests
{
    public class BinarySerializerTests
    {
        public class SerializationTest
        {
            public int MyInt { get; set; } = 12;
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var obj = new SerializationTest();
            using var stream = new MemoryStream();
            using var serializer = new BinarySerializer();
            serializer.Serialize(stream, obj);

            stream.Position = 0;
            var dObj = (SerializationTest)serializer.Deserialize(stream);

            Assert.AreEqual(obj.MyInt, dObj.MyInt);
        }
    }
}