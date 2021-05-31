using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AnnouncementService
{
    public class RabbitListener
    {
        ConnectionFactory Factory { get; set; }
        IConnection Connection { get; set; }
        IModel Channel { get; set; }
        AnnouncementService _announcer;

        public void Register()
        {
            Channel.ExchangeDeclare("general_topics", ExchangeType.Topic);

            var queueName = Channel.QueueDeclare().QueueName;

            Channel.QueueBind(queue: queueName, exchange: "general_topics", routingKey: "announce.#");

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                //call the announce method
                _announcer.Announce(new AnnounceArgs() { Content = message }, null);
            };
            Channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public void Deregister()
        {
            Connection.Close();
        }

        public RabbitListener(AnnouncementService announcer, IConfiguration config)
        {
            _announcer = announcer;
            Factory = new ConnectionFactory() { HostName = config["Services:EventService:HostName"], Port = int.Parse(config["Services:EventService:Port"]) };
            Factory.RequestedHeartbeat = TimeSpan.FromSeconds(60);
            Connection = Factory.CreateConnection();
            Channel = Connection.CreateModel();
        }
    }
}
