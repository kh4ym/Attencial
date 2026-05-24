using Attencial.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IStudentRepository _studentRepo;

    public TestController(IStudentRepository studentRepo)
    {
        _studentRepo = studentRepo;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _studentRepo.GetAllAsync();
        return Ok(students);
    }
}
