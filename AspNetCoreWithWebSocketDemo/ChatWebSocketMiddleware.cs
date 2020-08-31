using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatWebSocketMiddleware
{
    public class ChatWebSocketMiddleware
    {
        private static ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _sockets = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

        private readonly RequestDelegate _next;

        public ChatWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }
            System.Net.WebSockets.WebSocket dummy;

            CancellationToken ct = context.RequestAborted;
            var currentSocket = await context.WebSockets.AcceptWebSocketAsync();

            System.Security.Principal.WindowsIdentity currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
            string socketId = currentUser.User.ToString();

            if (!_sockets.ContainsKey(socketId))
            {
                _sockets.TryAdd(socketId, currentSocket);
            }
            //_sockets.TryRemove(socketId, out dummy);
            //_sockets.TryAdd(socketId, currentSocket);

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                string response = await ReceiveStringAsync(currentSocket, ct);
                MsgTemplate msg = JsonConvert.DeserializeObject<MsgTemplate>(response);

                if (string.IsNullOrEmpty(response))
                {
                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    continue;
                }

                foreach (var socket in _sockets)
                {
                    if (socket.Value.State != WebSocketState.Open)
                    {
                        continue;
                    }
                    if (socket.Key == msg.ReceiverID || socket.Key == socketId)
                    {
                        await SendStringAsync(socket.Value, JsonConvert.SerializeObject(msg), ct);
                    }
                }
            }

            //_sockets.TryRemove(socketId, out dummy);

            await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            currentSocket.Dispose();
        }

        private static Task SendStringAsync(System.Net.WebSockets.WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<string> ReceiveStringAsync(System.Net.WebSockets.WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }

}
