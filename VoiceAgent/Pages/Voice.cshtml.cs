using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using static VoiceAgent.Pages.VoiceModel;
using System.Reflection;

namespace VoiceAgent.Pages
{
    public class VoiceModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public VoiceModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public List<Voice> Voices { get; set; }
        public string SelectedLanguage { get; set; }
        public string UserName { get; set; }
        public object PageData { get; set; }

        public class Voice
        {

            [JsonPropertyName("id")]
            public required string Id { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }
        }

        public async Task OnGetAsync()
        {
            var jsonData = TempData["JsonData"] as string;
            if (!string.IsNullOrEmpty(jsonData))
            {
                PageData = JsonSerializer.Deserialize<object>(jsonData);
            }

            Voices = await FetchVoicesFromApiAsync();
        }

        private async Task<List<Voice>> FetchVoicesFromApiAsync()
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7058/api/externalapi/getVoices");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Voice>>(jsonResponse);
            }
            else
            {
                return new List<Voice>();
            }
        }
    }
    public class CreateAgentRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("transcriber")]
        public Transcriber Transcriber { get; set; }

        [JsonPropertyName("model")]
        public Model model { get; set; }

        [JsonPropertyName("voice")]
        public Voices Voice { get; set; }
    }

    public class Voices
    {

        [JsonPropertyName("voiceId")]
        public required string Id { get; set; }

        [JsonPropertyName("provider")]
        public required string Provider { get; set; }
    }


    public class Model
    {
        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("model")]
        public string model { get; set; }

        [JsonPropertyName("messages")]
        public Messages[] Messages { get; set; }

        [JsonPropertyName("knowledgeBaseId")]
        public string? KnowledgeBaseId { get; set; }
    }

    public class Messages
    {

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class Transcriber
    {
        [JsonPropertyName("provider")]
        public string ProviderName { get; set; }

        [JsonPropertyName("language")]
        public string language { get; set; }
    }

    public class CreateAgentResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("orgId")]
        public string OrgId { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

    }


}
