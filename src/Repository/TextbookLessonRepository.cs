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

		public async Task CreateTextbookLessonAsync(CreateTextbookLessonRequest request, string userId)
		{
			var textbookLessonId = Guid.NewGuid().ToString();

			var textbookLesson = new TextbookLesson
			{
				Id = textbookLessonId,
				Title = request.Title,
				SpecialityId = request.SpecialityId,
				Contents = request.Contents.Select(c => new TextbookLessonContent
				{
					Id = Guid.NewGuid().ToString(),
					TextbookLessonId = textbookLessonId,
					Heading = c.Heading,
					Text = c.Text
				}).ToList(),
				CreatedBy = userId,
				UpdatedBy = userId,
				CreatedOn = DateTime.UtcNow,
				UpdatedOn = DateTime.UtcNow
			};

			context.TextbookLessons.Add(textbookLesson);
			await context.SaveChangesAsync();
		}

		public async Task<TextbookLesson> UpdateTextbookLessonAsync(UpdateTextbookLessonRequest request, string userId, bool isAdmin)
		{
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
			textbookLesson.UpdatedBy = userId;
			textbookLesson.UpdatedOn = DateTime.UtcNow;

			// Remove all old content
			context.TextbookLessonContents.RemoveRange(textbookLesson.Contents);

			// Add the new content
			textbookLesson.Contents = request.Contents.Select(c => new TextbookLessonContent
			{
				Id = Guid.NewGuid().ToString(), // Generate a new ID for each new content
				TextbookLessonId = textbookLesson.Id,
				Heading = c.Heading,
				Text = c.Text
			}).ToList();

			await context.SaveChangesAsync();
			return textbookLesson;
		}

		public async Task<TextbookLesson> AddTextbookLessonContentAsync(string textbookLessonId, CreateTextbookLessonContentRequest request, string userId, bool isAdmin)
		{
			var textbookLesson = await context.TextbookLessons.FindAsync(textbookLessonId);

			if (textbookLesson == null)
			{
				return null;
			}

			if (!isAdmin && textbookLesson.CreatedBy != userId)
			{
				throw new AccessDeniedException("You do not have permission to modify this textbook lesson");
			}

			var newContent = new TextbookLessonContent
			{
				Id = Guid.NewGuid().ToString(),
				TextbookLessonId = textbookLessonId,
				Heading = request.Heading,
				Text = request.Text
			};

			context.TextbookLessonContents.Add(newContent);
			textbookLesson.UpdatedBy = userId;
			textbookLesson.UpdatedOn = DateTime.UtcNow;

			await context.SaveChangesAsync();
			return textbookLesson;
		}

		public async Task<bool> DeleteTextbookLessonContentAsync(string contentId, string userId, bool isAdmin)
		{
			var content = await context.TextbookLessonContents.FindAsync(contentId);

			if (content == null)
			{
				return false;
			}

			var textbookLesson = await context.TextbookLessons.FindAsync(content.TextbookLessonId);

			if (!isAdmin && textbookLesson?.CreatedBy != userId)
			{
				throw new AccessDeniedException("You do not have permission to delete this content");
			}

			context.TextbookLessonContents.Remove(content);
			await context.SaveChangesAsync();
			return true;
		}

		public async Task<TextbookLessonResponse> GetTextbookLessonAsync(string id)
		{
			var textbookLesson = await context.TextbookLessons
				.Include(t => t.Speciality)
				.Include(t => t.Contents)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (textbookLesson == null)
			{
				return null;
			}

			return CreateTextbookLessonResponseModel(textbookLesson);
		}

		public async Task<List<TextbookLessonResponse>> GetTextbookLessonsBySpecialityAsync(string specialityId)
		{
			var lessons = await context.TextbookLessons
				.Where(t => t.SpecialityId == specialityId)
				.Include(t => t.Speciality)
				.Include(t => t.Contents)
				.ToListAsync();

			return lessons.Select(CreateTextbookLessonResponseModel).ToList();
		}

		public async Task<List<TextbookLessonResponse>> GetTextbookLessonsAsync()
		{
			var lessons = await context.TextbookLessons
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
				SpecialityId = lesson.SpecialityId,
				Speciality = new SpecialityViewModel
				{
					Id = lesson.Speciality.Id,
					Name = lesson.Speciality.Name
				},
				Contents = lesson.Contents.Select(c => new TextbookLessonContentResponse
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
