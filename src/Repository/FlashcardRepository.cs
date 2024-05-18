using Azure.Core;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Storage;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class FlashcardRepository
    {
        private readonly ApplicationDbContext context;
        private readonly AzureBlobClient azureBlobClient;
        public FlashcardRepository(ApplicationDbContext context, AzureBlobClient azureBlobClient)
        {
            this.context = context;
            this.azureBlobClient = azureBlobClient;
        }

        public async Task CreateFlashcard(CreateFlashcardRequest request, string userId)
        {
            var flashcard = new Flashcard
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                SpecialityId = request.SpecialityId,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            context.Flashcards.Add(flashcard);
            await context.SaveChangesAsync();

            return;
        }

        public async Task<Flashcard> UpdateFlashcard(UpdateFlashcardRequest request, string userId)
        {
            var flashcard = await context.Flashcards.FindAsync(request.Id);

            if (flashcard == null)
            {
                return null;
            }

            flashcard.Name = request.Name;
            flashcard.ImageUrl = request.ImageUrl;
            flashcard.SpecialityId = request.SpecialityId;
            flashcard.UpdatedBy = userId;
            flashcard.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return flashcard;
        }

        public async Task<FlashcardResponse> GetFlashcard(string id)
        {
            var flashcard = await context.Flashcards.Include(m => m.Speciality).FirstOrDefaultAsync(m => m.Id == id);
            var noteForFlashcard = await context.Notes.FirstOrDefaultAsync(m => m.FlashcardId == id);

            if (flashcard == null)
            {
                return null;
            }

            return CreateFlashCardResponseModel(flashcard, noteForFlashcard);
        }

        private static FlashcardResponse CreateFlashCardResponseModel(Flashcard flashcard, Note? noteForFlashcard = null)
        {
            return new FlashcardResponse
            {
                Id = flashcard.Id,
                Name = flashcard.Name,
                ImageUrl = flashcard.ImageUrl,
                SpecialityId = flashcard.SpecialityId,
                Speciality = new SpecialityViewModel
                {
                    Id = flashcard.Speciality.Id,
                    Name = flashcard.Speciality.Name
                },
                CreatedBy = flashcard.CreatedBy,
                CreatedOn = flashcard.CreatedOn,
                Note = noteForFlashcard?.Content
            };
        }

        public async Task<List<FlashcardResponse>> GetFlashcards()
        {
            var flashcards = await context.Flashcards.Include(m => m.Speciality).ToListAsync();
            return flashcards.Select(flashcard => CreateFlashCardResponseModel(flashcard)).ToList();
        }

        public async Task DeleteFlashcard(string id)
        {
            var flashcard = await context.Flashcards.FindAsync(id);

            if (flashcard == null)
            {
                return;
            }

            context.Flashcards.Remove(flashcard);
            await context.SaveChangesAsync();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            return await azureBlobClient.UploadImageAsyc(file);
        }
    }
}
