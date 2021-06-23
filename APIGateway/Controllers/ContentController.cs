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
using ContentService.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Google.Protobuf;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APIGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IConfiguration _config;
        private Content.ContentClient _content;

        public ContentController(IConfiguration config, Content.ContentClient content)
        {
            _config = config;
            _content = content;
        }
        [HttpGet]
        public IEnumerable<Article> GetArticles()
        {
            return _content.GetArticles(new None()).Articles.ToList();
        }
        [HttpPut]
        public void PutArticle([FromBody] MarkdownDocumentMetadata json)
        {
            //Dictionary<string, object> articleJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());

            //Article article = new()
            //{
            //    Title = articleJson["title"].ToString(),
            //    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            //    EditedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            //    Author = articleJson["author"].ToString(),
            //    Category = JsonConvert.DeserializeObject<Category>(articleJson["category"].ToString()),
            //    Description = articleJson["description"].ToString(),
            //    LastEditor = articleJson["last_editor"].ToString()
            //};
            Article article = new()
            {
                Title = json.Title,
                Author = json.Author,
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Category = json.Category,
                Description = json.Description,
                EditedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                LastEditor = json.LastEditAuthor,
                CustomElements = json.CustomElements,
                FileId = json.FileId
            };
            article.Tags.Add(json.Tags);
            _content.PublishArticle(article);
        }

        [HttpPut("doc")]
        public string UploadDocument(IFormFile doc)
        {
            if (doc != null)
            {
                byte[] docBin;
                using (MemoryStream str = new MemoryStream())
                {
                    doc.OpenReadStream().CopyTo(str);
                    docBin = str.ToArray();
                }
                var response = _content.UploadDocument(new UploadDocumentRequest() { File = Google.Protobuf.ByteString.CopyFrom(docBin) });
                return response.Id;
            }
            return "Document should be uploaded";
        }

        [HttpGet("doc/{id}")]
        public IActionResult DownloadDocument(string id)
        {
            var data = _content.DownloadDocument(new DownloadDocumentRequest() { Id = id });
            return File(data.File.ToByteArray(), "text/markdown");
        }

        [HttpPut("image")]
        public string UploadImage(IFormFile image)
        {
            if (image != null)
            {
                byte[] imageBin;
                using (MemoryStream str = new MemoryStream())
                {
                    image.OpenReadStream().CopyTo(str);
                    imageBin = str.ToArray();
                }
                var response = _content.UploadImage(new UploadImageRequest()
                {
                    Image = ByteString.CopyFrom(imageBin)
                });
                return response.Id;
            }
            return "Image should be uploaded";
        }

        [HttpGet("image/{id}")]
        public IActionResult DownloadImage(string id)
        {
            var data = _content.DownloadImage(new DownloadImageRequest()
            {
                Id = id
            });
            return File(data.Image.ToByteArray(), "image/png");
        }

        [HttpDelete("doc/{id}")]
        public void DeleteArticle(string id)
        {
            _content.DeleteArticle(new ArticleRequest() { Id = id });
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
