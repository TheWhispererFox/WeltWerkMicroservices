using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManagementService
{
    public class DatabaseService : DatabaseManagement.DatabaseManagementBase
    {
        private readonly ILogger<DatabaseService> _logger;
        private MongoClient _client;
        private IConfiguration _config;
        public DatabaseService(ILogger<DatabaseService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
            _client = new MongoClient(_config["ConnectionStrings:Mongo"]);
            _logger.LogInformation("Database management service initialization...done");
        }

        public override Task<DatabaseResponse> AddToCollection(AdditionRequest request, ServerCallContext context)
        {
            var db = _client.GetDatabase(_config["ConnectionStrings:Database"]);
            var coll = db.GetCollection<BsonDocument>(request.Collection);
            if (BsonDocument.TryParse(request.DataJson, out BsonDocument bson))
            {
                try
                {
                    coll.InsertOne(bson);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                    return Task.FromResult(new DatabaseResponse()
                    {
                        Message = e.Message,
                        MessageType = MessageType.Error
                    });
                }
                _logger.LogInformation($"Successfully inserted bson document to collection {request.Collection}");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = $"Successfully inserted bson document to {request.Collection}",
                    MessageType = MessageType.Info
                });
            }
            else
            {
                _logger.LogError("Could not convert JSON to BSON, please check what provided JSON is valid");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = "Could not convert JSON to BSON, please check what provided JSON is valid",
                    MessageType = MessageType.Error
                });
            }
        }

        public override Task<DatabaseResponse> GetFromCollection(CollectionRequest request, ServerCallContext context)
        {
            var db = _client.GetDatabase(_config["ConnectionStrings:Database"]);
            var coll = db.GetCollection<BsonDocument>(request.Collection);
            List<BsonDocument> result;
            if (BsonDocument.TryParse(request.CriteriaJson, out BsonDocument bson))
            {
                try
                {
                    result = coll.Find(bson).ToList();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                    return Task.FromResult(new DatabaseResponse()
                    {
                        Message = e.Message,
                        MessageType = MessageType.Error
                    });
                }
                _logger.LogInformation($"Successfully retrieved bson documents from collection {request.Collection}");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = result.ToJson(new MongoDB.Bson.IO.JsonWriterSettings() { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson }),
                    MessageType = MessageType.Info
                });
            }
            else
            {
                _logger.LogError("Could not convert JSON to BSON, please check what provided JSON is valid");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = "Could not convert JSON to BSON, please check what provided JSON is valid",
                    MessageType = MessageType.Error
                });
            }
        }

        public override Task<DatabaseResponse> RemoveFromCollection(CollectionRequest request, ServerCallContext context)
        {
            var db = _client.GetDatabase(_config["ConnectionStrings:Database"]);
            var coll = db.GetCollection<BsonDocument>(request.Collection);
            if (BsonDocument.TryParse(request.CriteriaJson, out BsonDocument bson))
            {
                try
                {
                    coll.DeleteOne(bson);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                    return Task.FromResult(new DatabaseResponse()
                    {
                        Message = e.Message,
                        MessageType = MessageType.Error
                    });
                }
                _logger.LogInformation($"Successfully removed bson document from collection {request.Collection}");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = $"Successfully removed bson document from {request.Collection}",
                    MessageType = MessageType.Info
                });
            }
            else
            {
                _logger.LogError("Could not convert JSON to BSON, please check what provided JSON is valid");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = "Could not convert JSON to BSON, please check what provided JSON is valid",
                    MessageType = MessageType.Error
                });
            }
        }

        public override Task<DatabaseResponse> UpdateCollection(AdditionRequest request, ServerCallContext context)
        {
            var db = _client.GetDatabase(_config["ConnectionStrings:Database"]);
            var coll = db.GetCollection<BsonDocument>(request.Collection);
            var filterBuilder = new FilterDefinitionBuilder<BsonDocument>();
            if (BsonDocument.TryParse(request.DataJson, out BsonDocument bson))
            {
                try
                {
                    coll.ReplaceOne(filterBuilder.Where(d => d.GetValue("_id") == bson.GetValue("_id")), bson);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                    return Task.FromResult(new DatabaseResponse()
                    {
                        Message = e.Message,
                        MessageType = MessageType.Error
                    });
                }
                _logger.LogInformation($"Successfully updated bson document from collection {request.Collection}");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = $"Successfully updated bson document from {request.Collection}",
                    MessageType = MessageType.Info
                });
            }
            else
            {
                _logger.LogError("Could not convert JSON to BSON, please check what provided JSON is valid");
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = "Could not convert JSON to BSON, please check what provided JSON is valid",
                    MessageType = MessageType.Error
                });
            }
        }
    }
}
