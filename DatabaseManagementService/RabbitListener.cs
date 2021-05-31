using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace DatabaseManagementService
{
    public class RabbitListener
    {
        ConnectionFactory Factory { get; set; }
        IConnection Connection { get; set; }
        IModel Channel { get; set; }
        DatabaseService _db;

        public void Register()
        {
            Channel.QueueDeclare(queue: "database_request_queue");

            Channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = Channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    Dictionary<string, object> json = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                    if ((string)json["Action"] == "Publish")
                    {
                        DatabaseResponse res = _db.AddToCollection(new AdditionRequest() { Collection = "articles", DataJson = json["Article"].ToString() }, null).Result;
                        response = $"[{res.MessageType}]: {res.Message}";
                    } else
                    {
                        response = "Supply \"Action\" argument";
                    }
                }
                catch (Exception)
                {
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    Channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                    Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            Channel.BasicConsume(queue: "database_request_queue", autoAck: false, consumer: consumer);
        }

        public void Deregister()
        {
            Connection.Close();
        }

        public RabbitListener(DatabaseService db)
        {
            _db = db;
            Factory = new ConnectionFactory() { HostName = "host.docker.internal", Port = 5672 };
            Factory.RequestedHeartbeat = TimeSpan.FromSeconds(60);
            Connection = Factory.CreateConnection();
            Channel = Connection.CreateModel();
        }
    }
}
