using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewVoiceMedia.AspNetCore.Authentication;
using NewVoiceMedia.AspNetCore.Authentication.OidcAuth;
using NewVoiceMedia.AspNetCore.Mvc;
using NewVoiceMedia.AspNetCore.Mvc.Filters;
using NewVoiceMedia.AspNetCore.Mvc.Json;
using NewVoiceMedia.AspNetCore.Mvc.Metrics;
using NewVoiceMedia.AspNetCore.Mvc.Swagger;
using NewVoiceMedia.DotNetGrpcServiceExamples.ClrCounters;
using NewVoiceMedia.DotNetGrpcServiceExamples.DependencyInjection;
using NLog;
using NLog.Web;

namespace NewVoiceMedia.DotNetGrpcServiceExamples
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            // Get logging up and running ASAP
            ConfigureLogging(env);

            var latencyMode = System.Runtime.GCSettings.LatencyMode;
            var isServerGC = System.Runtime.GCSettings.IsServerGC;
            var compactionMode = System.Runtime.GCSettings.LargeObjectHeapCompactionMode;
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"GC settings: latencymode {latencyMode} - isServerGC {isServerGC} - compactionMode {compactionMode}");
            var memoryInfo = GC.GetGCMemoryInfo();
            logger.Info($"Total Available Memory: {memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024}MB");
            logger.Info($"High Memory Load Threshold: {memoryInfo.HighMemoryLoadThresholdBytes / 1024 / 1024}MB");
            logger.Info($"ProcessorCount: {Environment.ProcessorCount} 64-bit: {Environment.Is64BitProcess}");
            if (Directory.Exists("/sys/fs/cgroup"))
            {
                logger.Info($"/sys/fs/cgroup/memory/memory.max_usage_in_bytes: {File.ReadAllText("/sys/fs/cgroup/memory/memory.max_usage_in_bytes")}");
                logger.Info($"/sys/fs/cgroup/memory/memory.limit_in_bytes: {File.ReadAllText("/sys/fs/cgroup/memory/memory.limit_in_bytes")}");
                logger.Info($"/sys/fs/cgroup/cpu/cpu.cfs_quota_us: {File.ReadAllText("/sys/fs/cgroup/cpu/cpu.cfs_quota_us")}");
                logger.Info($"/sys/fs/cgroup/cpu/cpu.cfs_period_us: {File.ReadAllText("/sys/fs/cgroup/cpu/cpu.cfs_period_us")}");
                logger.Info($"/sys/fs/cgroup/cpu/cpu.shares: {File.ReadAllText("/sys/fs/cgroup/cpu/cpu.shares")}");
            }
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNvmAuthentication();
            services.AddAuthorization(AuthPolicies.AddPolicies);
            services.AddNvmApiVersioning();
            services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter(AuthPolicies.Global));
                options.Filters.Add(new AddCorrelationIdToNLogFilter());
                options.Filters.Add(new ActionMetricsFilter());
            }).AddNvmJsonOptions();

            var scopes = new Dictionary<string, string>
            {
                { Program.ScopeName, "Access the " + Program.ServiceName + " API on your behalf" },
                { Program.InternalScopeName, "Access to _info API for internal users" }
            };
            services.AddSwagger(Configuration.GetOidcIssuers().First(), Program.ServiceName, scopes);
            services.AddStatsD(Configuration, Program.ServiceName);
            services.AddDotNetGrpcServiceExamplesServices(Configuration);
            services.AddHostedService<ClrEventsService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            // This is somewhat of a hack
            // It is due to the fact the hitch does not send the X-Forwarded-Proto header, but we know that the
            // requests are always coming in on https, so we set this for every request so that the Location header
            // value will get populated correctly when returning a CreatedAt response.
            app.AlwaysUseHttpsScheme();
            app.UseRequestMetrics();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwaggerUi(Program.ServiceName, Program.SwaggerUiOauth2ClientName);
        }

        private static void ConfigureLogging(IHostingEnvironment env)
        {
            env.ConfigureNLog(env.IsDevelopment() ? "nlog.Development.config" : "nlog.Production.config");
        }
    }
}