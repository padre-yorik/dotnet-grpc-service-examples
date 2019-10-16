using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NewVoiceMedia.AspNetCore.Mvc.Json;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Models
{
    public class ToDoItem
    {
        [JsonIgnore]
        public ulong Id { get; set; }

        public string ToDoId { get; set; }

        [JsonIgnore]
        public ulong AccountId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [TrimWhitespace]
        public string Name { get; set; }

        [Required]
        public bool IsComplete { get; set; }
    }
}