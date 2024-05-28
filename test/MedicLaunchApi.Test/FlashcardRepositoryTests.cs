using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MedicLaunchApi.Test
{
    [TestClass]
    public class FlashcardRepositoryTests
    {
        private ApplicationDbContext context;
        private FlashcardRepository flashcardRepository;

        [TestInitialize]
        public void Setup()
        {
            var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi", new InMemoryDatabaseRoot())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            context = new ApplicationDbContext(options);
            var azureBlobClient = new Mock<Storage.IAzureBlobClient>();

            flashcardRepository = new FlashcardRepository(context, azureBlobClient.Object);
        }

        [TestMethod]
        public async Task CreateFlashcardAsync_WithValidRequest_ShouldCreateFlashcard()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await context.Specialities.AddAsync(speciality);
            await context.SaveChangesAsync();

            var request = new CreateFlashcardRequest()
            {
                Name = "Acute Medicine Flashcard",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg",
                SpecialityId = "1"
            };

            await flashcardRepository.CreateFlashcardAsync(request, "1");

            var flashcards = await flashcardRepository.GetFlashcards();
            Assert.AreEqual(1, flashcards.Count);
            Assert.AreEqual("Acute Medicine Flashcard", flashcards[0].Name);
            Assert.AreEqual("https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg", flashcards[0].ImageUrl);
            Assert.AreEqual("1", flashcards[0].SpecialityId);
        }

        [TestMethod]
        public async Task UpdateFlashcardAsync_WithValidRequest_ShouldUpdateFlashcard()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            var anotherSpeciality = new Speciality()
            {
                Id = "2",
                Name = "Cardiology"
            };

            await context.Specialities.AddAsync(speciality);
            await context.Specialities.AddAsync(anotherSpeciality);
            await context.SaveChangesAsync();

            var flashcard = new Flashcard()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg",
                SpecialityId = "1",
                CreatedBy = "1"
            };

            await context.Flashcards.AddAsync(flashcard);
            await context.SaveChangesAsync();

            var request = new UpdateFlashcardRequest()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard Updated",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine_updated.jpg",
                SpecialityId = "2"
            };

            await flashcardRepository.UpdateFlashcardAsync(request, "1", false);

            var flashcards = await flashcardRepository.GetFlashcards();
            Assert.AreEqual(1, flashcards.Count);
            Assert.AreEqual("Acute Medicine Flashcard Updated", flashcards[0].Name);
            Assert.AreEqual("https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine_updated.jpg", flashcards[0].ImageUrl);
            Assert.AreEqual("2", flashcards[0].SpecialityId);
        }

        [TestMethod]
        public async Task DeleteFlashcardAsync_WithValidFlashcardId_ShouldDeleteFlashcard()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await context.Specialities.AddAsync(speciality);
            await context.SaveChangesAsync();

            var flashcard = new Flashcard()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg",
                SpecialityId = "1",
                CreatedBy = "1"
            };

            await context.Flashcards.AddAsync(flashcard);
            await context.SaveChangesAsync();

            await flashcardRepository.DeleteFlashcardAsync("1");

            var flashcards = await flashcardRepository.GetFlashcards();
            Assert.AreEqual(0, flashcards.Count);
        }

        [TestMethod]
        public async Task UpdateFlashcardAsync_WithInvalidUser_ShouldThrowException()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await context.Specialities.AddAsync(speciality);
            await context.SaveChangesAsync();

            var flashcard = new Flashcard()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg",
                SpecialityId = "1",
                CreatedBy = "1"
            };

            await context.Flashcards.AddAsync(flashcard);
            await context.SaveChangesAsync();

            var request = new UpdateFlashcardRequest()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard Updated",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine_updated.jpg",
                SpecialityId = "2"
            };

            await Assert.ThrowsExceptionAsync<AccessDeniedException>(() => flashcardRepository.UpdateFlashcardAsync(request, "2", false));
        }

        [TestMethod]
        public async Task UpdateFlashcardAsync_WithAdminUser_ShouldUpdateFlashcard()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await context.Specialities.AddAsync(speciality);
            await context.SaveChangesAsync();

            var flashcard = new Flashcard()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine.jpg",
                SpecialityId = "1",
                CreatedBy = "1"
            };

            await context.Flashcards.AddAsync(flashcard);
            await context.SaveChangesAsync();

            var request = new UpdateFlashcardRequest()
            {
                Id = "1",
                Name = "Acute Medicine Flashcard Updated",
                ImageUrl = "https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine_updated.jpg",
                SpecialityId = "1"
            };

            await flashcardRepository.UpdateFlashcardAsync(request, "2", true);

            var flashcards = await flashcardRepository.GetFlashcards();
            Assert.AreEqual(1, flashcards.Count);
            Assert.AreEqual("Acute Medicine Flashcard Updated", flashcards[0].Name);
            Assert.AreEqual("https://mediclaunch.blob.core.windows.net/flashcards/acute_medicine_updated.jpg", flashcards[0].ImageUrl);
            Assert.AreEqual("1", flashcards[0].SpecialityId);
        }
    }
}
