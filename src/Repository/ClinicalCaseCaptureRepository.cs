using MedicLaunchApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class ClinicalCaseRepository
    {
        private readonly ApplicationDbContext context;
        public ClinicalCaseRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<ClinicalCase> CreateClinicalCaseAsync(string userId, string title, string caseDetails)
        {
            var clinicalCase = new ClinicalCase
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                CaseDetails = caseDetails,
                CreatedOn = DateTime.UtcNow
            };
            context.ClinicalCases.Add(clinicalCase);
            await context.SaveChangesAsync();
            return clinicalCase;
        }

        public async Task<ClinicalCase?> UpdateClinicalCaseAsync(string id, string userId, string title, string caseDetails)
        {
            var clinicalCase = await context.ClinicalCases
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (clinicalCase == null)
                return null;

            clinicalCase.Title = title;
            clinicalCase.CaseDetails = caseDetails;
            await context.SaveChangesAsync();
            return clinicalCase;
        }

        public async Task<List<ClinicalCase>> GetUserClinicalCasesAsync(string userId)
        {
            return await context.ClinicalCases
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();
        }

        public async Task<ClinicalCase?> GetClinicalCaseByIdAsync(string id, string userId)
        {
            return await context.ClinicalCases
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<bool> DeleteClinicalCaseAsync(string id, string userId)
        {
            var clinicalCase = await context.ClinicalCases
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (clinicalCase == null)
                return false;

            context.ClinicalCases.Remove(clinicalCase);
            await context.SaveChangesAsync();
            return true;
        }
    }
}