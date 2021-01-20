using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Http.Extensions;
using static AspNetCoreWithWebSocketDemoNew.EnumModel;
using HelperAspNet;
using log4net;

namespace AspNetCoreWithWebSocketDemoNew
{

    #region 注册APP

    public static class WebSocketExtensions
    {
        public static IApplicationBuilder UseCustomWebSocketManager(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomWebSocketManager>();
        }
        public static IApplicationBuilder HeartbeatStateS(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HeartbeatStateS>();
        }
        public static IApplicationBuilder AutomaticPostingApp(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AutomaticPostingApp>();
        }

    }

    #endregion

    #region 模型

    /// <summary>
    /// Socket传输模型
    /// </summary>
    public class CustomWebSocketMessage
    {
        public string Text { get; set; }
        public DateTime MessagDateTime { get; set; }
        public string Username { get; set; }
        public WSMessageType Type { get; set; }
        public string Url { get; set; }
        public UserType UserType { get; set; }

    }

    /// <summary>
    /// Socket连接模型
    /// </summary>
    public class CustomWebSocket
    {
        public WebSocket WebSocket { get; set; }
        public string Username { get; set; }
        public string UseSid { get; set; }
        public string Url { get; set; }
        public UserType UserType { get; set; }
    }

    /// <summary>
    /// 过账模型
    /// </summary>
    public class AutomaticPosting
    {
        public int id { get; set; }
        public string JobNumber { get; set; }
        public string item_num { get; set; }
        public string Warehouse_ { get; set; }
        public int state { get; set; }
        public DateTime? in_time { get; set; }
    }

    #endregion

    #region 连接和侦听

    public class CustomWebSocketManager
    {
        private readonly RequestDelegate _next;

        public CustomWebSocketManager(RequestDelegate next)
        {
            _next = next;

        }

        #region 连接

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wsFactory"></param>
        /// <param name="wsmHandler"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler, IAutomaticPostingFactory apFactory)
        {
            if (context.Request.Path == "/MesServiceStation")
            {

                Helperlog4.Info("新连接完成" + context.Request.Path.ToString());
                if (context.WebSockets.IsWebSocketRequest)
                {

                    System.Security.Principal.WindowsIdentity currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
                    string usesid = currentUser.User.ToString();
                    string customwebsocketS = context.Request.Query["username"];
                    //string username = usesid + "------Name";
                    if (!string.IsNullOrEmpty(customwebsocketS))
                    {
                        CustomWebSocket customwebsocket = customwebsocketS.ConvertToObject<CustomWebSocket>();
                         WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        List<CustomWebSocket> CustomWebSocketList = new List<CustomWebSocket>();
                        CustomWebSocketList = wsFactory.Others(customwebsocket.Username);
                        if (CustomWebSocketList.Count > 0)
                        {
                            CustomWebSocket userWebSocket1 = new CustomWebSocket();
                            userWebSocket1 = CustomWebSocketList[0];
                            wsFactory.Remove(userWebSocket1.Username);
                            CustomWebSocket userWebSocket = new CustomWebSocket()
                            {
                                WebSocket = webSocket,
                                Username = userWebSocket1.Username,
                                UseSid = usesid,
                                Url= context.Request.GetDisplayUrl(),
                                UserType = userWebSocket1.UserType

                            };
                            wsFactory.Add(userWebSocket);
                            //await Heartbeat(wsFactory, wsmHandler);
                            Helperlog4.Info("重新连接返回值" + userWebSocket.ConvertToJson());
                            await wsmHandler.SendInitialMessages(userWebSocket);
                            await Listen(context, userWebSocket, wsFactory, wsmHandler, apFactory);
                        
                        }
                        else
                        {
                            CustomWebSocket userWebSocket = new CustomWebSocket()
                            {
                                WebSocket = webSocket,
                                Username = customwebsocket.Username,
                                UseSid = usesid,
                                Url = context.Request.GetDisplayUrl(),
                                UserType = customwebsocket.UserType
                            };
                            wsFactory.Add(userWebSocket);

                            Helperlog4.Info("新连接返回值" + userWebSocket.ConvertToJson());
                            //await Heartbeat(wsFactory, wsmHandler);
                            await wsmHandler.SendInitialMessages(userWebSocket);
                            await Listen(context, userWebSocket, wsFactory, wsmHandler, apFactory);
                            
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            await _next(context);
        }

        #endregion

        #region 侦听

        /// <summary>
        /// 侦听
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userWebSocket"></param>
        /// <param name="wsFactory"></param>
        /// <param name="wsmHandler"></param>
        /// <returns></returns>
        private async Task Listen(HttpContext context, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler, IAutomaticPostingFactory apFactory)
        {
            WebSocket webSocket = userWebSocket.WebSocket;
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await wsmHandler.HandleMessage(result, buffer, userWebSocket, wsFactory, apFactory);
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            wsFactory.Remove(userWebSocket.Username);
            Helperlog4.Info("删除连接值" + userWebSocket.Username);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        #endregion



    }

    #endregion

    #region 心跳

    public class HeartbeatStateS
    {

        #region 心跳类

        /// <summary>
        /// 心跳类
        /// </summary>
        public async Task Invoke(string Username, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
        {
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(1000);

                    List<CustomWebSocket> CustomWebSocketList = new List<CustomWebSocket>();
                    CustomWebSocketList = wsFactory.Others(Username);

                    foreach (CustomWebSocket item in CustomWebSocketList)
                    {
                        if (item.WebSocket.State == WebSocketState.Aborted || item.WebSocket.State == WebSocketState.Closed)
                        {
                            wsFactory.Remove(item.Username);
                            ClientWebSocket _webSocket = new ClientWebSocket();
                            CustomWebSocket userWebSocket = new CustomWebSocket()
                            {
                                WebSocket = _webSocket,
                                Username = item.Username,
                                UseSid = item.UseSid,
                                Url = item.Url
                            };
                            wsFactory.Add(userWebSocket);
                            await wsmHandler.SendInitialMessages(userWebSocket);
                            //await Listen(context, userWebSocket, wsFactory, wsmHandler);
                        }

                    }

                }
                catch (WebSocketException e)
                {
                    // 产生 10035 == WSAEWOULDBLOCK 错误，说明被阻止了，但是还是连接的
                    //if (e.NativeErrorCode.Equals(10035))
                    //{
                    //    return true;
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                }
                finally
                {
                    //socket.Blocking = blockingState;    // 恢复状态
                }



            }

        }

        #endregion

    }

    #endregion

    #region 过账数据初始化

    /// <summary>
    /// 过账数据初始化
    /// </summary>
    public class AutomaticPostingApp
    {

        #region 过账数据初始化
        //public async Task Invoke(IAutomaticPostingFactory apFactory)
        //{
        //    string Text = HelperAspNet.Http.HttpGet("http://172.16.1.34:7777/api/GetAutomaticPosting/GetAutomaticPostingAPI");

        //    List<AutomaticPosting> AutomaticPostingList = new List<AutomaticPosting>();
        //    AutomaticPostingList = Text.ConvertToList<AutomaticPosting>();
        //    foreach (AutomaticPosting item in AutomaticPostingList)
        //    {
        //        apFactory.AutomaticPostingAdd(item);
        //    }
        //}


    #endregion

}

    #endregion

}
