using BLL.ModelsDto;

namespace BLL.Interfaces
{
    public interface IArticleTagService
    {
        Task<Result<ArticleTagDto>> GetByIdsAsync(int articleId, Guid tagId);

        Task<Result<IEnumerable<ArticleTagDto>>> GetByArticleIdAsync(int articleId);

        Task<Result<IEnumerable<ArticleTagDto>>> GetByTagIdAsync(Guid tagId);

        Task<Result<ArticleTagDto>> CreateAsync(ArticleTagCreateDto dto);

        Task<Result<bool>> DeleteAsync(int articleId, Guid tagId);

        Task<Result<bool>> DeleteAllForArticleAsync(int articleId);
    }
}