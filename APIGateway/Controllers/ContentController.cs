using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;
using ContentService;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        //private readonly ConnectionFactory _factory;
        private readonly IConfiguration _config;
        private Content.ContentClient _content;

        public ContentController(IConfiguration config, Content.ContentClient content)
        {
            _config = config;
            _content = content;
            //_factory = new ConnectionFactory() { HostName = _config["Services:EventService:HostName"], Port = int.Parse(_config["Services:EventService:Port"]) };
        }
        [HttpGet]
        public IEnumerable<ArticleResponse> GetArticles()
        {
            return _content.GetArticles(new None()).Articles.ToList();
        }
        [HttpPut]
        public void PutArticle([FromBody] object json)
        {
            //using (var connection = _factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    channel.ExchangeDeclare(exchange: "general_topics", type: ExchangeType.Topic);

            //    var body = Encoding.UTF8.GetBytes(json.ToString());
            //}
            Dictionary<string, object> articleJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

            Article article = new()
            {
                Title = articleJson["title"].ToString(),
                PublishDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            _content.PublishArticle(article);
        }


        //// GET: api/<ContentController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<ContentController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<ContentController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ContentController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ContentController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
