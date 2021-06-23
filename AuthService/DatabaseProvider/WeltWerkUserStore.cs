using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace AuthService.DatabaseProvider
{
    public class WeltWerkUserStore : IUserStore<WeltWerkUser>
    {
        const string USERCOLLECTION = "WeltWerkUsers";
        FilterDefinitionBuilder<WeltWerkUser> _builder;
        DatabaseManagementService.DatabaseManagement.DatabaseManagementClient _db;
        public WeltWerkUserStore(DatabaseManagementService.DatabaseManagement.DatabaseManagementClient db)
        {
            _db = db;
            _builder = new FilterDefinitionBuilder<WeltWerkUser>();
        }

        public async Task<IdentityResult> CreateAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            await _db.AddToCollectionAsync(new DatabaseManagementService.AdditionRequest()
            {
                Collection = USERCOLLECTION,
                DataJson = JsonConvert.SerializeObject(user)
            });

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            await _db.RemoveFromCollectionAsync(new DatabaseManagementService.CollectionRequest()
            {
                Collection = USERCOLLECTION,
                CriteriaJson = _builder.Where(u => u.Id == user.Id).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });

            return IdentityResult.Success;
        }

        public void Dispose()
        {
            
        }

        public async Task<WeltWerkUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var response = await _db.GetFromCollectionAsync(new DatabaseManagementService.CollectionRequest()
            {
                Collection = USERCOLLECTION,
                CriteriaJson = _builder.Where(u => u.Id == userId).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });

            return JsonConvert.DeserializeObject<WeltWerkUser>(response.Message);
        }

        public async Task<WeltWerkUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var response = await _db.GetFromCollectionAsync(new DatabaseManagementService.CollectionRequest()
            {
                Collection = USERCOLLECTION,
                CriteriaJson = _builder.Where(u => u.NormalizedUserName == normalizedUserName).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });
            return JsonConvert.DeserializeObject<WeltWerkUser>(response.Message);
        }

        public Task<string> GetNormalizedUserNameAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<string> GetUserIdAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            var response = await _db.GetFromCollectionAsync(new DatabaseManagementService.CollectionRequest()
            {
                Collection = USERCOLLECTION,
                CriteriaJson = _builder.Where(u => u.UserName == user.UserName).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });
            return JsonConvert.DeserializeObject<WeltWerkUser>(response.Message).Id;
        }

        public async Task<string> GetUserNameAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            var response = await _db.GetFromCollectionAsync(new DatabaseManagementService.CollectionRequest()
            {
                Collection = USERCOLLECTION,
                CriteriaJson = _builder.Where(u => u.Id == user.Id).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });
            return JsonConvert.DeserializeObject<WeltWerkUser>(response.Message).UserName;
        }

        public Task SetNormalizedUserNameAsync(WeltWerkUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(WeltWerkUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(WeltWerkUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
