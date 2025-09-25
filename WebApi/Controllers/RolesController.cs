using AutoMapper;
using BLL.Interfaces;
using BLL.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class RolesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IMapper mapper, IRoleService roleService, ILogger<RolesController> logger)
        {
            _mapper = mapper;
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllAsync();
                var viewModels = _mapper.Map<IEnumerable<RoleViewModel>>(roles);
                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех ролей.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _roleService.GetByIdAsync(id);
            if (result.Data == null)
                return NotFound();

            var viewModel = _mapper.Map<RoleViewModel>(result.Data);
            return Ok(viewModel);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByNames(string name)
        {
            var result = await _roleService.GetByNamesAsync(name);
            if (!result.Success)
                return NotFound(result.Errors);

            var viewModel = _mapper.Map<IEnumerable<RoleViewModel>>(result.Data);
            return Ok(viewModel);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] RegisterRoleModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var dto = _mapper.Map<RoleDto>(model);
                var result = await _roleService.Create(dto.Name);

                if (!result.Success)
                    return BadRequest(result.Errors);

                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании роли.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = _mapper.Map<RoleDto>(model);
            var result = await _roleService.UpdateAsync(dto);

            if (!result.Success)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _roleService.Delete(id);

            if (!result.Success)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}