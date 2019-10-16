using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Controllers
{
    [Route("/")]
    [ApiVersionNeutral]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("_status")]
        public ActionResult<Status> GetStatus()
        {
            var response = new Status
            {
                Version = BuildInfo.Version,
                ServiceName = Program.ServiceName,
                Healthy = IsServiceHealthy(),
                Hostname = GetObfuscatedHostname()
            };

            if (response.Healthy)
            {
                return response;
            }

            return StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
        }

        [Authorize(AuthPolicies.Internal)]
        [HttpGet("_info")]
        public ActionResult<Info> GetInfo()
        {
            return new Info
            {
                Version = BuildInfo.Version,
                Hostname = Environment.MachineName
            };
        }

        public class Status
        {
            public string Version { get; set; }
            public string ServiceName { get; set; }
            public bool Healthy { get; set; }
            public string Hostname { get; set; }
        }

        public class Info
        {
            public string Version { get; set; }
            public string Hostname { get; set; }
        }

        private bool IsServiceHealthy()
        {
            // Should check critical configuration / dependencies (eg. database access) here
            return true;
        }

        private static string GetObfuscatedHostname()
        {
            return Environment.MachineName.Substring(Math.Max(0, Environment.MachineName.Length - 2));
        }
    }
}
