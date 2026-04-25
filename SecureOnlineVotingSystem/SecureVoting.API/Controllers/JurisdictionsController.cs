using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;

[ApiController]
[Route("api/jurisdictions")]
public class JurisdictionsController : ControllerBase
{
    private readonly JurisdictionRepository _repo;

    public JurisdictionsController(JurisdictionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var data = _repo.GetAll();
        return Ok(data);
    }
}