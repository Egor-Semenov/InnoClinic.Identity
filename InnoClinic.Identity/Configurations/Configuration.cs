using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace InnoClinic.Identity.Configurations
{
    public sealed class Configuration
    {
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ("InnoClinicWebApi", "Web Api")
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("InnoClinicWebApi", "Web Api", new[] { JwtClaimTypes.Name })
                {
                    Scopes = { "InnoClinicWebApi" },
                    UserClaims = { JwtClaimTypes.Name, JwtClaimTypes.Role }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "innoclinic-web-api",
                    ClientSecrets = new [] { new Secret("inno-clinic-secret".Sha512()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "InnoClinicWebApi" },
                    AllowAccessTokensViaBrowser = true
                }
            };
    }
}
