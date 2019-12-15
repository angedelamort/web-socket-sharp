using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp.MessageControllers;

namespace WebSocketSharp
{
    /// <summary>
    /// If you use the MessageController mechanism, you will need to inherit from this class.
    /// </summary>
    public class WebSocketController : WebSocketHandler
    {
        public WebSocketController(WebSocketDictionary sockets) 
            : base(sockets)
        {
        }

        public async Task SendTextAsync(string socketId, IMessageRequest messageRequest)
        {
            await SendAsync(socketId, SerializeToText(messageRequest));
        }

        public async Task SendTextAsync(string socketId, IMessageResponse messageResponse)
        {
            await SendAsync(socketId, SerializeToText(messageResponse));
        }

        public async Task SendBinaryAsync(string socketId, IMessageRequest messageRequest)
        {
            await SendAsync(socketId, SerializeToBinary(messageRequest));
        }

        public async Task SendBinaryAsync(string socketId, IMessageResponse messageResponse)
        {
            await SendAsync(socketId, SerializeToBinary(messageResponse));
        }

        public override async Task OnTextAsync(WebSocketEventArgs e, string text)
        {
            var content = DeserializeFromText(text);
            var response = await MessageControllerFactory.ProcessAsync(content);
            if (response != null)
                await SendTextAsync(e.SocketId, response);
        }

        public override async Task OnBinaryAsync(WebSocketEventArgs e, byte[] buffer)
        {
            var content = DeserializeFromBinary(buffer);
            var response = await MessageControllerFactory.ProcessAsync(content);
            if (response != null)
                await SendTextAsync(e.SocketId, response);
        }

        private static string SerializeToText(object message)
        {
            return JsonConvert.SerializeObject(new
            {
                type = message.GetType().AssemblyQualifiedName,
                content = JsonConvert.SerializeObject(message)
            });
        }

        private static object DeserializeFromText(string text)
        {
            dynamic message = JObject.Parse(text);
            string typeString = message.type?.Value;
            return JsonConvert.DeserializeObject((string)message.content?.Value, Type.GetType(typeString));
        }

        private static string SerializeToBinary(object message)
        {
            // Need to create a map of types with their fields in MessageControllerFactory()
            // Need to encode the type in CRC32?
            // Need to encode the fields in CRC16?
            // iterate on all public properties. No attributes at beginning. Might be harder for arrays and objects.
            //      write bool as 1 bit, char as 2 bits, etc...
            // maybe zip it?
            throw new NotImplementedException();
        }

        private static object DeserializeFromBinary(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
