using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NewVoiceMedia.Claims;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test
{
    public static class ApiClient
    {
        private const string ServiceUrl = "https://dotnetgrpcservice.nvminternal.net:8443";
        private const string ServiceScopeName = "dotnetgrpcservice-api";
        private const string OidcIssuerCloud = "http://testoidc1:8080";

        public static HttpClient CreateUnauthenticatedHttpClient()
        {
            return CreateHttpClient(string.Empty);
        }

        public static async Task<HttpClient> CreateHttpClient(User user, string version = null)
        {
            return CreateHttpClient(await GetJwt(user), version);
        }

        public static HttpClient CreateHttpClient(string jwt, string version = null)
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
            if (!string.IsNullOrEmpty(version))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.ParseAdd($"application/vnd.newvoicemedia.v{version}+json");
            }
            client.BaseAddress = new Uri(ServiceUrl);
            return client;
        }

        private static async Task<string> GetJwt(User user = null)
        {
            if (user == null)
            {
                user = new User();
            }

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync($"{user.OidcIssuer}/token", user.TokenRequest);
                var json = await response.Content.ReadAsAsync<dynamic>();
                return json.access_token;
            }
        }

        public class User
        {
            public ulong AccountId;
            public ulong UserId;

            public Dictionary<string, string> Claims { get; set; }

            public IEnumerable<string> Scopes { get; set; } = new[] { ServiceScopeName };

            public object TokenRequest => new
            {
                client_id = "IntegrationTest",
                scope = string.Join(" ", Scopes),
                claims = Claims
            };

            public string OidcIssuer { get; set; } = OidcIssuerCloud;

            public User()
            {
                var random = new Random();
                AccountId = (ulong)random.Next();
                UserId = (ulong)random.Next();
                Claims = new Dictionary<string, string>
                {
                    {ClaimTypes.AccountId, AccountId.ToString()},
                    {ClaimTypes.UserId, UserId.ToString()}
                };
            }
        }
    }
}
