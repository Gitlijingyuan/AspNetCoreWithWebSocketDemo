using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HelperAspNet;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace AspNetCoreWithWebSocketDemoNew
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Helperlog4.Configure(); //使用前先配置
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSingleton<ICustomWebSocketFactory, CustomWebSocketFactory>();
            services.AddSingleton<IAutomaticPostingFactory, AutomaticPostingFactory>();
            services.AddSingleton<ICustomWebSocketMessageHandler, CustomWebSocketMessageHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAutomaticPostingFactory apFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            app.UseCustomWebSocketManager();
            //app.AutomaticPostingApp();
            string Text = HelperAspNet.Http.HttpGet("http://172.16.1.34:7777/api/GetAutomaticPosting/GetAutomaticPostingAPI");

            List<AutomaticPosting> AutomaticPostingList = new List<AutomaticPosting>();
            AutomaticPostingList = Text.ConvertToList<AutomaticPosting>();

            foreach (AutomaticPosting item in AutomaticPostingList)
            {
                Helperlog4.Info("取得未过账数据"+item.ConvertToJson());
                apFactory.AutomaticPostingAdd(item);
            }
            Helperlog4.Info("取得未过账数据" + AutomaticPostingList.ConvertToJson());

            if (AutomaticPostingList.Count == 0)
            {
                AutomaticPosting AutomaticPostings = new AutomaticPosting();

                AutomaticPostings.id = 0;
                AutomaticPostings.in_time = null;
                AutomaticPostings.item_num = null;
                AutomaticPostings.JobNumber = null;
                AutomaticPostings.state = 0;
                AutomaticPostings.Warehouse_ = null;
                apFactory.AutomaticPostingAdd(AutomaticPostings);
            }

        }

    }
}
