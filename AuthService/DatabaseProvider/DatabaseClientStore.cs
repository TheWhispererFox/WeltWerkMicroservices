using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseManagementService;
using Newtonsoft.Json;

namespace AuthService.DatabaseProvider
{
    public class DatabaseClientStore : IClientStore
    {
        private DatabaseManagement.DatabaseManagementClient _db;

        public DatabaseClientStore(DatabaseManagement.DatabaseManagementClient db)
        {
            _db = db;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var clientJson = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkClients",
                CriteriaJson = ""
            });

            var client = JsonConvert.DeserializeObject<Client>(clientJson.Message);

            if (client == null)
            {
                return Task.FromResult<Client>(null);
            }

            return Task.FromResult(new Client()
            {
                ClientId = client.ClientId,
                AllowedScopes = client.AllowedScopes,
                RedirectUris = client.RedirectUris,
                ClientSecrets = client.ClientSecrets,
            });
        }
    }
}
