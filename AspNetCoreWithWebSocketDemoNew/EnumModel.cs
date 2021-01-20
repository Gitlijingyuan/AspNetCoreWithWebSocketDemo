using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreWithWebSocketDemoNew
{
    public class EnumModel
    {
        public enum WSMessageType
        {
            登陆 = 100,
            注销 = 200,
            发送 = 300,
            重连 = 400
        }

        public enum UserType
        {
            过账客户端 = 100,
            过账数据端 = 200,
            过账数据添加端 = 210,
            过账数据删除端 = 220,
            吊挂工艺数据 = 300,
            吊挂模板数据 = 350

        }
    }
}
