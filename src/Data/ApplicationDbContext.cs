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

        public DbSet<MockExam> MockExams { get; set; }

        public DbSet<TrialQuestion> TrialQuestions { get; set; }

        public DbSet<TrialAnswerOption> TrialAnswerOptions { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<CoursePurchase> CoursePurchases { get; set; }

        public DbSet<TextbookLesson> TextbookLessons { get; set; }

        public DbSet<TextbookLessonContent> TextbookLessonContents { get; set; }

        public DbSet<ClinicalCase> ClinicalCases { get; set; }

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

            builder.Entity<TrialQuestion>()
                .Property(e => e.QuestionType)
                .HasConversion<string>();

            builder.Entity<TrialQuestion>()
                .Property(e => e.QuestionState)
                .HasConversion<string>();

            builder.Entity<MockExam>()
                .Property(e => e.MockExamType)
                .HasConversion<string>();

            builder.Entity<Course>()
                .Property(course => course.CourseType)
                .HasConversion<string>();

            builder.Entity<Course>()
               .Property(course => course.Price)
               .HasPrecision(18, 2); // This is a common precision for money values

            builder.Entity<CoursePurchase>()
               .Property(course => course.PurchasePrice)
               .HasPrecision(18, 2);

            // Add unique index on QuestionId for TextbookLesson
            builder.Entity<TextbookLesson>()
                .HasIndex(t => t.QuestionId)
                .IsUnique()
                .HasFilter("[QuestionId] IS NOT NULL");  // SQL Server syntax for filtered index

            // Also ensure one-to-one relationship between Question and TextbookLesson
            builder.Entity<TextbookLesson>()
                .HasOne(t => t.Question)
                .WithOne()
                .HasForeignKey<TextbookLesson>(t => t.QuestionId)
                .IsRequired(false);

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

        public string? VideoUrl { get; set; }
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

        public string Title { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ReadOn { get; set; }
    }

    [Table("MockExam")]
    public class MockExam
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public MockExamType MockExamType { get; set; }

        public int TotalQuestions { get; set; }

        public int QuestionsCompleted { get; set; }

        public DateTime StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }
    }

    [Table("TrialQuestion")]
    public class TrialQuestion : Audit
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public QuestionType QuestionType { get; set; }

        public string QuestionText { get; set; }

        public ICollection<TrialAnswerOption> Options { get; set; } = new List<TrialAnswerOption>();

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string LearningPoints { get; set; }

        public QuestionState QuestionState { get; set; }

        public string SpecialityId { get; set; }

        public Speciality Speciality { get; set; }

        public string? VideoUrl { get; set; }
    }

    [Table("TrialAnswerOption")]
    public class TrialAnswerOption
    {
        public string Id { get; set; }
        public string Letter { get; set; }
        public string Text { get; set; }

        public string TrialQuestionId { get; set; }
        public TrialQuestion Question { get; set; }
    }

    public enum MockExamType
    {
        PaperOneMockExam,
        PaperTwoMockExam
    }

    // Add this enum inside the namespace MedicLaunchApi.Data
    public enum CourseType
    {
        LiveCourse,
        Webinar,
        OnDemand
    }

    // Add this class inside the same namespace
    [Table("Course")]
    public class Course : Audit
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CourseDate { get; set; }
        public decimal Price { get; set; }
        public string CheckoutLink { get; set; }
        public CourseType CourseType { get; set; } = CourseType.LiveCourse;
    }

    [Table("CoursePurchase")]
    public class CoursePurchase
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }

        // Navigation properties
        public Course Course { get; set; }
        // Assuming a User entity exists, otherwise omit or adjust
        // public User User { get; set; }
    }

    [Table("TextbookLesson")]
    public class TextbookLesson : Audit
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string SpecialityId { get; set; }

        public Speciality Speciality { get; set; }

        public string? QuestionId { get; set; }

        public Question? Question { get; set; }

        public ICollection<TextbookLessonContent> Contents { get; set; } = new List<TextbookLessonContent>();

        public bool IsSubmitted { get; set; }
    }

    [Table("TextbookLessonContent")]
    public class TextbookLessonContent
    {
        public string Id { get; set; }

        public string TextbookLessonId { get; set; }
        public TextbookLesson TextbookLesson { get; set; }

        public string Heading { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
    }

    [Table("ClinicalCases")]
    public class ClinicalCase
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string CaseDetails { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}
