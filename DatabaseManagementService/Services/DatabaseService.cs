using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManagementService
{
    public class DatabaseService : DatabaseManagement.DatabaseManagementBase
    {
        private readonly ILogger<DatabaseService> _logger;
        private IConfiguration _config;
        private IMongoDatabase _db;
        public DatabaseService(ILogger<DatabaseService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
            IMongoClient client = new MongoClient(_config["ConnectionStrings:Mongo"]);
            _db = client.GetDatabase(_config["ConnectionStrings:Database"]);
            _logger.LogInformation("Database management service initialization... done");
        }

        public override Task<DatabaseResponse> AddToCollection(AdditionRequest request, ServerCallContext context)
        {
            var coll = _db.GetCollection<BsonDocument>(request.Collection);
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
            var coll = _db.GetCollection<BsonDocument>(request.Collection);
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
                result.ForEach(o => o["Id"] = o["_id"].AsObjectId.ToString());
                string res = result.ToJson(new MongoDB.Bson.IO.JsonWriterSettings() { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson });
                return Task.FromResult(new DatabaseResponse()
                {
                    Message = res,
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
            var coll = _db.GetCollection<BsonDocument>(request.Collection);
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
            var coll = _db.GetCollection<BsonDocument>(request.Collection);
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

        public override Task<UploadFileResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
        {
            var bucket = new GridFSBucket(_db);

            var id = bucket.UploadFromBytes(DateTime.Now.Ticks.ToString() + ".md", request.File.ToByteArray());

            return Task.FromResult(new UploadFileResponse()
            {
                Id = id.ToString()
            });
        }

        public override Task<DownloadFileResponse> DownloadFile(DownloadFileRequest request, ServerCallContext context)
        {
            var bucket = new GridFSBucket(_db);

            return Task.FromResult(new DownloadFileResponse()
            {
                File = ByteString.CopyFrom(bucket.DownloadAsBytes(ObjectId.Parse(request.Id))),
            });
        }

        //public override Task<FileInfoResponse> GetFileInfo(FileInfoRequest request, ServerCallContext context)
        //{
        //    var builder = new FilterDefinitionBuilder<GridFSFileInfo>();
        //    var bucket = new GridFSBucket(_db);
        //    string fileId;

        //    using (var cursor = bucket.Find(builder.Where(file => file.Metadata.GetValue("title") == request.Title)))
        //    {
        //        var fileInfo = cursor.ToList().FirstOrDefault();
        //        fileId = fileInfo != null ? fileInfo.Id.ToString() : string.Empty;
        //    }

        //    return Task.FromResult(new FileInfoResponse()
        //    {
        //        Id = fileId,
        //    });
        //}

        //public override Task<FilesInfosResponse> GetFilesInfos(None request, ServerCallContext context)
        //{
        //    var bucket = new GridFSBucket(_db);
        //    var builder = new FilterDefinitionBuilder<GridFSFileInfo>();
        //    FilesInfosResponse response = new FilesInfosResponse();

        //    using (var cursor = bucket.Find(builder.Empty))
        //    {
        //        response.Metadata.Add(cursor.ToList().Select(fl => fl.Metadata.ToJson()));
        //    }

        //    return Task.FromResult(response);
        //}

        public override Task<UploadImageResponse> UploadImage(UploadImageRequest request, ServerCallContext context)
        {
            var bucket = new GridFSBucket(_db, new GridFSBucketOptions() { BucketName = "images" });

            var id = bucket.UploadFromBytes(DateTime.Now.ToString(), request.File.ToByteArray());

            return Task.FromResult(new UploadImageResponse() { Id = id.ToString() });
        }

        public override Task<DownloadImageResponse> DownloadImage(DownloadImageRequest request, ServerCallContext context)
        {
            var bucket = new GridFSBucket(_db, new GridFSBucketOptions() { BucketName = "images" });

            var file = bucket.DownloadAsBytes(ObjectId.Parse(request.Id));

            return Task.FromResult(new DownloadImageResponse()
            {
                File = ByteString.CopyFrom(file)
            });
        }
    }
}
