using MedicLaunchApi.Data;
using MedicLaunchApi.Models;
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

        public async Task CreateClinicalCaseAsync(string userId, string title, string caseDetails)
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
    }
}