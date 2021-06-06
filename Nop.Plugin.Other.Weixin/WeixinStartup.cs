using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Nop.Core.Infrastructure;
using Senparc.CO2NET;
using Senparc.Weixin.RegisterServices;
using Senparc.CO2NET.AspNet;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Nop.Core;
using Nop.Data;

namespace Nop.Plugin.Other.Weixin
{
    public class WeixinConfig : BaseEntity
    {
        public string AppId { get; }

        public string Secret { get; set; }

        public string Name { get; set; }
    }

    public class WeixinStartup : INopStartup
    {
        public int Order => 501;

        public void Configure(IApplicationBuilder application)
        {
            var env = application.ApplicationServices.GetRequiredService<IHostEnvironment>();
            var senparcSetting = application.ApplicationServices.GetRequiredService<IOptions<SenparcSetting>>();
            var senparcWeixinSetting = application.ApplicationServices.GetRequiredService<IOptions<SenparcWeixinSetting>>();

            // 启动 CO2NET 全局注册，必须！
            // 关于 UseSenparcGlobal() 的更多用法见 CO2NET Demo：https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore3/Startup.cs
            var registerService = application.UseSenparcGlobal(env, senparcSetting.Value, globalRegister =>
            {

            }, true);
            //使用 Senparc.Weixin SDK
            registerService.UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
            {
                using var scope = application.ApplicationServices.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<WeixinConfig>>();
                foreach (var weixinConfig in repository.Table.ToList())
                {
                    weixinRegister.RegisterMpAccount(weixinConfig.AppId, weixinConfig.Secret, weixinConfig.Name);
                }
            });
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            services.AddSenparcWeixinServices(configuration);
        }

    }
}
