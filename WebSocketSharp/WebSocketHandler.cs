using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketSharp
{
    public abstract class WebSocketHandler
    {
        protected readonly WebSocketDictionary Sockets;
        private readonly int bufferLength = 4 * 1024;

        // TODO: should we add a cancellation token?
        //private readonly CancellationToken cancellationToken = new CancellationToken();

        protected WebSocketHandler(WebSocketDictionary sockets)
        {
            Sockets = sockets;
        }

        /// <summary>
        /// Connect to the WebSocket.
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <remarks>
        /// The socket must be opened before.
        /// </remarks>
        public Task ConnectAsync(string socketId, WebSocket socket)
        {
            if (socket.State != WebSocketState.Open)
                throw new Exception("The socket must be opened before calling this method.");

            Sockets.Add(socketId, socket);
            return OnOpenAsync(new WebSocketEventArgs(socketId, socket));
        }

        public async Task CloseAsync(string socketId, string message = "Closing")
        {
            await Sockets[socketId].CloseAsync(WebSocketCloseStatus.NormalClosure, message, CancellationToken.None);
            Sockets.Remove(socketId);
        }

        public virtual async Task OnOpenAsync(WebSocketEventArgs e) => await Task.CompletedTask;
        public virtual async Task OnTextAsync(WebSocketEventArgs e, string text) => await Task.CompletedTask;
        public virtual async Task OnBinaryAsync(WebSocketEventArgs e, byte[] buffer) => await Task.CompletedTask;
        public virtual async Task OnCloseAsync(WebSocketEventArgs e) => await Task.CompletedTask;
        // TODO: call it when necessary.
        public virtual async Task OnErrorAsync(WebSocketErrorEventArgs e) => await Task.CompletedTask;

        public Task SendAsync(string socketId, string message)
        {
            return SendAsync(Sockets[socketId], Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text);
        }

        public Task SendAsync(string socketId, byte[] buffer)
        {
            return SendAsync(Sockets[socketId], buffer, WebSocketMessageType.Binary);
        }

        public Task SendAsync(IEnumerable<string> sockets, string message)
        {
            return Task.WhenAll(sockets.Select(socketId => SendAsync(socketId, message)).ToList());
        }

        public Task SendAsync(IEnumerable<string> sockets, byte[] buffer)
        {
            return Task.WhenAll(sockets.Select(socketId => SendAsync(socketId, buffer)).ToList());
        }

        public Task SendToAllAsync(string message)
        {
            return Task.WhenAll(Sockets.Select(kvp => SendAsync(kvp.Key, message)).ToList());
        }

        public Task SendToAllAsync(byte[] buffer)
        {
            return Task.WhenAll(Sockets.Select(kvp => SendAsync(kvp.Key, buffer)).ToList());
        }

        private async Task SendAsync(WebSocket socket, byte[] message, WebSocketMessageType type)
        {
            if (socket.State != WebSocketState.Open)
                return;

            for (var offset = 0; offset < message.Length; offset += bufferLength)
            {
                var remainingBuffer = message.Length - offset;
                var buffer = new ArraySegment<byte>(message, offset, Math.Min(bufferLength, remainingBuffer));

                var isLastLoop = offset + bufferLength >= message.Length;
                await socket.SendAsync(buffer, type, isLastLoop, CancellationToken.None);
            }
        }

        public async Task ReceiveLoopAsync(string socketId, WebSocket socket)
        {
            var memoryStream = new MemoryStream();
            var buffer = new byte[bufferLength];
            var segment = new ArraySegment<byte>(buffer);

            do
            {
                WebSocketReceiveResult result;
                memoryStream.Position = 0;
                do
                {
                    result = await socket.ReceiveAsync(segment, CancellationToken.None);
                    await memoryStream.WriteAsync(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var str = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                        await OnTextAsync(new WebSocketEventArgs(socketId, socket), str);
                        break;

                    case WebSocketMessageType.Binary:
                        await OnBinaryAsync(new WebSocketEventArgs(socketId, socket), memoryStream.ToArray());
                        break;

                    case WebSocketMessageType.Close:
                        if (socket.State == WebSocketState.CloseReceived)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        }
                        await OnCloseAsync(new WebSocketEventArgs(socketId, socket));
                        break;
                }
            } while (socket.State == WebSocketState.Open);
        }
    }
}
