using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebSocketSharp
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly WebSocketHandler webSocketHandler;

        public WebSocketManagerMiddleware(RequestDelegate next,
            WebSocketHandler webSocketHandler)
        {
            this.next = next;
            this.webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var socketId = Guid.NewGuid().ToString(); // TODO: find a way to bind the socket ID to a connection/User
                await webSocketHandler.ConnectAsync(socketId, socket);
                await webSocketHandler.ReceiveLoopAsync(socketId, socket);
            }

            //TODO - investigate the Kestrel exception thrown when this is the last middleware
            //next.Invoke(context);
        }
    }
}
