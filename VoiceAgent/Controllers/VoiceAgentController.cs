using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VoiceAgent.Pages;
using static VoiceAgent.Pages.VoiceModel;

[Route("api/[controller]")]
[ApiController]
public class ExternalApiController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExternalApiController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpGet("getKnowledgeBase")]
    public async Task<IActionResult> GetKnowledgeBase()
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.vapi.ai/file");
            requestMessage.Headers.Add("Authorization", "Bearer b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            return Ok(jsonDoc.RootElement);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error fetching knowledge base: {ex.Message}" });
        }
    }

    [HttpGet("getFiles")]
    public async Task<IActionResult> GetFiles()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.vapi.ai/file");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            return Ok(jsonDoc.RootElement);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error fetching files: {ex.Message}" });
        }
    }

    [HttpPost("uploadFile")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var fileStream = file.OpenReadStream();
            content.Add(new StreamContent(fileStream), "file", file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.vapi.ai/file")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", "b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217") },
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            return Ok(JsonDocument.Parse(jsonString).RootElement);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error uploading file: {ex.Message}" });
        }
    }

    [HttpDelete("deleteFile/{fileId}")]
    public async Task<IActionResult> DeleteFile(string fileId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.vapi.ai/file/{fileId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return Ok(new { message = "File deleted successfully" });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error deleting file: {ex.Message}" });
        }
    }

    [HttpGet("getVoices")]
    public async Task<IActionResult> GetVoices()
    {
        try
        {
            var voicesUrl = "https://api.cartesia.ai/voices/";
            var voices = new List<Voice>();

            using var client = new HttpClient();
            var apiKey = _configuration["APIKeys:CartesiaApiKey"];
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            client.DefaultRequestHeaders.Add("Cartesia-Version", "2024-06-10");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync(voicesUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                voices = JsonSerializer.Deserialize<List<Voice>>(jsonResponse);

                voices.ForEach(voice => voice.Id = voice.Id.ToString());
            }

            return Ok(voices);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error fetching voices: {ex.Message}" });
        }
    }

    [HttpGet("getVoice/{voiceId}")]
    public async Task<IActionResult> GetVoice(string voiceId)
    {
        try
        {
            var voiceUrl = $"https://api.cartesia.ai/voices/{voiceId}";
            using var client = new HttpClient();
            var apiKey = _configuration["APIKeys:CartesiaApiKey"];
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            client.DefaultRequestHeaders.Add("Cartesia-Version", "2024-06-10");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var voiceResponse = await client.GetAsync(voiceUrl);
            if (!voiceResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, new { error = "Error fetching voice data." });
            }

            var voiceJson = await voiceResponse.Content.ReadAsStringAsync();
            var voiceData = JsonSerializer.Deserialize<Voice>(voiceJson);

            if (voiceData == null)
            {
                return StatusCode(500, new { error = "Voice data not found." });
            }

            var transcript = "Hello! This is a test voice";
            var payload = new
            {
                transcript = transcript,
                model_id = "sonic",
                voice = new
                {
                    mode = "id",
                    id = voiceData.Id
                },
                output_format = new
                {
                    container = "wav",
                    encoding = "pcm_f32le",
                    sample_rate = 44100
                }
            };

            var audioUrlResponse = await client.PostAsJsonAsync("https://api.cartesia.ai/tts/bytes", payload);
            if (!audioUrlResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, new { error = "Error generating audio." });
            }

            var audioBytes = await audioUrlResponse.Content.ReadAsByteArrayAsync();

            return File(audioBytes, "audio/wav");
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error processing voice request: {ex.Message}" });
        }
    }

    [HttpPost("createAgent")]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request)
    {
        try
        {
            

            var jsonContent = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var externalApiUrl = "https://api.vapi.ai/assistant";
            var apiKey = "b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var response = await _httpClient.PostAsync(externalApiUrl, httpContent);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var agentResponse = JsonSerializer.Deserialize<CreateAgentResponse>(responseBody);

            return Ok(agentResponse);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error creating agent: {ex.Message}" });
        }
    }

    [HttpGet("getAgent")]
    public async Task<IActionResult> GetAgent()
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.vapi.ai/assistant");
            requestMessage.Headers.Add("Authorization", "Bearer b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);

            var serializedData = JsonSerializer.Serialize(jsonDoc.RootElement);

            return Ok(serializedData);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error fetching agent data: {ex.Message}" });
        }
    }

    [HttpDelete("deleteAgent/{id}")]
    public async Task<IActionResult> DeleteAgent(string id)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"https://api.vapi.ai/assistant/{id}");
            requestMessage.Headers.Add("Authorization", "Bearer b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return StatusCode(500, new { error = $"Error deleting agent from external API: {errorMessage}" });
            }

            return Ok(new { message = "Agent deleted successfully" });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error deleting agent: {ex.Message}" });
        }
    }

    [HttpPost("deployAgent")]
    public async Task<IActionResult> DeployAgent([FromBody] dynamic payload)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.vapi.ai/phone-number")
            {
                Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", "Bearer b9a1b9a2-a2cc-41e1-a1e6-50e2f12e3217");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);

            return Ok(jsonDoc.RootElement);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { error = $"Error deploying agent: {ex.Message}" });
        }
    }





}
