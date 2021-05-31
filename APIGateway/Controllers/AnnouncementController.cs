using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace APIGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly ConnectionFactory _factory;
        private readonly IConfiguration _config;
        public AnnouncementController(IConfiguration config)
        {
            _config = config;
            _factory = new ConnectionFactory() { HostName = _config["Services:EventService:HostName"], Port = int.Parse(_config["Services:EventService:Port"]) };
        }
        [HttpPost]
        public void Post([FromBody] object message)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "general_topics", type: ExchangeType.Topic);

                var body = Encoding.UTF8.GetBytes(message.ToString());

                channel.BasicPublish(exchange: "general_topics", routingKey: "announce.api", basicProperties: null, body: body);
            }
        }
    }
}
