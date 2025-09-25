using BLL.ModelsDto;

namespace BLL.Interfaces
{
    public interface IArticleService
    {
        Task<Result<ArticleDto>> CreateAsync(ArticleDto dto);

        Task<Result<ArticleDto>> CreateAsync2(ArticleDto dto);

        Task<Result<IEnumerable<ArticleDto>>> FindByTitleAsync(string? title = null);

        Task<Result<IEnumerable<ArticleDto>>> GetLatestArticlesAsync((int startIndex, int count) item);

        Task<ArticleDto> FindByIdAsync(int id);
        Task<Result<IEnumerable<ArticleDto>>> GetByAuthorIdAsync(string authorId);

        Task<Result<bool>> DeleteAsync(int id, string userId);
        Task<Result<ArticleDto>> Update(ArticleDto dto, string editorId);


    }
}