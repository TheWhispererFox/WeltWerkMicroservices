using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnouncementService
{
    public static class ApplicationBuilderExtensions
    {
        private static RabbitListener Listener { get; set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app)
        {
            Listener = (RabbitListener)app.ApplicationServices.GetService(typeof(RabbitListener));
            IHostApplicationLifetime lifetime = (IHostApplicationLifetime)app.ApplicationServices.GetService(typeof(IHostApplicationLifetime));
            lifetime.ApplicationStarted.Register(OnStarted);

            lifetime.ApplicationStopping.Register(OnStopping);

            return app;
        }

        private static void OnStarted()
        {
            Listener.Register();
        }

        private static void OnStopping()
        {
            Listener.Deregister();
        }
    }
}
