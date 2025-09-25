namespace BLL.Interfaces
{
    public interface IBaseService<TEntity, TDto>
    where TEntity : class
    where TDto : class
    {
        abstract Task<Result<TDto>> CreateAsync(TDto dto);
    }
}