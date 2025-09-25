using BLL.ModelsDto;
using DAL.Entities;
using System.Security.Claims;

namespace BLL.Interfaces
{
    public interface ITagService
    {
        Task<Result<bool>> CreateAsync(TagDto tagDto, ClaimsPrincipal user);

        Task<Result<IEnumerable<TagDto>>> FindByNameAsync(string? name = null);

        Task<Result<TagDto>> FindByIdAsync(Guid id);

        Task<Result<TagDto>> UpdateAsync(TagDto updDto);

        Task<Result<bool>> DeleteAsync(Guid id);

        internal Task<Tag?> FirstOrDefaultEntityAsync(string name);

        Task<IEnumerable<Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames);
    }
}