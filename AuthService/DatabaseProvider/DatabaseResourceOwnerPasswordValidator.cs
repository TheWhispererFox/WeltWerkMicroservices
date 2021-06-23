using IdentityServer4.Validation;
using System.Threading.Tasks;
using DatabaseManagementService;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace AuthService.DatabaseProvider
{
    public class DatabaseResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private DatabaseManagement.DatabaseManagementClient _db;

        public DatabaseResourceOwnerPasswordValidator(DatabaseManagement.DatabaseManagementClient db)
        {
            _db = db;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var builder = new FilterDefinitionBuilder<WeltWerkUser>();
            var userJson = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkUsers",
                CriteriaJson = builder.Where(u => u.UserName == context.UserName).Render(BsonSerializer.SerializerRegistry.GetSerializer<WeltWerkUser>(), BsonSerializer.SerializerRegistry).ToJson()
            });

            var user = JsonConvert.DeserializeObject<WeltWerkUser>(userJson.Message);

            if (context.UserName == user.UserName && context.Password == user.PasswordHash)
            {
                return Task.FromResult(new GrantValidationResult(context.UserName, "password"));
            }

            return Task.FromResult(new GrantValidationResult(IdentityServer4.Models.TokenRequestErrors.InvalidGrant, "Wrong username or password"));
        }
    }
}
