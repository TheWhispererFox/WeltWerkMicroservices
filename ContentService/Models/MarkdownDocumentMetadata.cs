using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace ContentService.Models
{
    public class MarkdownDocumentMetadata
    {
        [BsonId]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
        public string LastEditAuthor { get; set; }
        public Category Category { get; set; }
        public List<Tag> Tags { get; set; }
        public string Description { get; set; }
        public string FileId { get; set; }
        [BsonExtraElements]
        public BsonDocument CustomElements { get; set; }
    }
}
