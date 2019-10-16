using Microsoft.AspNetCore.Builder;

namespace NewVoiceMedia.DotNetGrpcServiceExamples {
    public static class HttpsExtensions
    {
        public static IApplicationBuilder AlwaysUseHttpsScheme(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Request.Scheme = "https";
                await next();
            });
            return app;
        }
    }
}
