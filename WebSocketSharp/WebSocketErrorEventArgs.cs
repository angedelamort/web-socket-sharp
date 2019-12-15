using System;
using System.Net.WebSockets;

namespace WebSocketSharp
{
    public class WebSocketErrorEventArgs : WebSocketEventArgs
    {
        public Exception Exception { get; }

        public WebSocketErrorEventArgs(string socketId, WebSocket socket, Exception exception) 
            : base(socketId, socket)
        {
            Exception = exception;
        }
    }
}
