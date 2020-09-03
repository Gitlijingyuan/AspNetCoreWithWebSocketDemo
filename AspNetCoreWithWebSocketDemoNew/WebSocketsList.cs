using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreWithWebSocketDemoNew
{
    public class WebSocketsList
    {
    }
    public interface ICustomWebSocketFactory
    {
        void Add(CustomWebSocket uws);
        void Remove(string username);
        List<CustomWebSocket> All();
        List<CustomWebSocket> Others(string username);
        CustomWebSocket Client(string username);
    }

    public class CustomWebSocketFactory : ICustomWebSocketFactory
    {
        List<CustomWebSocket> List;

        public CustomWebSocketFactory()
        {
            List = new List<CustomWebSocket>();
        }
        /// <summary>
        /// 添加链接到List
        /// </summary>
        /// <param name="uws"></param>
        public void Add(CustomWebSocket uws)
        {
            List.Add(uws);
        }

        /// <summary>
        /// 断开链接
        /// </summary>
        /// <param name="username"></param>
        public void Remove(string username)
        {
            List.Remove(Client(username));
        }

        /// <summary>
        /// 取得全部链接
        /// </summary>
        /// <returns></returns>
        public List<CustomWebSocket> All()
        {
            return List;
        }

        /// <summary>
        /// 取得链接
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public List<CustomWebSocket> Others(string username)
        {
            return List.Where(c => c.Username == username).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public CustomWebSocket Client(string username)
        {
            return List.First(c => c.Username == username);
        }
    }

}
