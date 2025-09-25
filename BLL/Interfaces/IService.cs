namespace BLL.Interfaces
{
    public interface IService<TEntity, TDto>
    where TEntity : class
    where TDto : class
    {
        Task<IEnumerable<TDto>> GetAllAsync();

        Task<Result<TDto>> CreateAsync(TDto dto);
    }
}