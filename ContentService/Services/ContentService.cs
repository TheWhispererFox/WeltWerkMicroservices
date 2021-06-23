using ContentService.Models;
using DatabaseManagementService;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentService.Services
{
    public class ContentService : Content.ContentBase
    {
        ILogger<ContentService> _logger;
        IConfiguration _config;
        DatabaseManagement.DatabaseManagementClient _db;

        public ContentService(ILogger<ContentService> logger, IConfiguration config, DatabaseManagement.DatabaseManagementClient db)
        {
            _logger = logger;
            _config = config;
            _db = db;
        }

        public override Task<ContentResponse> PublishArticle(Article request, ServerCallContext context)
        {
            var response = _db.AddToCollection(new AdditionRequest() { Collection = "WeltWerkArticles", DataJson = JsonConvert.SerializeObject(request) });

            return Task.FromResult(new ContentResponse()
            {
                Message = $"[{response.MessageType}] Uploaded article id: {response.Message}"
            });
        }

        public override Task<Article> GetArticle(ArticleRequest request, ServerCallContext context)
        {
            DatabaseResponse response;
            var builder = new FilterDefinitionBuilder<Article>();
            response = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkArticles",
                CriteriaJson = builder.Where(a => a.Id == request.Id).Render(BsonSerializer.SerializerRegistry.GetSerializer<Article>(), BsonSerializer.SerializerRegistry).ToJson()
            });

            return Task.FromResult(JsonConvert.DeserializeObject<Article>(response.Message));
        }

        public override Task<ArticlesResponse> GetArticles(None request, ServerCallContext context)
        {
            ArticlesResponse articles = new ArticlesResponse();
            var response = _db.GetFromCollection(new CollectionRequest() { Collection = "WeltWerkArticles", CriteriaJson = "{}" });

            List<Article> metadatas = JsonConvert.DeserializeObject<List<Article>>(response.Message);

            foreach (var metadata in metadatas)
            {
                articles.Articles.Add(metadata);
            }

            return Task.FromResult(articles);
        }

        public override Task<ContentResponse> DeleteArticle(ArticleRequest request, ServerCallContext context)
        {
            DatabaseResponse response;
            var builder = new FilterDefinitionBuilder<Article>();
            response = _db.RemoveFromCollection(new CollectionRequest() { Collection = "WeltWerkArticles", CriteriaJson = builder.Where(a => a.Id == request.Id).Render(BsonSerializer.SerializerRegistry.GetSerializer<Article>(), BsonSerializer.SerializerRegistry).ToJson() });
            return Task.FromResult(new ContentResponse()
            {
                Message = $"[{response.MessageType}] {response.Message}"
            });
        }

        public override Task<Categories> GetCategories(None request, ServerCallContext context)
        {
            var response = _db.GetFromCollection(new CollectionRequest()
            {
                Collection = "WeltWerkArticles",
                CriteriaJson = "{}"
            });

            Categories categories = new Categories();

            List<MarkdownDocumentMetadata> metadatas = JsonConvert.DeserializeObject<List<MarkdownDocumentMetadata>>(response.Message);

            foreach (var metadata in metadatas)
            {
                categories.Category.Add(metadata.Category);
            }

            return Task.FromResult(categories);
        }

        public override Task<DownloadDocumentResponse> DownloadDocument(DownloadDocumentRequest request, ServerCallContext context)
        {
            var response = _db.DownloadFile(new DownloadFileRequest()
            {
                Id = request.Id
            });

            return Task.FromResult(new DownloadDocumentResponse()
            {
                File = response.File
            });
        }

        public override Task<UploadDocumentResponse> UploadDocument(UploadDocumentRequest request, ServerCallContext context)
        {
            var response = _db.UploadFile(new UploadFileRequest()
            {
                File = request.File
            });

            return Task.FromResult(new UploadDocumentResponse()
            {
                Id = response.Id
            });
        }

        public override Task<DownloadImageResponse> DownloadImage(DownloadImageRequest request, ServerCallContext context)
        {
            var data = _db.DownloadImage(new DatabaseManagementService.DownloadImageRequest()
            {
                Id = request.Id
            });

            return Task.FromResult(new DownloadImageResponse()
            {
                Image = data.File
            });
        }

        public override Task<UploadImageResponse> UploadImage(UploadImageRequest request, ServerCallContext context)
        {
            var response = _db.UploadImage(new DatabaseManagementService.UploadImageRequest()
            {
                File = request.Image
            });

            return Task.FromResult(new UploadImageResponse()
            {
                Id = response.Id
            });
        }
    }
}
