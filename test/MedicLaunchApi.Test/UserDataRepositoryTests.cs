using MedicLaunchApi.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Models.ViewModels;

namespace MedicLaunchApi.Test
{
    [TestClass]
    public class UserDataRepositoryTests
    {
        private ApplicationDbContext context;
        private UserDataRepository userDataRepository;

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

            userDataRepository = new UserDataRepository(context);
        }

        [TestMethod]
        public async Task CreateNoteAsync_WithValidRequest_ShouldCreateNote()
        {
            var request = new CreateNoteRequest()
            {
                Content = "Test Note",
                SpecialityId = "1"
            };

            var userId = "1";

            await userDataRepository.CreateNoteAsync(request, userId);

            var notes = await context.Notes.ToListAsync();
            Assert.AreEqual(1, notes.Count);
            Assert.AreEqual("1", notes[0].SpecialityId);
            Assert.IsNull(notes[0].QuestionId);
            Assert.IsNull(notes[0].FlashcardId);
        }

        [TestMethod]
        public async Task GetNotesAsync_WithValidUserId_ShouldReturnNotes()
        {
            var userId = "1";
            var note1 = new CreateNoteRequest()
            {
                Content = "Test Note 1",
                SpecialityId = "1"
            };

            var note2 = new CreateNoteRequest()
            {
                Content = "Test Note 2",
                SpecialityId = "2"
            };

            await userDataRepository.CreateNoteAsync(note1, userId);
            await userDataRepository.CreateNoteAsync(note2, userId);

            var notes = await userDataRepository.GetNotesAsync(userId);
            Assert.AreEqual(2, notes.Count());
        }

        [TestMethod]
        public async Task DeleteNoteAsync_WithValidNoteId_ShouldDeleteNote()
        {
            var userId = "1";

            var note = new CreateNoteRequest()
            {
                Content = "Test Note",
                SpecialityId = "1"
            };

            await userDataRepository.CreateNoteAsync(note, userId);

            // delete note
            var notes = await context.Notes.ToListAsync();
            await userDataRepository.DeleteNoteAsync(notes[0].Id, userId);

            var newNotes = await userDataRepository.GetNotesAsync(userId);
            Assert.AreEqual(0, newNotes.Count());
        }
    }
}
