using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace InnoClinic.Identity.Configurations
{
    public sealed class Configuration
    {
        public static List<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ("InnoClinicWebApi", "Web Api")
            };

        public static List<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static List<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("InnoClinicWebApi", "Web Api", new[] { JwtClaimTypes.Name })
                {
                    Scopes = { "InnoClinicWebApi" },
                    UserClaims = { JwtClaimTypes.Name, JwtClaimTypes.Role }
                }
            };

        public static List<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "innoclinic-web-api",
                    ClientSecrets = new [] { new Secret("inno-clinic-secret".Sha512()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "InnoClinicWebApi" },
                    RedirectUris = { "http://localhost:4200/signin-oidc" },
                    AllowAccessTokensViaBrowser = true,
                }
            };
    }
}
