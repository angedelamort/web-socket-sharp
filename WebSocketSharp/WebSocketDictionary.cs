using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace WebSocketSharp
{
    public class WebSocketDictionary : IEnumerable<KeyValuePair<string, WebSocket>>
    {
        private readonly ConcurrentDictionary<string, WebSocket> sockets = new ConcurrentDictionary<string, WebSocket>();

        public string Add(string socketId, WebSocket socket)
        {
            if (sockets.TryAdd(socketId, socket))
                return socketId;

            throw new Exception("Could not insert the key.");
        }

        public bool Remove(string socketId) => sockets.TryRemove(socketId, out var _);

        public WebSocket this[string socketId] => sockets.TryGetValue(socketId, out var socket) ? socket : null;

        public IEnumerator<KeyValuePair<string, WebSocket>> GetEnumerator() => sockets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
