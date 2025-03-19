using MedicLaunchApi.Authorization;
using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
	public class TextbookLessonRepository
	{
		private readonly ApplicationDbContext context;

		public TextbookLessonRepository(ApplicationDbContext context)
		{
			this.context = context;
		}

		public async Task<string> CreateTextbookLessonAsync(CreateTextbookLessonRequest request, string userId)
		{
			var speciality = await context.Specialities.FindAsync(request.SpecialityId);
			if (speciality == null)
			{
				throw new ArgumentException($"Speciality with ID {request.SpecialityId} not found");
			}

			if (!string.IsNullOrEmpty(request.QuestionId))
			{
				var question = await context.Questions.FindAsync(request.QuestionId);
				if (question == null)
				{
					throw new ArgumentException($"Question with ID {request.QuestionId} not found");
				}
			}

			var textbookLessonId = Guid.NewGuid().ToString();

			var textbookLesson = new TextbookLesson
			{
				Id = textbookLessonId,
				Title = request.Title,
				SpecialityId = request.SpecialityId,
				QuestionId = request.QuestionId,
				Contents = request.Contents.Select((c, index) => new TextbookLessonContent
				{
					Id = Guid.NewGuid().ToString(),
					TextbookLessonId = textbookLessonId,
					Heading = c.Heading,
					Text = c.Text,
					Order = index
				}).ToList(),
				CreatedBy = userId,
				UpdatedBy = userId,
				CreatedOn = DateTime.UtcNow,
				UpdatedOn = DateTime.UtcNow,
				IsSubmitted = request.IsSubmitted
			};

			context.TextbookLessons.Add(textbookLesson);
			await context.SaveChangesAsync();

			return textbookLessonId;
		}

		public async Task<TextbookLesson> UpdateTextbookLessonAsync(UpdateTextbookLessonRequest request, string userId, bool isAdmin)
		{
			var speciality = await context.Specialities.FindAsync(request.SpecialityId);
			if (speciality == null)
			{
				throw new ArgumentException($"Speciality with ID {request.SpecialityId} not found");
			}

			if (!string.IsNullOrEmpty(request.QuestionId))
			{
				var question = await context.Questions.FindAsync(request.QuestionId);
				if (question == null)
				{
					throw new ArgumentException($"Question with ID {request.QuestionId} not found");
				}
			}

			var textbookLesson = await context.TextbookLessons
				.Include(t => t.Contents)
				.FirstOrDefaultAsync(t => t.Id == request.Id);

			if (textbookLesson == null)
			{
				return null;
			}

			if (!isAdmin && textbookLesson.CreatedBy != userId)
			{
				throw new AccessDeniedException("You do not have permission to update this textbook lesson");
			}

			textbookLesson.Title = request.Title;
			textbookLesson.SpecialityId = request.SpecialityId;
			textbookLesson.QuestionId = request.QuestionId;
			textbookLesson.UpdatedBy = userId;
			textbookLesson.UpdatedOn = DateTime.UtcNow;
			textbookLesson.IsSubmitted = request.IsSubmitted;

			context.TextbookLessonContents.RemoveRange(textbookLesson.Contents);
			textbookLesson.Contents = request.Contents.Select((c, index) => new TextbookLessonContent
			{
				Id = Guid.NewGuid().ToString(),
				TextbookLessonId = textbookLesson.Id,
				Heading = c.Heading,
				Text = c.Text,
				Order = index  // Use array index as order
			}).ToList();

			await context.SaveChangesAsync();
			return textbookLesson;
		}

		public async Task<TextbookLessonResponse> GetTextbookLessonAsync(string id, bool isAdmin = false)
		{
			var query = context.TextbookLessons
				.Include(t => t.Speciality)
				.Include(t => t.Contents)
				.Where(t => t.Id == id);

			if (!isAdmin)
			{
				query = query.Where(t => t.IsSubmitted);
			}

			var textbookLesson = await query.FirstOrDefaultAsync();

			if (textbookLesson == null)
			{
				return null;
			}

			return CreateTextbookLessonResponseModel(textbookLesson);
		}

		public async Task<List<TextbookLessonResponse>> GetTextbookLessonsBySpecialityAsync(string specialityId, bool isAdmin)
		{
			var query = context.TextbookLessons.Where(t => t.SpecialityId == specialityId);

			if (!isAdmin)
			{
				query = query.Where(t => t.IsSubmitted);
			}

			var lessons = await query
				.Include(t => t.Speciality)
				.Include(t => t.Contents)
				.ToListAsync();

			return lessons.Select(CreateTextbookLessonResponseModel).ToList();
		}

		public async Task<List<TextbookLessonResponse>> GetTextbookLessonsAsync(bool isAdmin)
		{
			var query = context.TextbookLessons.AsQueryable();

			if (!isAdmin)
			{
				query = query.Where(t => t.IsSubmitted);
			}

			var lessons = await query
				.Include(t => t.Speciality)
				.Include(t => t.Contents)
				.ToListAsync();

			return lessons.Select(CreateTextbookLessonResponseModel).ToList();
		}

		public async Task DeleteTextbookLessonAsync(string id)
		{
			var textbookLesson = await context.TextbookLessons.FindAsync(id);

			if (textbookLesson == null)
			{
				return;
			}

			context.TextbookLessons.Remove(textbookLesson);
			await context.SaveChangesAsync();
		}

		private static TextbookLessonResponse CreateTextbookLessonResponseModel(TextbookLesson lesson)
		{
			return new TextbookLessonResponse
			{
				Id = lesson.Id,
				Title = lesson.Title,
				IsSubmitted = lesson.IsSubmitted,
				QuestionId = lesson.QuestionId,
				SpecialityId = lesson.SpecialityId,
				Speciality = new SpecialityViewModel
				{
					Id = lesson.Speciality.Id,
					Name = lesson.Speciality.Name
				},
				Contents = lesson.Contents
					.OrderBy(c => c.Order)
					.Select(c => new TextbookLessonContentResponse
					{
						Id = c.Id,
						TextbookLessonId = c.TextbookLessonId,
						Heading = c.Heading,
						Text = c.Text,
					}).ToList()
			};
		}
	}
}
