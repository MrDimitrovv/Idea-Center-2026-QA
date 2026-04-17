
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text.Json.Serialization;


namespace Exam_Prep.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("id")]

        public string? Id { get; set; }

    }
}
