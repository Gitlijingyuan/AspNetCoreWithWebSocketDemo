using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreWithWebSocketDemoNew
{
    public class WebSocketsList
    {
    }

    #region CustomWebSocket接口
    public interface ICustomWebSocketFactory
    {


        void Add(CustomWebSocket uws);
        void Remove(string username);
        List<CustomWebSocket> All();
        List<CustomWebSocket> Others(string username);
        CustomWebSocket Client(string username);

    }

    #endregion

    #region AutomaticPosting接口

    public interface IAutomaticPostingFactory
    {
  
        void AutomaticPostingAdd(AutomaticPosting automaticposting);
        void AutomaticPostingRemove(AutomaticPosting automaticposting);
        List<AutomaticPosting> AutomaticPostingAll();
        List<AutomaticPosting> AutomaticPostingOthers(AutomaticPosting automaticposting);
        AutomaticPosting AutomaticPostingClient(AutomaticPosting automaticposting);

    }
    #endregion

    #region CustomWebSocketFactory

    public class CustomWebSocketFactory : ICustomWebSocketFactory
    {
        List<CustomWebSocket> List;
    

        public CustomWebSocketFactory()
        {
            List = new List<CustomWebSocket>();
            
        }

        #region 操作CustomWebSocket

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

        #endregion

   

    }

    #endregion

    #region AutomaticPostingFactory

    public class AutomaticPostingFactory : IAutomaticPostingFactory
    {
        List<AutomaticPosting> AutomaticPostingList;
        public AutomaticPostingFactory()
        {
            AutomaticPostingList = new List<AutomaticPosting>();
        }
        #region 操作AutomaticPosting

        /// <summary>
        /// 添加过账行到List
        /// </summary>
        /// <param name="automaticposting"></param>
        public void AutomaticPostingAdd(AutomaticPosting automaticposting)
        {
            AutomaticPostingList.Add(automaticposting);
        }

        /// <summary>
        /// 删除过账行
        /// </summary>
        /// <param name="automaticposting"></param>
        public void AutomaticPostingRemove(AutomaticPosting automaticposting)
        {
            AutomaticPostingList.Remove(AutomaticPostingClient(automaticposting));
        }

        /// <summary>
        /// 取得全部过账行
        /// </summary>
        /// <returns></returns>
        public List<AutomaticPosting> AutomaticPostingAll()
        {
            return AutomaticPostingList;
        }

        /// <summary>
        /// 取得过账行
        /// </summary>
        /// <param name="automaticposting"></param>
        /// <returns></returns>
        public List<AutomaticPosting> AutomaticPostingOthers(AutomaticPosting automaticposting)
        {
            return AutomaticPostingList.Where(c => c.JobNumber == automaticposting.JobNumber && c.item_num == automaticposting.item_num && c.Warehouse_ == automaticposting.Warehouse_).ToList();
        }

        /// <summary>
        /// 定位过账行
        /// </summary>
        /// <param name="automaticposting"></param>
        /// <returns></returns>
        public AutomaticPosting AutomaticPostingClient(AutomaticPosting automaticposting)
        {
            return AutomaticPostingList.First(c => c.JobNumber == automaticposting.JobNumber && c.item_num == automaticposting.item_num && c.Warehouse_ == automaticposting.Warehouse_);
        }

        #endregion
    }

    #endregion

}
