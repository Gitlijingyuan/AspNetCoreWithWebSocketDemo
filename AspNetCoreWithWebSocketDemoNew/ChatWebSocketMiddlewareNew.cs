using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets;

namespace AspNetCoreWithWebSocketDemoNew
{
    public static class WebSocketExtensions
    {
        public static IApplicationBuilder UseCustomWebSocketManager(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomWebSocketManager>();
        }
    }

    public class CustomWebSocketMessage
    {
        public string Text { get; set; }
        public DateTime MessagDateTime { get; set; }
        public string Username { get; set; }
        public WSMessageType Type { get; set; }

    }
        public class CustomWebSocket
    {
        public WebSocket WebSocket { get; set; }
        public string Username { get; set; }
    }

    public class CustomWebSocketManager
    {
        private readonly RequestDelegate _next;

        public CustomWebSocketManager(RequestDelegate next)
        {
            _next = next;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wsFactory"></param>
        /// <param name="wsmHandler"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
        {
            if (context.Request.Path == "/MesServiceStation")
            {


                if (context.WebSockets.IsWebSocketRequest)
                {

                    System.Security.Principal.WindowsIdentity currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
                    string username = currentUser.User.ToString();
                    //string username = context.Request.Query["u"];

                    if (!string.IsNullOrEmpty(username))
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        CustomWebSocket userWebSocket = new CustomWebSocket()
                        {
                            WebSocket = webSocket,
                            Username = username
                        };
                        wsFactory.Add(userWebSocket);
                        await wsmHandler.SendInitialMessages(userWebSocket);
                        await Listen(context, userWebSocket, wsFactory, wsmHandler);    
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            await _next(context);
        }

        /// <summary>
        /// 侦听
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userWebSocket"></param>
        /// <param name="wsFactory"></param>
        /// <param name="wsmHandler"></param>
        /// <returns></returns>
        private async Task Listen(HttpContext context, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
        {
            WebSocket webSocket = userWebSocket.WebSocket;
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await wsmHandler.HandleMessage(result, buffer, userWebSocket, wsFactory);
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            wsFactory.Remove(userWebSocket.Username);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

}
