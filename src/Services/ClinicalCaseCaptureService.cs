using System.Text.Json;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Models.OpenAI;

namespace MedicLaunchApi.Services
{
  public class ClinicalCaseCaptureService
  {
    private readonly OpenAIService openAIService;

    public ClinicalCaseCaptureService(OpenAIService openAIService)
    {
      this.openAIService = openAIService;
    }

    private List<ChatMessage> ConstructChatPrompt(ClinicalCaseGenerationRequest caseDetails)
    {
      var messages = new List<ChatMessage>();

      var systemMessage = "You are an expert medical educator creating highly beneficial, structured, and concise clinical learning resources tailored for final-year medical students in the UK, preparing specifically for the UKMLA (Medical Licensing Assessment). You will receive anonymised patient information provided by medical students during their clinical placements. Using this scenario information, create a structured learning output containing the sections outlined below.\n\n## Student Input Template (example):\n- Patient Demographics: [e.g., \"55-year-old male, retired taxi driver\"]\n- Clinical Context: [e.g., GP clinic, Primary Care, Cardiology]\n- Presenting Complaint: [e.g., Chest pain]\n- Symptoms: [comma-separated keywords, e.g., chest tightness, sweating, nausea]\n- Brief History of Presenting Complaint: [1-2 sentence concise description]\n\n# 📋 Your Structured Output (follow precisely):\n## 🩺 1. Scenario Overview\nCreate a succinct, structured clinical vignette (2-3 sentences) clearly restating the scenario professionally and concisely for student reference.\n\n## 🎯 2. Key Learning Points\nList 3-5 clear, concise bullet points highlighting fundamental clinical learning aspects relevant to the patient's presentation. Tailor specifically for UKMLA preparation.\n\n## 🔍 3. Possible Differential Diagnoses\nProvide a concise, prioritised bullet-point list (3-5 items) of relevant differential diagnoses.\n\n## 📌 4. Management Plan\nClearly outline step-by-step initial management following current NICE or UK national guidelines. Include clearly structured sub-sections:\n- Initial Investigations\n- Immediate Management\n- Further Management\n- Referrals or Specialist Input (if appropriate)\n\n## 💡 5. Clinical Tips\nProvide 2 concise, practical clinical tips relevant to communication skills, clinical examination, procedural skills, or clinical reasoning in similar patient encounters.\n\n## 🚀 6. \"Boost My Learning\" - Advanced Clinical Insights\nShare one or two advanced clinical insights that go beyond the basic curriculum or standard guidelines. \nExamples include:\n- New guideline updates\n- Recent clinical studies or landmark trials\n- Relevant pathophysiology or clinical pearls that enhance clinical reasoning and professional distinction.\n\n# ⚠️ Important guidelines for your responses:\n- All content must strictly follow UK medical practice, referencing NICE, SIGN, or NHS guidance as appropriate.\n- Write clearly, succinctly, and professionally in British English.\n- Tailor content specifically to final-year medical students preparing for the UKMLA.\n- All content returned must strictly follow HTML format, where h3 is the largest denomination of headings used.";
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

    public async Task<string> GenerateClinicalCaseAsync(ClinicalCaseGenerationRequest caseDetails)
    {
      var messages = ConstructChatPrompt(caseDetails);
      var response = await openAIService.GenerateChatCompletion(messages: messages, modelName: "gpt-4.1");

      return response;
    }
  }
}