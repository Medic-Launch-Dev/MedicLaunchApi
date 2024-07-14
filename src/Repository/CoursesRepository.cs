using MedicLaunchApi.Data;
using Microsoft.EntityFrameworkCore;
public class CoursesRepository
{
    private readonly ApplicationDbContext _context;

    public CoursesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateCourse(CreateCourseRequest request, string userId)
    {
        var course = new Course
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            CourseDate = request.CourseDate,
            Price = request.Price,
            CheckoutLink = request.CheckoutLink,
            CourseType = request.CourseType,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
            UpdatedOn = DateTime.UtcNow
        };

        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCourse(UpdateCourseRequest request, string userId)
    {
        var course = await _context.Courses.FindAsync(request.Id);
        if (course == null) throw new InvalidOperationException("Course not found");

        course.Title = request.Title;
        course.Description = request.Description;
        course.CourseDate = request.CourseDate;
        course.Price = request.Price;
        course.CheckoutLink = request.CheckoutLink;
        course.CourseType = request.CourseType;
        course.UpdatedOn = DateTime.UtcNow;
        course.UpdatedBy = userId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourse(string courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) throw new InvalidOperationException("Course not found");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CourseResponse>> ListCourses()
    {
        return await _context.Courses
            .Select(c => new CourseResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CourseDate = c.CourseDate,
                Price = c.Price,
                CheckoutLink = c.CheckoutLink,
                CourseType = c.CourseType
            }).ToListAsync();
    }

    public async Task PurchaseCourse(string userId, string courseId)
    {
        // Validate input
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentException("User ID and Course ID must be provided");
        }

        // Retrieve the course to get the current price
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            throw new InvalidOperationException("Course not found");
        }

        // Prevent user from purchasing the same course multiple times
        var existingPurchase = await _context.CoursePurchases
            .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CourseId == courseId);
        if (existingPurchase != null) {
            throw new InvalidOperationException("User has already purchased this course");
        }

        // Create a new CoursePurchase record
        var coursePurchase = new CoursePurchase
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            CourseId = courseId,
            PurchasePrice = course.Price,
            PurchaseDate = DateTime.UtcNow
        };

        await _context.CoursePurchases.AddAsync(coursePurchase);
        await _context.SaveChangesAsync();
    }

}
