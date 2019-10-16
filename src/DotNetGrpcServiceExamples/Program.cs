using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace NewVoiceMedia.DotNetGrpcServiceExamples
{
    public static class Program
    {
        public static string ServiceName { get; } = "dotnetgrpcservice";
        public static string InternalScopeName { get; } = "internal";
        public static string ScopeName { get; } = "dotnetgrpcservice-api";
        public static string SwaggerUiOauth2ClientName { get; } = "dotnetgrpcservice-swagger-ui";
        private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(25);

        public static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                BuildHost(args).Run();
            }
            catch (Exception e)
            {
                logger.Error(e, "Unhandled exception");
                throw;
            }
        }

        private static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.ListenAnyIP(context.Configuration.GetValue("STATUS_HTTP_PORT", 3000));
                    });
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                })
                .UseNLog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<HostOptions>(option =>
                    {
                        option.ShutdownTimeout = ShutdownTimeout;
                    });
                })
                .Build();
        }
    }
}
