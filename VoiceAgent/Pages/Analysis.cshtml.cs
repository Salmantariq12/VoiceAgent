using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VoiceAgent.Pages
{
    public class AnalysisModel : PageModel
    {
        public string AgentId { get; set; }
        public string AgentName { get; set; }

        public void OnGet()
        {
            AgentId = Request.Query["agentId"];
            AgentName = Request.Query["agentName"];
        }
    }
}
