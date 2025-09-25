using AutoMapper;
using BLL.Interfaces;
using BLL.ModelsDto;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.ViewModels.Tags;

namespace WebApi.Controllers
{
    [ApiController, Authorize]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IMapper _mapper;
        private readonly IService<Tag, TagDto> _Service;

        public TagController(ITagService tagService, IMapper mapper, IService<Tag, TagDto> service)
        {
            _tagService = tagService;
            _mapper = mapper;
            _Service = service;
        }

        #region Create Tag

        [HttpPost("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] RegisterTagModel model)
        {
            var dto = _mapper.Map<TagDto>(model);
            var result = await _tagService.CreateAsync(dto, User);
            return StatusCode(result.StatusCode, result);
        }

        #endregion Create Tag

        #region Find Tag

        [HttpGet("by-name/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByName(string? name)
        {
            var result = await _tagService.FindByNameAsync(name);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("by-id/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FindById(Guid id)

        {
            var result = await _tagService.FindByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _Service.GetAllAsync();
            return Ok(result);
        }

        #endregion Find Tag

        #region Update Tag

        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] UpdateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var dto = _mapper.Map<TagDto>(model);
                var result = await _tagService.UpdateAsync(dto);

                return StatusCode(result.StatusCode, result?.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex?.InnerException?.Message);
            }
        }

        #endregion Update Tag

        #region Delete Tag

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _tagService.DeleteAsync(id);

            return StatusCode(result.StatusCode, result);
        }

        #endregion Delete Tag
    }
}