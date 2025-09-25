using AutoMapper;
using BLL.Interfaces;
using BLL.ModelsDto;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ArticleTagService : IArticleTagService
    {
        private readonly IRepository<ArticleTags> _repository;
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ArticleTagService> _logger;

        public ArticleTagService(
            IRepository<ArticleTags> repository,
            IRepository<Article> articleRepository,
            IRepository<Tag> tagRepository,
            IMapper mapper,
            ILogger<ArticleTagService> logger)
        {
            _repository = repository;
            _articleRepository = articleRepository;
            _tagRepository = tagRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ArticleTagDto>> GetByIdsAsync(int articleId, Guid tagId)
        {
            try
            {
                var entity = await _repository.GetQueryable()
                    .Include(at => at.Article)
                    .Include(at => at.Tag)
                    .FirstOrDefaultAsync(at => at.ArticleId == articleId && at.TagId == tagId);

                if (entity == null)
                    return Result<ArticleTagDto>.Fail(404, "Связь не найдена");

                return Result<ArticleTagDto>.Ok(200, _mapper.Map<ArticleTagDto>(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении связи Article-Tag");
                return Result<ArticleTagDto>.Fail(500, "Internal server error");
            }
        }

        public async Task<Result<IEnumerable<ArticleTagDto>>> GetByArticleIdAsync(int articleId)
        {
            try
            {
                var entities = await _repository.GetQueryable()
                    .Include(at => at.Tag)
                    .Where(at => at.ArticleId == articleId)
                    .ToListAsync();

                return Result<IEnumerable<ArticleTagDto>>.Ok(
                    200,
                    _mapper.Map<IEnumerable<ArticleTagDto>>(entities));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тегов статьи");
                return Result<IEnumerable<ArticleTagDto>>.Fail(500, "Internal server error");
            }
        }

        public async Task<Result<ArticleTagDto>> CreateAsync(ArticleTagCreateDto dto)
        {
            try
            {
                // Проверка существования статьи и тега через GetByIdAsync
                var article = await _articleRepository.GetByIdAsync(dto.ArticleId);
                var tag = await _tagRepository.GetByIdAsync(dto.TagId);

                if (article == null || tag == null)
                    return Result<ArticleTagDto>.Fail(404, "Статья или тег не найдены");

                // Проверка существования связи через GetQueryable
                var exists = await _repository.GetQueryable()
                    .AnyAsync(at => at.ArticleId == dto.ArticleId && at.TagId == dto.TagId);

                if (exists)
                    return Result<ArticleTagDto>.Fail(400, "Связь уже существует");

                // Создание новой связи
                var entity = new ArticleTags
                {
                    ArticleId = dto.ArticleId,
                    TagId = dto.TagId,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(entity);

                // Получение созданной связи с включением связанных данных
                var createdEntity = await _repository.GetQueryable()
                    .Include(at => at.Article)
                    .Include(at => at.Tag)
                    .FirstOrDefaultAsync(at => at.ArticleId == dto.ArticleId && at.TagId == dto.TagId);

                // Ручное маппинг, если AutoMapper не используется
                var resultDto = new ArticleTagDto
                {
                    ArticleId = createdEntity.ArticleId,
                    TagId = createdEntity.TagId,
                    CreatedAt = createdEntity.CreatedAt,
                    Article = new ArticleShortDto
                    {
                        Id = createdEntity.Article.Id,
                        Title = createdEntity.Article.Title
                    },
                    Tag = new TagDto
                    {
                        Id = createdEntity.Tag.Id,
                        Name = createdEntity.Tag.Name
                    }
                };

                return Result<ArticleTagDto>.Ok(201, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании связи Article-Tag. DTO: {@Dto}", dto);
                return Result<ArticleTagDto>.Fail(500, "Не удалось создать связь");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int articleId, Guid tagId)
        {
            try
            {
                var entity = await _repository.FirstOrDefaultAsync(
                    at => at.ArticleId == articleId && at.TagId == tagId);

                if (entity == null)
                    return Result<bool>.Fail(404, "Связь не найдена");

                await _repository.DeleteAsync(entity);
                return Result<bool>.Ok(204, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении связи Article-Tag");
                return Result<bool>.Fail(500, "Internal server error");
            }
        }

        public async Task<Result<int>> GetTagUsageCountAsync(Guid tagId)
        {
            try
            {
                var count = await _repository.GetQueryable()
                    .CountAsync(at => at.TagId == tagId);

                return Result<int>.Ok(200, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подсчете использования тега");
                return Result<int>.Fail(500, "Internal server error");
            }
        }

        public Task<Result<IEnumerable<ArticleTagDto>>> GetByTagIdAsync(Guid tagId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool>> DeleteAllForArticleAsync(int articleId)
        {
            throw new NotImplementedException();
        }
    }
}