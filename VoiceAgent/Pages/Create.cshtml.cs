using System.Collections.Generic;
using System.Linq; 
using System.Text.Json; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace VoiceAgent.Pages
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Message { get; set; }

        [BindProperty]
        public string SystemPrompt { get; set; }

        [BindProperty]
        public string Provider { get; set; }

        [BindProperty]
        public string Model { get; set; }

        [BindProperty]
        public List<string> KnowledgeBaseId { get; set; } 

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var jsonPayload = new
            {
                name = Name,
                firstMessage = Message,
                model = new
                {
                    provider = Provider,
                    model = Model,
                    messages = new[]
                    {
                        new
                        {
                            role = "assistant",
                            content = SystemPrompt
                        }
                    }
                },
                knowledgeBase = KnowledgeBaseId?.Select(id => new { knowledgeBaseId = id })
            };

            var jsonData = JsonSerializer.Serialize(jsonPayload);
            TempData["JsonData"] = jsonData;

            return RedirectToPage("/Transcriber");
        }
    }
}
