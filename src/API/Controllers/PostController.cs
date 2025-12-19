using System.Security.Claims;
using API.Attribute;
using Application.DTOs.Post;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]  
[ApiVersion("1.0")]  
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _service;

    public PostsController(IPostService service)
    {
        _service = service;
    }

    // GET: api/posts
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PostQueryParametersDto queryParameters)
    {
        var pagedResult = await _service.GetPagedAsync(queryParameters);
        if (pagedResult.Items == null || !pagedResult.Items.Any())
            return NoContent();
        return Ok(pagedResult);
    }

    // GET: api/posts/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await _service.GetByIdAsync(id);
        if (post == null)
            return NotFound(new { Message = $"Post with id '{id}' not found." });
        return Ok(post);
    }

    // POST: api/posts
    [HttpPost]
    [AuthorizeRole("Admin", "User")]
    public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreatePostAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/posts/{id}
    [HttpPut("{id}")]
    [AuthorizeRole("Admin", "User")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PostUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var updated = await _service.UpdatePostAsync(id, userId, isAdmin, dto);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    // DELETE: api/posts/{id}
    [HttpDelete("{id}")]
    [AuthorizeRole("Admin", "User")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var deleted = await _service.DeletePostAsync(id, userId, isAdmin);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}