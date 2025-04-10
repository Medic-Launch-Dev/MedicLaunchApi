using System.Text.Json;
using MedicLaunchApi.Models.OpenAI;
using MedicLaunchApi.Models.QuestionDTOs;

namespace MedicLaunchApi.Services
{
  public class QuestionGenerationService : IQuestionGenerationService
  {
    private readonly OpenAIService openAIService;

    public QuestionGenerationService(OpenAIService openAIService)
    {
      this.openAIService = openAIService;
    }

    private List<ChatMessage> BuildQuestionTextAndExplanationPrompt(string conditions)
    {
      var messages = new List<ChatMessage>();

      var systemUserMessage = "Goal:\nI want a set of high-quality multiple-choice questions (MCQs) aimed at final-year medical students in the UK. These MCQs should assess clinical decision-making skills, patient management approaches, and core medical knowledge in an integrated manner, combining clinical scenarios with relevant basic sciences. Each question should be challenging and useful for assessing readiness for practice.\n\nReturn Format:\nProvide each MCQ structured clearly with the following elements:\n- A concise clinical vignette outlining a realistic patient scenario.\n- Clearly worded question stem.\n- Five answer options labelled (a) through (e), ensuring there is only one correct and unambiguously best answer per question. The other four distractors must be plausible but incorrect.\n- Clearly indicated correct answer.\n- A short, evidence-based explanation accompanying the correct answer, referencing current guidelines and best practices applicable within the UK healthcare system.\n\nYou MUST ENSURE that each response adheres STRICTLY to the following JSON schema:\n{\n  \"questionText\": \"string\",\n  \"options\": [\n    {\n      \"letter\": \"string (max length: 1)\",\n      \"text\": \"string\"\n    }\n  ],\n  \"correctAnswerLetter\": \"string (max length: 1)\",\n  \"explanation\": \"string\"\n}\nNote that the \"questionText\" and \"explanation\" fields are strings of HTML.\n\nWarnings:\n- Ensure that vignettes, stems, and answer choices avoid ambiguity. Do not write questions that are misleading or inaccurate.\n- Do not include outdated information; all questions and answers must reflect current, evidence-based guidelines from NICE CKS and uk guidelines.\n- Ensure integration with basic sciences is clear and relevant; do not choose overly specialised or obscure basic science information.\n- In the explanation, do not refer to answer choices by letter e.g. \"Answer B is incorrect because...\". Instead, refer to them by the answer text e.g. \"Cystic fibrosis is incorrect because...\".\n- Ensure that answer options are plain text strings, without any HTML formatting.\n\nContext:\nThese MCQs are intended to help final-year medical students preparing for their final exams and clinical practice in the UK. Students will have familiarity with common UK guidelines (NICE, SIGN, GMC recommendations) and will be preparing to enter medical practice within the NHS soon. Coverage should focus on high-yield topics which are common but also cover the areas outlined, clinically meaningful, and critical for safe patient care. The questions are designed to assess the student's ability to apply theoretical knowledge and guidelines to real-world clinical situations effectively.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = systemUserMessage }] });
      var example1UserMessage = "Turn the following into an MCQ: Colorectal Cancer - older patient, change in bowel habits, iron-deficiency anemia, rectal bleeding, colonoscopy reveal a mass. Diagnosis?";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example1UserMessage }] });
      var example1AssistantMessage = "{\n  \"questionText\": \"<p>A 68-year-old man presents to his GP with a three month history of a change in bowel habits, including alternating constipation and diarrhoea. He has also noticed occasional rectal bleeding and reports feeling increasingly fatigued. Blood tests reveal iron-deficiency anaemia and examination shows a small haemorrhoid. </p><p></p><p><strong>What is the single most likely diagnosis?</strong></p>\",\n  \"options\": [\n    {\n      \"letter\": \"A\",\n      \"text\": \"Colorectal Cancer\"\n    },\n    {\n      \"letter\": \"B\",\n      \"text\": \"Diverticular disease\"\n    },\n    {\n      \"letter\": \"C\",\n      \"text\": \"Haemorrhoids\"\n    },\n    {\n      \"letter\": \"D\",\n      \"text\": \"Ischaemic Colitis\"\n    },\n    {\n      \"letter\": \"E\",\n      \"text\": \"Ulcerative Colitis\"\n    }\n  ],\n  \"correctAnswerLetter\": \"A\",\n  \"explanation\": \"<p>The single most likely diagnosis is colorectal cancer, as the patient presents with classic red-flag symptoms, including a persistent change in bowel habits, iron-deficiency anaemia, fatigue, and rectal bleeding. While haemorrhoids are noted on examination, they do not account for systemic symptoms like anaemia or changes in bowel habits and should not be considered the sole cause of rectal bleeding in this context, particularly in an older patient.</p><p></p><p>Haemorrhoids typically cause bright red bleeding with defecation but do not explain iron-deficiency anaemia or altered bowel habits. Diverticular disease can cause rectal bleeding and bowel habit changes but is less likely to cause anaemia or fatigue without significant complications like chronic blood loss. Ulcerative colitis may present with rectal bleeding and anaemia, but it typically includes chronic abdominal pain, weight loss, or systemic symptoms like fever. Ischaemic colitis usually presents with acute abdominal pain and bloody diarrhoea, not a chronic change in bowel habits or anaemia.</p>\"\n}";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example1AssistantMessage }] });
      var example2UserMessage = "Turn the following into an MCQ: Acute Myocarditis - viral infection → develops cardiac symptoms - whats the dx?";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example2UserMessage }] });
      var example2AssistantMessage = "{\n  \"questionText\": \"<p>A 24-year-old male presents to the Emergency Department with a 5-day history of worsening fatigue, shortness of breath on exertion, and chest discomfort. He reports having a \\\"cold\\\" two weeks ago with symptoms of a sore throat, runny nose, and mild fever, which resolved on its own. Observations include a pulse of 110/min, blood pressure 100/60 mmHg. An ECG shows sinus tachycardia with non-specific ST-segment changes. A troponin test is positive, and a bedside echocardiogram reveals a mildly reduced ejection fraction.</p><p></p><p><strong>What is the most likely diagnosis?</strong></p>\",\n  \"options\": [\n    {\n      \"letter\": \"A\",\n      \"text\": \"Acute coronary syndrome\"\n    },\n    {\n      \"letter\": \"B\",\n      \"text\": \"Acute myocarditis\"\n    },\n    {\n      \"letter\": \"C\",\n      \"text\": \"Pericarditis\"\n    },\n    {\n      \"letter\": \"E\",\n      \"text\": \"Viral pleuritis \"\n    },\n    {\n      \"letter\": \"D\",\n      \"text\": \"Pulmonary embolism\"\n    }\n  ],\n  \"correctAnswerLetter\": \"B\",\n  \"explanation\": \"<p>Acute myocarditis is the most likely diagnosis because it often follows a viral infection and presents with symptoms of heart failure or chest pain. The history of a recent \\\"cold,\\\" combined with the cardiac symptoms, positive troponin, and reduced ejection fraction on echocardiogram, all point towards myocarditis as the underlying cause.</p><p></p><p>Acute coronary syndrome is less likely due to the patient’s age, lack of risk factors, and viral history. Pulmonary embolism usually presents with pleuritic chest pain and shortness of breath but does not account for reduced ejection fraction or viral illness. Pericarditis may cause chest pain and ECG changes but not typically a reduced ejection fraction. Viral pleuritis could cause chest pain but doesn\'t explain the positive troponin or reduced ejection fraction.</p>\"\n}";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example2AssistantMessage }] });
      var example3UserMessage = "Turn the following into an MCQ: Tuberculosis - visited Pakistan, night sweats, hemoptysis, apical crackles. Initial step in management?";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example3UserMessage }] });
      var example3AssistantMessage = "{\n  \"questionText\": \"<p>A 32-year-old man presents to the GP with a 3-week history of night sweats, persistent cough, and haemoptysis. He has recently returned from a visit to Pakistan. On examination, there are apical crackles in his lungs and a sweaty t-shirt.&nbsp;</p><p></p><p><strong><span style=\\\"color: inherit; font-family: inherit; font-size: inherit\\\">What is the most appropriate initial step in management?</span></strong></p>\",\n  \"options\": [\n    {\n      \"letter\": \"A\",\n      \"text\": \"Arrange for a Mantoux test\"\n    },\n    {\n      \"letter\": \"B\",\n      \"text\": \"Isolate the patient and obtain sputum samples for acid-fast bacilli testing \"\n    },\n    {\n      \"letter\": \"C\",\n      \"text\": \"Order a chest X-ray\"\n    },\n    {\n      \"letter\": \"D\",\n      \"text\": \"Refer for bronchoscopy and biopsy\"\n    },\n    {\n      \"letter\": \"E\",\n      \"text\": \"Start empirical anti-tuberculosis therapy\"\n    }\n  ],\n  \"correctAnswerLetter\": \"B\",\n  \"explanation\": \"<p>The patient’s history of travel to an area with a high prevalence of tuberculosis, combined with his symptoms of night sweats, haemoptysis, and apical crackles, strongly suggests pulmonary tuberculosis (TB). The most appropriate initial step in management is to isolate the patient and obtain sputum samples for acid-fast bacilli (AFB) testing. Isolation is critical to prevent the spread of TB, and obtaining sputum samples is essential for confirming the diagnosis through microbiological testing.&nbsp;</p><p></p><p>Starting empirical anti-tuberculosis therapy without confirmation is premature; a chest X-ray should follow patient isolation and sputum sampling; the Mantoux test is less useful for active TB; and bronchoscopy is reserved for inconclusive sputum results or non-pulmonary TB suspicion.</p>\"\n}";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example3AssistantMessage }] });

      var content = $"Turn the following into an MCQ: {conditions}";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = content }] });

      return messages;
    }

    private List<ChatMessage> BuildLearningPointsPrompt(string condition)
    {
      var messages = new List<ChatMessage>();

      var systemUserMessage = "**Goal:**\nI want a comprehensive clinical revision summary for final-year medical students preparing for the UKMLA exam.\n\n**Return Format:**\nBase the summary on NICE guidelines and write in UK English. It should include:\n- A brief overview of the condition.\n- Key signs and symptoms.\n- Relevant investigations.\n- Diagnosis criteria.\n- Management based on NICE guidance.\n- Important differentials.\n- High-yield revision tips. Make it structured and thorough enough for exam success.\n- You must return your answer STRICTLY adhering to HTML formatting.\n\n**Warnings:**\nEnsure that the clinical content is accurate and reflects the most recent NICE guidance. Avoid generic information; tailor it to the condition mentioned in the question.\n\n**For context:**\nThis is for a UKMLA exam revision tool. The condition to summarise is either the correct answer or the key learning point from the question scenario. Final-year medical students will use this as a high-quality study resource, so clarity, accuracy, and practical relevance are essential.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = systemUserMessage }] });
      var example1UserMessage = "Generate a comprehensive clinical revision summary for the following: Open-Angle Glaucoma.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example1UserMessage }] });
      var example1AssistantMessage = "<h3><strong>Open-Angle Glaucoma</strong></h3><p></p><p><strong>Overview:</strong> Open-angle glaucoma is a chronic, progressive optic neuropathy characterised by the gradual loss of peripheral vision due to increased intraocular pressure (IOP), leading to optic nerve damage. It is the most common type of glaucoma and is often asymptomatic until advanced stages.</p><p></p><p><strong>Pathophysiology:</strong></p><ul><li><p>The drainage angle between the iris and cornea remains open, but the trabecular meshwork becomes less efficient at draining aqueous humour.</p></li><li><p>This results in increased IOP, leading to optic nerve damage and visual field loss.</p></li></ul><p></p><p><strong>Risk Factors:</strong></p><ul><li><p>Age (&gt;60 years)</p></li><li><p>Family history of glaucoma</p></li><li><p>African or Hispanic descent</p></li><li><p>Chronic conditions: Myopia, diabetes, hypertension</p></li><li><p>Long-term steroid use</p></li></ul><p></p><p><strong>Symptoms:</strong></p><ul><li><p>Early stages: Often asymptomatic.</p></li><li><p>Late stages:</p><ul><li><p>Gradual loss of peripheral (side) vision.</p></li><li><p>\"Tunnel vision\" in advanced disease.</p></li><li><p>Central vision typically preserved until late stages.</p></li></ul></li></ul><p></p><p><strong>Diagnosis:</strong></p><p></p><ol><li><p><strong>Tonometry:</strong> Measures intraocular pressure (normal range: 10-21 mmHg).</p></li><li><p><strong>Gonioscopy:</strong> Confirms an open anterior chamber angle.</p></li><li><p><strong>Optic Disc Examination:</strong> Shows optic disc cupping and thinning of the neuroretinal rim.</p></li><li><p><strong>Visual Field Testing:</strong> Detects peripheral vision loss.</p></li><li><p><strong>Optical Coherence Tomography (OCT):</strong> Measures retinal nerve fibre layer thickness.</p></li></ol><p></p><p><strong>Management:</strong></p><p></p><ul><li><p><strong>Medications:</strong></p><ol><li><p><strong>Prostaglandin analogues</strong> (e.g., latanoprost): First-line treatment; increases aqueous humour outflow.</p></li><li><p><strong>Beta-blockers</strong> (e.g., timolol): Reduces aqueous humour production.</p></li><li><p><strong>Alpha-agonists</strong> (e.g., brimonidine): Decreases aqueous production and increases outflow.</p></li><li><p><strong>Carbonic anhydrase inhibitors</strong> (e.g., acetazolamide): Lowers aqueous production.</p></li><li><p><strong>Miotics</strong> (e.g., pilocarpine): Increases outflow by contracting the ciliary muscle (less commonly used).</p></li></ol></li><li><p><strong>Laser Therapy:</strong></p><ul><li><p><strong>Laser trabeculoplasty:</strong> Used when medications are insufficient. Increases fluid outflow.</p></li></ul></li><li><p><strong>Surgery:</strong></p><ul><li><p><strong>Trabeculectomy</strong> or <strong>aqueous shunt placement</strong>: For advanced or refractory cases to create new drainage pathways.</p></li></ul></li></ul><p></p><p><strong>Follow-Up:</strong></p><ul><li><p>Regular monitoring of IOP, visual fields, and optic nerve changes is essential to prevent disease progression.</p></li></ul>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example1AssistantMessage }] });
      var example2UserMessage = "Generate a comprehensive clinical revision summary for the following: Bulimia Nervosa.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example2UserMessage }] });
      var example2AssistantMessage = "<h3><strong>Bulimia Nervosa</strong></h3><p></p><ul><li><p><strong>Body Weight</strong>: People with bulimia often maintain a normal body weight, but it may fluctuate.</p></li></ul><p></p><ul><li><p><strong>Binge and Purge</strong>: The disorder is characterised by binge eating followed by purging behaviours like self-induced vomiting or laxative use.</p></li></ul><p></p><ul><li><p><strong>Physical Findings</strong>:</p><ul><li><p>Common Presentations: May include abdominal pain or reflux symptoms.</p></li><li><p>Blood Gas Analysis: Can reveal <strong>alkalosis</strong> caused by vomiting out stomach acid.</p></li><li><p><strong>Hypokalaemia</strong>: Due to purging behaviours.</p></li><li><p><strong>Dental Erosion</strong>: From repeated exposure to stomach acid.</p></li><li><p><strong>Swollen Salivary Glands</strong>: Leading to facial or jaw swelling.</p></li><li><p>Mouth <strong>Ulcers</strong> and Gastro-oesophageal <strong>Reflux</strong>: Due to repeated vomiting.</p></li><li><p><strong>Russell’s Sign</strong>: Calluses on knuckles from inducing vomiting.</p></li></ul></li></ul>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example2AssistantMessage }] });
      var example3UserMessage = "Generate a comprehensive clinical revision summary for the following: Polypharmacy and Drug Interactions.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example3UserMessage }] });
      var example3AssistantMessage = "<h3><strong>Polypharmacy and Drug Interactions</strong></h3><p></p><ul><li><p><strong>Definition:</strong> Polypharmacy is the use of multiple medications by a patient, often defined as the use of five or more drugs.</p><p></p></li><li><p><strong>Risks:</strong> Increased risk of drug-drug interactions, adverse drug reactions, medication non-adherence, and potentially inappropriate prescribing.</p><p></p></li><li><p><strong>Common Interactions:</strong></p><ul><li><p><strong>Pharmacokinetic:</strong> Alterations in drug absorption, distribution, metabolism, or excretion (e.g., warfarin and antibiotics).</p></li><li><p><strong>Pharmacodynamic:</strong> Drugs that have additive, synergistic, or antagonistic effects (e.g., ACE inhibitors and NSAIDs).</p><p></p></li></ul></li><li><p><strong>Management:</strong></p><ul><li><p><strong>Medication Review:</strong> Regularly review all medications to assess necessity, efficacy, and safety.</p></li><li><p><strong>Deprescribing:</strong> Gradual reduction or stopping of medications that are no longer necessary or potentially harmful.</p></li><li><p><strong>Patient Education:</strong> Ensuring patients understand their medications, possible interactions, and the importance of adherence.</p></li></ul></li></ul>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example3AssistantMessage }] });

      var content = $"Generate a comprehensive clinical revision summary for the following: {condition}";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = content }] });

      return messages;
    }

    private List<ChatMessage> BuildClinicalTipsPrompt(string condition)
    {
      var messages = new List<ChatMessage>();

      var systemUserMessage = "**Goal:**\nI want a concise and practical clinical tip written in UK English that enhances a junior doctor’s real-world practice.\n\n**Return Format:**\nProvide a one-to-two sentence tip based around:\n- Communication skills\n- Examination technique\n- Procedural know-how\n- Disease detection strategies\n- Professional growth on the ward\n\nMake it specific and memorable — something that can be easily applied during NHS clinical placements.\n\nEnsure that your response adheres STRICTLY to HTML formatting.\n\n**Warnings:**\nAvoid vague or overly generic tips. Focus on delivering something insightful or nuanced that a wise consultant would pass on.\n\n**For context:**\nThis is for junior doctors and final-year medical students. The tip should relate to the condition referenced in the previous question, or be applicable to clinical practice based on the conditions listed. The tone should reflect the wisdom of an experienced NHS consultant.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = systemUserMessage }] });
      var example1UserMessage = "Generate a concise and practical clinical tip for: Drug Allergy (Amoxicillin-induced Rash).";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example1UserMessage }] });
      var example1AssistantMessage = "<p>Clearly document any drug allergies in the patient’s medical record and educate the patient about the need to avoid beta-lactam antibiotics in the future.</p>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example1AssistantMessage }] });
      var example2UserMessage = "Generate a concise and practical clinical tip for: Malignant Melanoma.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example2UserMessage }] });
      var example2AssistantMessage = "<p>When assessing pigmented lesions, use the ABCDE rule (Asymmetry, Border irregularity, Colour variation, Diameter &gt;6mm, and Evolution) to differentiate benign from suspicious lesions requiring urgent referral.</p>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example2AssistantMessage }] });
      var example3UserMessage = "Generate a concise and practical clinical tip for: Adenocarcinoma of the Lungs.";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example3UserMessage }] });
      var example3AssistantMessage = "<p>When discussing treatment options for lung cancer, ensure to communicate the importance of genetic testing to the patient, as targeted therapies based on these results can significantly improve outcomes and quality of life.</p>";
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example3AssistantMessage }] });

      var content = $"Generate a concise and practical clinical tip for: {condition}";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = content }] });

      return messages;
    }

    public async Task<QuestionTextAndExplanation> GenerateQuestionTextAndExplanationAsync(string content)
    {
      var messages = BuildQuestionTextAndExplanationPrompt(content);

      var response = await openAIService.GenerateChatCompletion(messages);

      JsonSerializerOptions jsonOptions = new()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      return JsonSerializer.Deserialize<QuestionTextAndExplanation>(response, jsonOptions);
    }

    public async Task<string> GenerateLearningPointsAsync(string condition)
    {
      var messages = BuildLearningPointsPrompt(condition);
      var response = await openAIService.GenerateChatCompletion(messages);
      return response;
    }

    public async Task<string> GenerateClinicalTipsAsync(string condition)
    {
      var messages = BuildClinicalTipsPrompt(condition);
      var response = await openAIService.GenerateChatCompletion(messages);
      return response;
    }
  }
}