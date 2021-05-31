using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using System.Text;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using DatabaseManagementService;
using Newtonsoft.Json.Linq;

namespace ContentService.Services
{
    public class ContentService : Content.ContentBase
    {
        ILogger<ContentService> _logger;
        IConfiguration _config;
        DatabaseManagement.DatabaseManagementClient _db;
        //ConnectionFactory _factory;
        //BlockingCollection<string> respQueue = new BlockingCollection<string>();
        //IConnection connection;
        //IBasicProperties props;
        //EventingBasicConsumer consumer;
        //string replyQueueName;
        //IModel channel;

        public ContentService(ILogger<ContentService> logger, IConfiguration config, DatabaseManagement.DatabaseManagementClient db)
        {
            _logger = logger;
            _config = config;
            _db = db;
            //_factory = new ConnectionFactory() { HostName = _config["Services:EventService:HostName"], Port = int.Parse(_config["Services:EventService:Port"]) };
            //replyQueueName = channel.QueueDeclare().QueueName;
            //consumer = new EventingBasicConsumer(channel);

            //props = channel.CreateBasicProperties();
            //var correlationId = Guid.NewGuid().ToString();
            //props.ReplyTo = replyQueueName;

            //consumer.Received += (model, ea) =>
            //{
            //    byte[] body = ea.Body.ToArray();
            //    string response = Encoding.UTF8.GetString(body);
            //    if (ea.BasicProperties.CorrelationId == correlationId)
            //    {
            //        respQueue.Add(response);
            //    }
            //};
        }

        public override Task<ContentResponse> PublishArticle(Article request, ServerCallContext context)
        {
            #region big comment
            //Dictionary<string, object> dataDictionary = new Dictionary<string, object>()
            //{
            //    {
            //        "Action",
            //        "Publish"
            //    },
            //    {
            //        "Article",
            //        request
            //    }
            //};
            //var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataDictionary));
            //channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: props, body: messageBytes);

            //channel.BasicConsume(
            //    consumer: consumer,
            //    queue: replyQueueName,
            //    autoAck: true);

            //return Task.FromResult(new ContentResponse()
            //{
            //    Message = "200"
            //});
            #endregion

            var response = _db.AddToCollection(new AdditionRequest() { Collection = "Articles", DataJson = JsonConvert.SerializeObject(request) });
            return Task.FromResult(new ContentResponse()
            {
                Message = $"[{response.MessageType}] {response.Message}"
            });
        }

        public override Task<ArticleResponse> GetArticle(ArticleRequest request, ServerCallContext context)
        {
            DatabaseResponse response;
            switch (request.ArticleCriteriaCase)
            {
                case ArticleRequest.ArticleCriteriaOneofCase.None:
                    response = _db.GetFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = "{}" });
                    break;
                case ArticleRequest.ArticleCriteriaOneofCase.Id:
                    response = _db.GetFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = $"{{ \"id\": \"{request.Id}\" }}" });
                    break;
                case ArticleRequest.ArticleCriteriaOneofCase.Title:
                    response = _db.GetFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = $"{{ \"id\": \"{request.Title}\" }}" });
                    break;
                default:
                    response = new DatabaseResponse() { Message = "None", MessageType = MessageType.Error };
                    break;
            }
            ArticleResponse article = (JsonConvert.DeserializeObject(response.Message) as List<ArticleResponse>).FirstOrDefault();
            return Task.FromResult(article);
        }

        public override Task<ArticlesResponse> GetArticles(None request, ServerCallContext context)
        {
            DatabaseResponse response;
            response = _db.GetFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = "{}" });
            ArticlesResponse article = new ArticlesResponse();
            List<Dictionary<string, object>> listOfDicts = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response.Message);
            foreach (Dictionary<string, object> dict in listOfDicts)
            {
                var dicto = dict["PublishDate"].ToString();
                article.Articles.Add(new ArticleResponse()
                {
                    Id = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict["_id"].ToString())["$oid"].ToString(),
                    Title = dict["Title"].ToString(),
                    PublishDate = JsonConvert.DeserializeObject<Google.Protobuf.WellKnownTypes.Timestamp>(dict["PublishDate"].ToString()),
                });
            }

            return Task.FromResult(article);
        }

        public override Task<ContentResponse> DeleteArticle(ArticleRequest request, ServerCallContext context)
        {
            DatabaseResponse response;
            switch (request.ArticleCriteriaCase)
            {
                case ArticleRequest.ArticleCriteriaOneofCase.None:
                    response = _db.RemoveFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = "{}" });
                    break;
                case ArticleRequest.ArticleCriteriaOneofCase.Id:
                    response = _db.RemoveFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = $"{{ \"id\": \"{request.Id}\" }}" });
                    break;
                case ArticleRequest.ArticleCriteriaOneofCase.Title:
                    response = _db.RemoveFromCollection(new CollectionRequest() { Collection = "Articles", CriteriaJson = $"{{ \"id\": \"{request.Title}\" }}" });
                    break;
                default:
                    response = new DatabaseResponse() { Message = "None", MessageType = MessageType.Error };
                    break;
            }
            return Task.FromResult(new ContentResponse()
            {
                Message = $"[{response.MessageType}] {response.Message}"
            });
        }

        public override Task<ContentResponse> CreateCategory(Category request, ServerCallContext context)
        {
            return Task.FromResult(new ContentResponse()
            {
                Message = "[Error] Not implemented yet"
            });
        }

        public override Task<ContentResponse> DeleteCategory(Category request, ServerCallContext context)
        {
            return Task.FromResult(new ContentResponse()
            {
                Message = "[Error] Not implemented yet"
            });
        }
    }
}
