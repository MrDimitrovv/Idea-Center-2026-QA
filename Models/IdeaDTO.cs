using System.Text.Json.Serialization;


namespace Exam_Prep.Models
{
    internal class IdeaDTO
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
