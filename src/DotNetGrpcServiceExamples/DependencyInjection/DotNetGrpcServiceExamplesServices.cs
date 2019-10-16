using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewVoiceMedia.DotNetGrpcServiceExamples.DataAccess;
using NewVoiceMedia.Messaging.NetCore.Extensions;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.DependencyInjection
{
    public static class DotNetGrpcServiceExamplesServices
    {
        public static void AddDotNetGrpcServiceExamplesServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessaging(configuration, publish: true, consume: true);
            services.Configure<DbOptions>(configuration);
            services.AddSingleton<IToDoItemRepository, ToDoItemRepository>();
        }
    }
}