using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;


namespace AspNetCoreWithWebSocketDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // 此方法由运行时调用。使用此方法可将服务添加到容器中。
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // 此方法由运行时调用。配置此方法的HTTP管道。
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseWebSockets();
            app.UseMiddleware<ChatWebSocketMiddleware>();

            //务必在mvc中间件之前进行配置。
            //app.UseWebSockets();
            //app.Use(async (context, next) =>
            //{
            //    if (context.WebSockets.IsWebSocketRequest)
            //    {
            //        using (IServiceScope scope = app.ApplicationServices.CreateScope())
            //        {
            //            //do something 
            //            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            //            await Echo(context, webSocket);
            //        }
            //    }
            //    else
            //    {
            //        //移交给下一个中间件
            //        await next();
            //    }
            //});
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                string str = System.Text.Encoding.UTF8.GetString(buffer);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
