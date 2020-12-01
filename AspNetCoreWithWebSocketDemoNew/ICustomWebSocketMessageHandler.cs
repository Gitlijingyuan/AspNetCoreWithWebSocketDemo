using HelperAspNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AspNetCoreWithWebSocketDemoNew.EnumModel;

namespace AspNetCoreWithWebSocketDemoNew
{
    public interface ICustomWebSocketMessageHandler
    {
        Task SendInitialMessages(CustomWebSocket userWebSocket);
        Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, IAutomaticPostingFactory apFactory);
        Task BroadcastOthers(byte[] buffer, string username, ICustomWebSocketFactory wsFactory);
        Task BroadcastAll(byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory);

    }

    public class CustomWebSocketMessageHandler : ICustomWebSocketMessageHandler
    {
   
        /// <summary>
        /// 链接webSocket
        /// </summary>
        /// <param name="userWebSocket"></param>
        /// <returns></returns>
        public async Task SendInitialMessages(CustomWebSocket userWebSocket)
        {
            WebSocket webSocket = userWebSocket.WebSocket;
            var msg = new CustomWebSocketMessage
            {
                MessagDateTime = DateTime.Now,
                Type = WSMessageType.登陆,
                Text = "登录成功！",
                Username = "system",
                Url = ""
            };
 
            string serialisedMessage = JsonConvert.SerializeObject(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(serialisedMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="buffer"></param>
        /// <param name="userWebSocket"></param>
        /// <param name="wsFactory"></param>
        /// <returns></returns>
        public async Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, IAutomaticPostingFactory apFactory)
        {
            string msg = Encoding.UTF8.GetString(buffer);
            try
            {
                //var message = JsonConvert.DeserializeObject<CustomWebSocketMessage>(msg);
                String JSON = msg.Replace("\0", "");
                CustomWebSocketMessage message = JSON.ConvertToObject<CustomWebSocketMessage>();
                if (message.Text == "1")
                {
                    message.Text = HelperAspNet.Http.HttpGet("http://172.16.14.250:7031/MesStationMasterMbjGetApi?StationNo=" + message.Username);
                }
                else if (message.Text == "0")
                {
                    message.Text = HelperAspNet.Http.HttpGet("http://172.16.14.250:7030/MesStationMasterAppGetApi?StationNo=" + message.Username);
                }
                else if (message.Type == EnumModel.WSMessageType.发送 && message.UserType ==  UserType.过账客户端)
                {
                    message.Text = apFactory.AutomaticPostingAll().ConvertToJson();
                    //message.Text = HelperAspNet.Http.HttpGet("http://172.16.1.34:7777/api/GetAutomaticPosting/GetAutomaticPostingAPI");
                }
                else if (message.Type == WSMessageType.登陆)
                {
                    await userWebSocket.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }

                string serialisedMessage = JsonConvert.SerializeObject(message);
                if (message.Type == WSMessageType.发送 || message.Type == WSMessageType.重连)
                {
                    await BroadcastOthers(Encoding.UTF8.GetBytes(serialisedMessage), message.Username, wsFactory);
                }
            }
            catch (Exception e)
            {
                await userWebSocket.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }

        /// <summary>
        /// 返回信息到用户
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="userWebSocket"></param>
        /// <param name="wsFactory"></param>
        /// <returns></returns>
        public async Task BroadcastOthers(byte[] buffer, string username, ICustomWebSocketFactory wsFactory)
        {
            var others = wsFactory.Others(username);
            foreach (var uws in others)
            {
                //string msg = Encoding.UTF8.GetString(buffer);
                //String JSON = msg.Replace("\u0000", "");
                //CustomWebSocketMessage message = JSON.ConvertToObject<CustomWebSocketMessage>();

                //if (message.Type == WSMessageType.发送)
                //{


                    //string serialisedMessage = JsonConvert.SerializeObject(message);
                    //buffer = Encoding.UTF8.GetBytes(serialisedMessage);
                    await uws.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                //}


            }

            //var others = wsFactory.Client(userWebSocket.Username);

            //await others.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);

        }

        /// <summary>
        /// 全部广播
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="userWebSocket"></param>
        /// <param name="wsFactory"></param>
        /// <returns></returns>
        public async Task BroadcastAll(byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory)
        {
            var all = wsFactory.All();
            foreach (var uws in all)
            {
                await uws.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }


    }

}
