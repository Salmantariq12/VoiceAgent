using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;

namespace VoiceAgent.Pages
{
    public class TranscriberModel : PageModel
    {
        public List<Language> Languages { get; private set; }

        [BindProperty]
        public string LanguageCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PageDataJson { get; set; }

        public TranscriberModel() { }

        public void OnGet()
        {
            var jsonData = TempData["JsonData"] as string;
            var pageData = new object();

            if (!string.IsNullOrEmpty(jsonData))
            {
                pageData = JsonSerializer.Deserialize<object>(jsonData);
            }

            PageDataJson = JsonSerializer.Serialize(pageData);

            Languages = new List<Language>
            {
                new Language { Code = "en", Name = "English" },
                new Language { Code = "es", Name = "Spanish" },
                new Language { Code = "fr", Name = "French" },
                new Language { Code = "It", Name = "Italian" }
            };
        }

        public IActionResult OnPost()
        {
            var providerName = Request.Form["Provider"].ToString();
            var languageCode = LanguageCode;

            if (providerName == "assembly-ai")
            {
                languageCode = "en";
            }

            var pageData = JsonSerializer.Deserialize<object>(PageDataJson);

            var transcriptionData = new
            {
                transcriber = new
                {
                    provider = providerName,
                    language = languageCode
                }
            };

            var combinedData = new
            {
                pageData = pageData,
                transcription = transcriptionData
            };

            string finalJsonData = JsonSerializer.Serialize(combinedData);
            TempData["JsonData"] = finalJsonData;

            return RedirectToPage("/Voice");
        }

    }

    public class Language
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
