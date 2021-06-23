using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;

namespace ContentService.Models
{
    public class MarkdownDocumentMetadata
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public Timestamp CreatedAt { get; set; }
        public Timestamp EditedAt { get; set; }
        public string LastEditAuthor { get; set; }
        public Category Category { get; set; }
        public List<Tag> Tags { get; set; }
        public string Description { get; set; }
        public string FileId { get; set; }
        public string CustomElements { get; set; }
    }
}
