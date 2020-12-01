using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HelperAspNet
{
   public class Http
    {
        /// <summary>
        /// GET方式调用
        /// </summary>
        /// <param name="url">访问的URL</param>
        /// <returns>HTTP回传的字符串</returns>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            //转成 Base64 形式的 System.String  

            request.Method = "GET";
            request.ContentType = "application/json";
            //string data = "{\n\"header\": {\n\"token\": \"30xxx6aaxxx93ac8cxx8668xx39xxxx\",\n\"username\": \"jdads\",\n\"password\": \"liuqiangdong2010\",\n\"action\": \"\"\n},\n\"body\": {}\n}";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 调用HTTP方式的接口
        /// </summary>
        /// <param name="url">访问的URL</param>
        /// <param name="data">要传递的数据的字符串</param>
        /// <param name="SetMethod">发送的模式（PUT，POST）</param>
        /// <param name="userAndPassword">用户名加密码中间用:隔开,如果没有请输入""</param>
        /// <returns>HTTP回传的字符串</returns>
        public static string HttpPost(string url, string data, string SetMethod, string userAndPassword)
        {

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;


            if (userAndPassword.Trim().Length != 0)
            {


                string a = userAndPassword;
                byte[] b = System.Text.Encoding.Default.GetBytes(a);
                //转成 Base64 形式的 System.String  
                a = "Basic " + Convert.ToBase64String(b);




                request.Headers.Set(HttpRequestHeader.Authorization, a);
            }


            request.Method = SetMethod;
            request.ContentType = "application/json";
            //string data = "{\n\"header\": {\n\"token\": \"30xxx6aaxxx93ac8cxx8668xx39xxxx\",\n\"username\": \"jdads\",\n\"password\": \"liuqiangdong2010\",\n\"action\": \"\"\n},\n\"body\": {}\n}";

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = byteData.Length;
            request.Timeout = 1000 * 60 * 4;

            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                return reader.ReadToEnd();
            }
        }
    }
}
