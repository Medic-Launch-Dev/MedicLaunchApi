using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class UserDataRepository
    {
        private readonly ApplicationDbContext dbContext;

        public UserDataRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateNoteAsync(CreateNoteRequest request, string userId)
        {
            var note = new Note
            {
                Id = Guid.NewGuid().ToString(),
                Content = request.Content,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            };

            if(!string.IsNullOrEmpty(request.SpecialityId))
            {
                note.SpecialityId = request.SpecialityId;
            }

            if (!string.IsNullOrEmpty(request.QuestionId))
            {
                note.QuestionId = request.QuestionId;
            }

            if (!string.IsNullOrEmpty(request.FlashcardId))
            {
                note.FlashcardId = request.FlashcardId;
            }

            await dbContext.Notes.AddAsync(note);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Note>> GetNotesAsync(string userId)
        {
            return await dbContext.Notes.Where(n => n.UserId == userId).Include(m => m.Speciality).ToListAsync();
        }

        public async Task DeleteNoteAsync(string noteId, string userId)
        {
            var note = await dbContext.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

            if (note != null)
            {
                dbContext.Notes.Remove(note);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(UpdateNoteRequest updateNoteRequest, string userId)
        {
            var note = await dbContext.Notes.FirstOrDefaultAsync(n => n.Id == updateNoteRequest.Id && n.UserId == userId);

            if (note != null)
            {
                note.Content = updateNoteRequest.Content;
                note.SpecialityId = updateNoteRequest.SpecialityId;
                note.UpdatedOn = DateTime.UtcNow;
                note.QuestionId = updateNoteRequest.QuestionId;
                note.FlashcardId = updateNoteRequest.FlashcardId;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
