// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace AuthService
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("api-gateway", "API Gateway")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "frontend",
                    ClientName = "WeltWerk front end application",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    AllowedScopes = { 
                        IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
                        "api-gateway" },
                    AllowedCorsOrigins = { "http://localhost:8080" },
                    RedirectUris = { "http://localhost:8080/#/callback" },
                    PostLogoutRedirectUris = { "http://localhost:8080/#/api-test" }
                }
            };
    }
}