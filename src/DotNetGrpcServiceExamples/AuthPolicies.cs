using Microsoft.AspNetCore.Authorization;
using NewVoiceMedia.AspNetCore.Authentication;
using NewVoiceMedia.Claims;

namespace NewVoiceMedia.DotNetGrpcServiceExamples
{
    public static class AuthPolicies
    {
        public const string Global = "GlobalPolicy";
        public const string Internal = "Internal";

        public static void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(Global, policy =>
            {
                policy.RequireScope(Program.ScopeName);
                policy.RequireClaim(ClaimTypes.UserId);
                policy.RequireClaim(ClaimTypes.AccountId);
                policy.AddAuthenticationSchemes(NvmAuthenticationDefaults.OidcAuthenticationScheme, NvmAuthenticationDefaults.HttpHeaderInternalAuthenticationScheme);
            });

            options.AddPolicy(Internal, policy =>
            {
                policy.RequireScope(Program.InternalScopeName);
                policy.AddAuthenticationSchemes(NvmAuthenticationDefaults.OidcAuthenticationScheme);
            });
        }
    }
}