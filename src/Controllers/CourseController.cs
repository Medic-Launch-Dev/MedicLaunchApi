using MedicLaunchApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/courses")]
[ApiController]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly CoursesRepository coursesRepository;
    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    public CourseController(CoursesRepository coursesRepository)
    {
        this.coursesRepository = coursesRepository;
    }

    [HttpPost("create")]
    [Authorize(Policy = RoleConstants.Admin)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        await coursesRepository.CreateCourse(request, CurrentUserId);
        return Ok();
    }

    [HttpPut("update")]
    [Authorize(Policy = RoleConstants.Admin)]
    public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseRequest request)
    {
        await coursesRepository.UpdateCourse(request, CurrentUserId);
        return Ok();
    }

    [HttpDelete("delete/{courseId}")]
    [Authorize(Policy = RoleConstants.Admin)]
    public async Task<IActionResult> DeleteCourse(string courseId)
    {
        await coursesRepository.DeleteCourse(courseId);
        return Ok();
    }

    [HttpGet("list")]
    [AllowAnonymous]
    public async Task<IActionResult> ListCourses()
    {
        var courses = await coursesRepository.ListCourses();
        return Ok(courses);
    }

    // TODO: Link courses purchased operation with the stripe checkout

    //[HttpPost("purchase/{courseId}")]
    //public async Task<IActionResult> PurchaseCourse(string courseId)
    //{
    //    await coursesRepository.PurchaseCourse(CurrentUserId, courseId);
    //    return Ok();
    //}
}
