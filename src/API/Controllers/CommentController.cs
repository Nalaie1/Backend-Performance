using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;

        public CommentController(ICommentRepository commentRepo)
        {
            _commentRepo = commentRepo;
        }

        // ================= GET =================

        // Tree structure
        [HttpGet("{postId}/comments/tree")]
        public async Task<IActionResult> GetCommentsTree(Guid postId)
        {
            var comments = await _commentRepo.GetAllCommentsRecursive(postId);
            return Ok(comments);
        }

        // Flat list
        [HttpGet("{postId}/comments/flat")]
        public async Task<IActionResult> GetCommentsFlat(Guid postId)
        {
            var comments = await _commentRepo.GetAllCommentsIterative(postId);
            return Ok(comments);
        }

        // Top-level comments only (Day 8 N+1 demo)
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetTopLevelComments(Guid postId)
        {
            var comments = await _commentRepo.GetAllCommentsForPost(postId, true);
            return Ok(comments);
        }

        // ================= CREATE =================
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] Comment comment)
        {
            comment.PostId = postId;

            // Nếu là reply, ParentCommentId đã được set từ client
            var created = await _commentRepo.CreateAsync(comment);
            return CreatedAtAction(nameof(GetCommentsTree), new { postId }, created);
        }

        // ================= UPDATE =================
        [HttpPut("comments/{id}")]
        public async Task<IActionResult> UpdateComment(Guid id, [FromBody] string newContent)
        {
            var updated = await _commentRepo.UpdateAsync(id, newContent);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // ================= DELETE =================
        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var deleted = await _commentRepo.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
