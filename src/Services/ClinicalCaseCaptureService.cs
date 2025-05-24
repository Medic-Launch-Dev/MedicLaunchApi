using System.Text.Json;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Models.OpenAI;

namespace MedicLaunchApi.Services
{
  public class ClinicalCaseService
  {
    private readonly OpenAIService openAIService;

    public ClinicalCaseService(OpenAIService openAIService)
    {
      this.openAIService = openAIService;
    }

    private List<ChatMessage> ConstructChatPrompt(GenerateClinicalCaseDTO caseDetails)
    {
      var messages = new List<ChatMessage>();

      var systemMessage = "You are an expert medical educator creating highly beneficial, structured, and concise clinical learning resources tailored for final-year medical students in the UK, preparing specifically for the UKMLA (Medical Licensing Assessment). You will receive anonymised patient information provided by medical students during their clinical placements. Using this scenario information, create a structured learning output containing the sections outlined below.\n\n## Student Input Template (example):\n- Patient Demographics: [e.g., \\\"55-year-old male, retired taxi driver\\\"]\n- Clinical Context: [e.g., GP clinic, Primary Care, Cardiology]\n- Presenting Complaint: [e.g., Chest pain]\n- Symptoms: [comma-separated keywords, e.g., chest tightness, sweating, nausea]\n- Brief History of Presenting Complaint: [1-2 sentence concise description]\n\n# 📋 Your Structured Output (follow precisely):\n<h3>🩺 1. Scenario Overview</h3>\nCreate a succinct, structured clinical vignette (2-3 sentences) clearly restating the scenario professionally and concisely for student reference.\n\n<h3>🎯 2. Key Learning Points</h3>\nList 3-5 clear, concise bullet points highlighting fundamental clinical learning aspects relevant to the patient's presentation. Tailor specifically for UKMLA preparation.\n\n<h3>🔍 3. Possible Differential Diagnoses</h3>\nProvide a concise, prioritised bullet-point list (3-5 items) of relevant differential diagnoses.\n\n<h3>📌 4. Management Plan</h3>\nClearly outline step-by-step initial management following current NICE or UK national guidelines. Include clearly structured sub-sections:\n<ul>\n<li><strong>Initial Investigations</strong></li>\n<li><strong>Immediate Management</strong></li>\n<li><strong>Further Management</strong></li>\n<li><strong>Referrals or Specialist Input</strong> (if appropriate)</li>\n</ul>\n\n<h3>💡 5. Clinical Tips</h3>\nProvide 2 concise, practical clinical tips relevant to communication skills, clinical examination, procedural skills, or clinical reasoning in similar patient encounters.\n\n<h3>🚀 6. \\\"Boost My Learning\\\" - Advanced Clinical Insights</h3>\nShare one or two advanced clinical insights that go beyond the basic curriculum or standard guidelines. \nExamples include:\n<ul>\n<li>New guideline updates</li>\n<li>Recent clinical studies or landmark trials</li>\n<li>Relevant pathophysiology or clinical pearls that enhance clinical reasoning and professional distinction.</li>\n</ul>\n\n# ⚠️ Important guidelines for your responses:\n- All content must strictly follow UK medical practice, referencing NICE, SIGN, or NHS guidance as appropriate.\n- Write clearly, succinctly, and professionally in British English.\n- Tailor content specifically to final-year medical students preparing for the UKMLA.\n- All content returned must be structured ONLY as valid JSON in the following format:\n\n{\n  \\\"title\\\": \\\"<A title that is an appropriate identifier for this case based on the case summary>\\\",\n  \\\"caseDetails\\\": \\\"<The clinical case that follows the structure mentioned above, formatted STRICTLY as valid HTML>\\\"\n}\n\n- Important: Ensure the entire response is valid JSON with properly escaped HTML content in the caseDetails field.\n- Do not include any explanatory text outside the JSON structure.";
      messages.Add(new ChatMessage { Role = "system", Content = [new ChatContent { Text = systemMessage }] });

      var userMessage = $@"
        # 📝 Now generate the Clinical Insight using the student's submitted details below:
        - Patient Demographics: {caseDetails.PatientDemographics}
        - Clinical Context: {caseDetails.ClinicalContext}
        - Presenting Complaint: {caseDetails.PresentingComplaint}
        - Symptoms: {caseDetails.Symptoms}
        - Brief History of Presenting Complaint: {caseDetails.ComplaintHistory}
      ";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = userMessage }] });

      return messages;
    }

    public async Task<ClinicalCaseDTO> GenerateClinicalCaseAsync(GenerateClinicalCaseDTO caseDetails)
    {
      var messages = ConstructChatPrompt(caseDetails);
      var response = await openAIService.GenerateChatCompletion(messages: messages, modelName: "gpt-4.1");

      var clinicalCase = JsonSerializer.Deserialize<ClinicalCaseDTO>(response, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });

      return clinicalCase!;
    }
  }
}