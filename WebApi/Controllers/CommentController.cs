using AutoMapper;
using BLL;
using BLL.Interfaces;
using BLL.ModelsDto;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.ViewModels.Comments;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public CommentController(ICommentService commentService, IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;
        }

        [HttpPost("Example")]
        public IActionResult TestResponse([FromBody] GetCommentViewModel model)
        {
            var cmnt = new Comment()
            {
                Id = new Guid(),
                Message = "Тестовый ответ",
                Author = "Иван Иванов",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            var vm = new CommentViewModel() { ArticleId = model.ArticleId, Comments = [cmnt] };
            return StatusCode(200, vm);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User?.Identity?.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User is not authenticated.");

            var dto = new CommentDto
            {
                ArticleId = model.ArticleId,
                Message = model.Message,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _commentService.CreateAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, string.Join("\r\n", result.Errors));

            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] GetCommentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _commentService.GetAsync(model.ArticleId, model.Count);

            if (!result.Success)
                return StatusCode(result.StatusCode, string.Join("\r\n", result.Errors));

            if (result.DataIsNull)
                return NoContent();

            var artId = result.Data!.FirstOrDefault()?.ArticleId ?? model.ArticleId;

            var comments = new CommentViewModel
            {
                ArticleId = artId,
                Comments = _mapper.Map<List<ViewModels.Comments.Comment>>(result.Data)
            };

            return StatusCode(result.StatusCode, comments);
        }

        [HttpPost("GetById")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _commentService.GetByIdAsync(id);

            if (!result.Success)
                return StatusCode(result.StatusCode, string.Join("\r\n", result.Errors));
            var comments = new CommentViewModel()
            {
                ArticleId = result.Data.ArticleId,
                Comments = new List<ViewModels.Comments.Comment>
                {
                    _mapper.Map<ViewModels.Comments.Comment>(result.Data)
                }
            };
            return StatusCode(result.StatusCode, comments);
        }

        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromBody] EditCommentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.CommentId == default)
                return BadRequest("Id is required");

            var userId = User?.Identity?.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var dto = new CommentDto
            {
                Id = model.CommentId,
                Message = model.Message,
                AuthorId = userId,
                UpdatedAt = DateTime.UtcNow
            };

            var isPermissionEdit = User.IsInRole("Administrator") || User.IsInRole("Moderator");
            var result = await _commentService.UpdateAsync(dto, isPermissionEdit);

            if (!result.Success)
                return StatusCode(result.StatusCode, string.Join("\n", result.Errors));

            return Ok(result);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == default)
                return BadRequest("Id is required.");

            var userId = User?.Identity?.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            // Проверка, админ ли пользователь
            var isAdmin = User.IsInRole("Administrator");

            var result = await _commentService.DeleteAsync(id, userId, isAdmin);

            if (!result.Success)
                return StatusCode(result.StatusCode, string.Join("\n", result.Errors));

            return Ok("Комментарий удалён.");
        }
    }
}