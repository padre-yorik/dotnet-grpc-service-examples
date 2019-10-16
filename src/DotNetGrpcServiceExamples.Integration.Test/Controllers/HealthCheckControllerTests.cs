using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Controllers
{
    [Trait("Category", "Integration")]
    public class HealthCheckControllerTests
    {
        private const string OidcIssuerCloud = "http://testoidc1:8080";
        private const string OidcIssuerRegion = "http://testoidc2:8080";

        [Fact]
        public async Task Status_Ok()
        {
            using(var client = ApiClient.CreateUnauthenticatedHttpClient())
            {
                var response = await client.GetAsync("_status");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var jToken = JToken.Parse(content);
                jToken["serviceName"].ToString().Should().Be("dotnetgrpcservice");
                jToken["healthy"].ToString().Should().Be(true.ToString());
                jToken["hostname"].ToString().Length.Should().Be(2);
            }
        }

        [Theory]
        [InlineData(OidcIssuerCloud)]
        [InlineData(OidcIssuerRegion)]
        public async Task Info_Ok(string oidcIssuer)
        {
            var user = new ApiClient.User
            {
                OidcIssuer = oidcIssuer,
                Scopes = new [] { "dotnetgrpcservice-api", "internal" }
            };
            using(var client = await ApiClient.CreateHttpClient(user))
            {
                var response = await client.GetAsync("_info");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var jToken = JToken.Parse(content);
                jToken["version"].ToString().Should().NotBeNullOrEmpty();
                jToken["hostname"].ToString().Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task Info_MissingScope()
        {
            var user = new ApiClient.User { Scopes = new [] { "unknown-scope" } };

            using(var client = await ApiClient.CreateHttpClient(user))
            {
                var response = await client.GetAsync("_info");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task Info_MissingInternalScope()
        {
            var user = new ApiClient.User();

            using(var client = await ApiClient.CreateHttpClient(user))
            {
                var response = await client.GetAsync("_info");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Theory]
        [InlineData("AccountId")]
        [InlineData("UserId")]
        public async Task Info_MissingClaims(string claimToRemove)
        {
            var user = new ApiClient.User();
            user.Claims.Remove(claimToRemove);

            using(var client = await ApiClient.CreateHttpClient(user))
            {
                var response = await client.GetAsync("_info");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }
    }
}