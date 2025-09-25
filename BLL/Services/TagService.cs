using AutoMapper;
using BLL.Interfaces;
using BLL.ModelsDto;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BLL.Services
{
    public class TagService : ITagService
    {
        private readonly IRepository<Tag> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TagService> _logger;

        public TagService(IRepository<Tag> repository, IMapper mapper, ILogger<TagService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<bool>> CreateAsync(TagDto tagDto, ClaimsPrincipal user)
        {
            try
            {
                if (string.IsNullOrEmpty(tagDto.Name))
                    return Result<bool>.Fail(400, "No name is set for the tag");

                if (user == null)
                    return Result<bool>.Fail(401, "The ClaimsPrincipal user must not be null");

                var existingTag = await _repository.FirstOrDefaultAsync(n => n.Name == tagDto.Name);

                if (existingTag != null)
                    return Result<bool>.Fail(409, "A tag with this name already exists.");

                var authorid = user.Identity.GetUserId();

                var tagEntity = _mapper.Map<Tag>(tagDto);
                tagEntity.CreatedByUserId = authorid;
                await _repository.AddAsync(tagEntity);
                return Result<bool>.Ok(201, true);
            }
            catch (Exception ex)
            {
                var detailedMessage = ex.InnerException?.Message ?? ex.Message;

                return Result<bool>.Fail(500, detailedMessage);
            }
        }

        public async Task<Result<IEnumerable<TagDto>>> FindByNameAsync(string? name = null)
        {
            try
            {
                var result = await _repository.GetAllAsync(tag => tag.CreatedByUser);
                if (!string.IsNullOrEmpty(name))
                {
                    result = result.Where(n => n.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                }
                var dto = _mapper.Map<IEnumerable<TagDto>>(result);

                return Result<IEnumerable<TagDto>>.Ok(200, dto);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<TagDto>>.Fail(500, ex.Message);
            }
        }

        public async Task<Result<TagDto>> FindByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentException("Id must not be null or empty.", nameof(id));
                }

                var result = await _repository.GetByIdAsync(id);
                var dto = _mapper.Map<TagDto>(result);
                return Result<TagDto>.Ok(200, dto);
            }
            catch (Exception ex)
            {
                return Result<TagDto>.Fail(500, ex.Message);
            }
        }

        public async Task<Result<TagDto>> UpdateAsync(TagDto updDto)
        {
            var id = updDto.Id;

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must not be null or empty.", nameof(id));
            }

            var tag = await _repository.GetByIdAsync(id);
            if (tag == null)
                return Result<TagDto>.Fail(404, "Tag not found");

            // Обновляем только если имя не пустое
            if (!string.IsNullOrEmpty(updDto.Name))
                tag.Name = updDto.Name;

            // Описание можно обновить всегда
            tag.Description = updDto.Description;

            tag.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(tag);

            // Повторно загружаем
            var updatedTag = await _repository.GetByIdAsync(id);

            var tagDto = _mapper.Map<TagDto>(updatedTag);

            return Result<TagDto>.Ok(200, tagDto);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Result<bool>.Fail(401, "Invalid id: Id must not be empty.");
            }

            var tag = await _repository.GetByIdAsync(id);

            if (tag == null)
                return Result<bool>.Fail(404, "Tag not found");

            await _repository.DeleteAsync(tag);

            return Result<bool>.Ok(204, true);
        }

        public async Task<Tag?> FirstOrDefaultEntityAsync(string name) =>
            string.IsNullOrEmpty(name) ? null : await _repository.FirstOrDefaultAsync(n => n.Name == name);

        public async Task<IEnumerable<Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames)
        {
            if (tagNames == null || !tagNames.Any())
                return Enumerable.Empty<Tag>();

            try
            {
                return await _repository.GetQueryable()
                    .Where(t => tagNames.Contains(t.Name))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске тегов по именам: {TagNames}", tagNames);
                throw;
            }
        }
    }
}