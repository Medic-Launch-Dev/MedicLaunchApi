using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicLaunchApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<MedicLaunchApi.Models.MedicLaunchUser>
    {
        public DbSet<Question> Questions { get; set; }

        public DbSet<AnswerOption> AnswerOptions { get; set; }

        public DbSet<Speciality> Specialities { get; set; }

        public DbSet<QuestionAttempt> QuestionAttempts { get; set; }

        public DbSet<FlaggedQuestion> FlaggedQuestions { get; set; }

        public DbSet<Note> Notes { get; set; }

        public DbSet<Flashcard> Flashcards { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        private readonly ApplicationDbContext context;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Question>()
                .Property(e => e.QuestionType)
                .HasConversion<string>();

            builder.Entity<Question>()
                .Property(e => e.QuestionState)
                .HasConversion<string>();

            base.OnModelCreating(builder);
        }
    }

    public class Audit
    {
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdatedOn { get; set; }
    }

    [Table("Question")]
    public class Question : Audit
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public QuestionType QuestionType { get; set; }

        public string QuestionText { get; set; }

        public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string LearningPoints { get; set; }

        public QuestionState QuestionState { get; set; }

        public string SpecialityId { get; set; }

        public Speciality Speciality { get; set; }
    }

    public enum QuestionType
    {
        General,
        PaperOneMockExam,
        PaperTwoMockExam
    }

    public enum QuestionState
    {
        Draft,
        Submitted
    }

    [Table("AnswerOption")]
    public class AnswerOption
    {
        public string Id { get; set; }
        public string Letter { get; set; }
        public string Text { get; set; }

        public string QuestionId { get; set; }
        public Question Question { get; set; }
    }

    [Table("Speciality")]
    public class Speciality
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    [Table("QuestionAttempt")]
    public class QuestionAttempt
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string QuestionId { get; set; }

        public string ChosenAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Question Question { get; set; }
    }

    [Table("FlaggedQuestion")]
    public class FlaggedQuestion
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string QuestionId { get; set; }

        public Question Question { get; set; }

        public DateTime CreatedOn { get; set; }
    }


    // A note could be associated with a speciality, a question or a flashcard
    [Table("Note")]
    public class Note
    {
        public string Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public string? SpecialityId { get; set; }

        public string? QuestionId { get; set; }

        public string? FlashcardId { get; set; }

        public Speciality? Speciality { get; set; }

        public Question? Question { get; set; }

        public Flashcard? Flashcard { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }

    [Table("Flashcard")]
    public class Flashcard
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string SpecialityId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Speciality Speciality { get; set; }
    }

    [Table("UserNotification")]
    public class UserNotification
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ReadOn { get; set;}
    }
}
