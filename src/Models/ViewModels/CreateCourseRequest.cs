using MedicLaunchApi.Data;

public class CreateCourseRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CourseDate { get; set; }
    public decimal Price { get; set; }
    public string CheckoutLink { get; set; }
    public CourseType CourseType { get; set; }
}

public class UpdateCourseRequest : CreateCourseRequest
{
    public string Id { get; set; }
}

public class CourseResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CourseDate { get; set; }
    public decimal Price { get; set; }
    public string CheckoutLink { get; set; }
    public CourseType CourseType { get; set; }
}
