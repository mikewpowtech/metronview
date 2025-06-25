using Application.ConfigurationUploads;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationUploadsController : ControllerBase
{
    private readonly IConfigurationUploadService _service;

    public ConfigurationUploadsController(IConfigurationUploadService service)
    {
        _service = service;
    }

    // GET: api/configurationuploads/unit/{unitId}
    [HttpGet("unit/{unitId:int}")]
    public async Task<ActionResult<List<ConfigurationUpload>>> GetByUnitId(int unitId)
    {
        var uploads = await _service.GetByUnitIdAsync(unitId);
        return Ok(uploads);
    }

    // GET: api/configurationuploads/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConfigurationUpload>> GetById(int id)
    {
        var upload = await _service.GetByIdAsync(id);
        if (upload == null)
            return NotFound();
        return Ok(upload);
    }

    // POST: api/configurationuploads
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ConfigurationUpload upload)
    {
        await _service.AddAsync(upload);
        return CreatedAtAction(nameof(GetById), new { id = upload.Id }, upload);
    }
}