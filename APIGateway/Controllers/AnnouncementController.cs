using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace APIGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        public void Post([FromBody] Announcement message)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "general_topics", type: ExchangeType.Topic);

                var body = Encoding.UTF8.GetBytes(message.Content);

                channel.BasicPublish(exchange: "general_topics", routingKey: "announce.api", basicProperties: null, body: body);
            }
        }
    }
}
