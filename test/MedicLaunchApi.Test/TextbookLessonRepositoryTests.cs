using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicLaunchApi.Test
{
	[TestClass]
	public class TextbookLessonRepositoryTests
	{
		private ApplicationDbContext context;
		private TextbookLessonRepository textbookLessonRepository;

		[TestInitialize]
		public void Setup()
		{
			var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();

			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MedicLaunchApi")
				.UseInternalServiceProvider(serviceProvider)
				.Options;

			context = new ApplicationDbContext(options);
			textbookLessonRepository = new TextbookLessonRepository(context);
		}

		[TestMethod]
		public async Task CreateTextbookLessonAsync_WithValidRequest_ShouldCreateTextbookLesson()
		{
			var request = new CreateTextbookLessonRequest
			{
				Title = "Acute Medicine Textbook Lesson",
				SpecialityId = "1",
				Contents = new List<CreateTextbookLessonContentRequest>
				{
					new CreateTextbookLessonContentRequest { Heading = "Introduction", Text = "This is an introduction." },
					new CreateTextbookLessonContentRequest { Heading = "Chapter 1", Text = "This is chapter 1." }
				}
			};

			await textbookLessonRepository.CreateTextbookLessonAsync(request, "1");

			var textbookLessons = await context.TextbookLessons.ToListAsync();
			Assert.AreEqual(1, textbookLessons.Count);
			Assert.AreEqual("Acute Medicine Textbook Lesson", textbookLessons[0].Title);
			Assert.AreEqual("1", textbookLessons[0].SpecialityId);
			Assert.AreEqual(2, textbookLessons[0].Contents.Count);
		}

		[TestMethod]
		public async Task UpdateTextbookLessonAsync_WithValidRequest_ShouldUpdateTextbookLesson()
		{
			var request = new CreateTextbookLessonRequest
			{
				Title = "Acute Medicine Textbook Lesson",
				SpecialityId = "1",
				Contents = new List<CreateTextbookLessonContentRequest>
		{
			new CreateTextbookLessonContentRequest { Heading = "Introduction", Text = "This is an introduction." }
		}
			};

			await textbookLessonRepository.CreateTextbookLessonAsync(request, "1");

			var lessonToUpdate = await context.TextbookLessons.FirstOrDefaultAsync();
			var updateRequest = new UpdateTextbookLessonRequest
			{
				Id = lessonToUpdate.Id,
				Title = "Updated Acute Medicine Textbook Lesson",
				SpecialityId = "2",
				Contents = new List<UpdateTextbookLessonContentRequest> // Corrected type here
        {
			new UpdateTextbookLessonContentRequest { Heading = "Updated Introduction", Text = "This is an updated introduction." }
		}
			};

			await textbookLessonRepository.UpdateTextbookLessonAsync(updateRequest, "1", true);

			var updatedLesson = await context.TextbookLessons.FirstOrDefaultAsync();
			Assert.AreEqual("Updated Acute Medicine Textbook Lesson", updatedLesson.Title);
			Assert.AreEqual("2", updatedLesson.SpecialityId);
			Assert.AreEqual("Updated Introduction", updatedLesson.Contents.First().Heading);
		}

		[TestMethod]
		public async Task DeleteTextbookLessonAsync_WithValidLessonId_ShouldDeleteTextbookLesson()
		{
			var request = new CreateTextbookLessonRequest
			{
				Title = "Acute Medicine Textbook Lesson",
				SpecialityId = "1",
				Contents = new List<CreateTextbookLessonContentRequest>
				{
					new CreateTextbookLessonContentRequest { Heading = "Introduction", Text = "This is an introduction." }
				}
			};

			await textbookLessonRepository.CreateTextbookLessonAsync(request, "1");

			var lessonToDelete = await context.TextbookLessons.FirstOrDefaultAsync();
			await textbookLessonRepository.DeleteTextbookLessonAsync(lessonToDelete.Id);

			var textbookLessons = await context.TextbookLessons.ToListAsync();
			Assert.AreEqual(0, textbookLessons.Count);
		}

		[TestMethod]
		public async Task UpdateTextbookLessonAsync_WithInvalidUser_ShouldThrowException()
		{
			var request = new CreateTextbookLessonRequest
			{
				Title = "Acute Medicine Textbook Lesson",
				SpecialityId = "1",
				Contents = new List<CreateTextbookLessonContentRequest>
		{
			new CreateTextbookLessonContentRequest { Heading = "Introduction", Text = "This is an introduction." }
		}
			};

			await textbookLessonRepository.CreateTextbookLessonAsync(request, "1");

			var lessonToUpdate = await context.TextbookLessons.FirstOrDefaultAsync();
			var updateRequest = new UpdateTextbookLessonRequest
			{
				Id = lessonToUpdate.Id,
				Title = "Updated Acute Medicine Textbook Lesson",
				SpecialityId = "2",
				Contents = new List<UpdateTextbookLessonContentRequest> // Use UpdateTextbookLessonContentRequest
        {
			new UpdateTextbookLessonContentRequest { Heading = "Updated Introduction", Text = "This is an updated introduction." }
		}
			};

			// Simulating an access violation scenario
			await Assert.ThrowsExceptionAsync<AccessDeniedException>(() => textbookLessonRepository.UpdateTextbookLessonAsync(updateRequest, "2", false));
		}
	}
}
