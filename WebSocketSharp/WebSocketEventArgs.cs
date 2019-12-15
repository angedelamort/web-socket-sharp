using System;
using System.Net.WebSockets;

namespace WebSocketSharp
{
    public class WebSocketEventArgs : EventArgs
    {
        public WebSocketEventArgs(string socketId, WebSocket socket)
        {
            Socket = socket;
            SocketId = socketId;
        }

        public WebSocket Socket { get; }
        
        public string SocketId { get; }
    }
}
