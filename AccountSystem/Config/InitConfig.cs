using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountSystem.Config
{
    public class InitConfig
    {
        //暂时不用oidc
        // scopes define the resources in your system
        //public static IEnumerable<IdentityResource> GetIdentityResources()
        //{
        //    return new List<IdentityResource>
        //    {
        //        new IdentityResources.OpenId(),
        //        new IdentityResources.Profile(),
        //    };
        //}

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("Web.Portal", "Portal"),
                new ApiResource("AccountSystem", "Account", new[]{ JwtClaimTypes.Name, JwtClaimTypes.Role }),
                new ApiResource("Web.Manager", "Manager")
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = "account.ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("account.secret".Sha256())
                    },
                    AllowedScopes = { "AccountSystem" }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "portal.ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("portal.secret".Sha256())
                    },
                    AllowedScopes = { "Web.Portal" }
                },

                new Client
                {
                    ClientId = "manager.ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("manager.secret".Sha256())
                    },
                    AllowedScopes = { "Web.Manager" }
                },
                //暂时不用oidc
                // OpenID Connect hybrid flow and client credentials client (MVC)
                //new Client
                //{
                //    ClientId = "mvc",
                //    ClientName = "MVC Client",
                //    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                //    RequireConsent = true,

                //    ClientSecrets =
                //    {
                //        new Secret("secret".Sha256())
                //    },

                //    RedirectUris = { "http://localhost:5002/signin-oidc" },
                //    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                //    AllowedScopes =
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        "api1"
                //    },
                //    AllowOfflineAccess = true
                //}
            };
        }
    }
}
