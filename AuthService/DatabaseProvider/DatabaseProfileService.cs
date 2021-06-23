using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using DatabaseManagementService;
using System.Security.Claims;
using IdentityModel;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthService.DatabaseProvider
{
    public class DatabaseProfileService : IProfileService
    {
        DatabaseManagement.DatabaseManagementClient _db;

        public DatabaseProfileService(DatabaseManagement.DatabaseManagementClient db)
        {
            _db = db;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var bsonBuilder = new FilterDefinitionBuilder<WeltWerkUser>();


            var userJson = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkUsers",
                CriteriaJson = bsonBuilder.Where(u => u.Id == context.Subject.GetSubjectId()).ToBson().ToJson()
            });

            var user = JsonConvert.DeserializeObject<WeltWerkUser>(userJson.Message);

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id),
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.Email, user.Email),
            };

            context.IssuedClaims = claims;

            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var bsonBuilder = new FilterDefinitionBuilder<WeltWerkUser>();

            var userJson = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkUsers",
                CriteriaJson = bsonBuilder.Where(u => u.Id == context.Subject.GetSubjectId()).ToBson().ToJson()
            });

            var user = JsonConvert.DeserializeObject<WeltWerkUser>(userJson.Message);

            context.IsActive = user != null;

            return Task.FromResult(0);
        }
    }
}
