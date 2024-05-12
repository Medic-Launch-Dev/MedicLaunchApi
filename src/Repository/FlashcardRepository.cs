using Azure.Core;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class FlashcardRepository
    {
        private readonly ApplicationDbContext _context;
        public FlashcardRepository(ApplicationDbContext context)
        {
            _context = context;
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

            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            return;
        }

        public async Task<Flashcard> UpdateFlashcard(UpdateFlashcardRequest request, string userId)
        {
            var flashcard = await _context.Flashcards.FindAsync(request.Id);

            if (flashcard == null)
            {
                return null;
            }

            flashcard.Name = request.Name;
            flashcard.ImageUrl = request.ImageUrl;
            flashcard.SpecialityId = request.SpecialityId;
            flashcard.UpdatedBy = userId;
            flashcard.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return flashcard;
        }

        public async Task<FlashcardResponse> GetFlashcard(string id)
        {
            var flashcard = await _context.Flashcards.Include(m => m.Speciality).FirstOrDefaultAsync(m => m.Id == id);

            if (flashcard == null)
            {
                return null;
            }

            return CreateFlashCardResponseModel(flashcard);
        }

        private static FlashcardResponse CreateFlashCardResponseModel(Flashcard flashcard)
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
                CreatedOn = flashcard.CreatedOn
            };
        }

        public async Task<List<FlashcardResponse>> GetFlashcards()
        {
            var flashcards = await _context.Flashcards.Include(m => m.Speciality).ToListAsync();
            return flashcards.Select(CreateFlashCardResponseModel).ToList();
        }

        public async Task DeleteFlashcard(string id)
        {
            var flashcard = await _context.Flashcards.FindAsync(id);

            if (flashcard == null)
            {
                return;
            }

            _context.Flashcards.Remove(flashcard);
            await _context.SaveChangesAsync();
        }
    }
}
