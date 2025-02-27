using MedicLaunchApi.Data;

namespace MedicLaunchApi.Models.ViewModels
{
	public class CreateTextbookLessonRequest
	{
		public string Title { get; set; }

		public string SpecialityId { get; set; }

		public string? QuestionId { get; set; }

		public List<CreateTextbookLessonContentRequest> Contents { get; set; }

		public bool IsSubmitted { get; set; } = false;
	}

	public class UpdateTextbookLessonRequest
	{
		public string Id { get; set; }

		public string Title { get; set; }

		public string SpecialityId { get; set; }

		public string? QuestionId { get; set; }

		public List<UpdateTextbookLessonContentRequest> Contents { get; set; }

		public bool IsSubmitted { get; set; }
	}

	public class TextbookLessonResponse
	{
		public string Id { get; set; }

		public string Title { get; set; }

		public string SpecialityId { get; set; }

		public SpecialityViewModel Speciality { get; set; }

		public string? QuestionId { get; set; }

		public List<TextbookLessonContentResponse> Contents { get; set; }

		// Include the new flag in the response
		public bool IsSubmitted { get; set; }

	}

	public class CreateTextbookLessonContentRequest
	{
		public string Heading { get; set; }

		public string Text { get; set; }
	}

	public class UpdateTextbookLessonContentRequest
	{

		public string Heading { get; set; }

		public string Text { get; set; }
	}

	public class TextbookLessonContentResponse
	{
		public string Id { get; set; }

		public string TextbookLessonId { get; set; }

		public string Heading { get; set; }

		public string Text { get; set; }

	}

	public class GenerateTextbookLessonContentRequest
	{
		public string LearningPoints { get; set; }
		public string SpecialityId { get; set; }
	}
}
