using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;

namespace AnnouncementService
{
    public class AnnouncementService : Announcer.AnnouncerBase
    {
        private readonly ILogger<AnnouncementService> _logger;
        private IConfiguration _config;
        //private ConnectionFactory _factory;
        private readonly HttpClient _http;
        private RabbitListener _rabbit;
        public AnnouncementService(ILogger<AnnouncementService> logger, IConfiguration config, HttpClient httpClient /*RabbitListener rabbit*/)
        {
            _logger = logger;
            _config = config;
            //_rabbit = rabbit;
            //_factory = new ConnectionFactory() { HostName = _config["Services:EventService:HostName"], Port = int.Parse(_config["Services:EventService:Port"]) };
            _http = httpClient;
        }

        public override Task<AnnounceReply> Announce(AnnounceArgs request, ServerCallContext context)
        {
            StringContent jsonContent = new(JsonConvert.SerializeObject(new Dictionary<string, string>() { { "content", $"{request.Content}" } }));
            jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            _http.PostAsync($"https://discord.com/api/webhooks/{_config["Announcements:Discord:WebhookId"]}/{_config["Announcements:Discord:WebhookToken"]}", jsonContent);
            return Task.FromResult(new AnnounceReply
            {
                Response = "200 OK"
            });
        }

        //public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        //{
        //    return Task.FromResult(new HelloReply
        //    {
        //        Message = "Hello " + request.Name
        //    });
        //}
    }
}
