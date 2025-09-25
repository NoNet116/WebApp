using AutoMapper;
using BLL.Interfaces;
using DAL.Interfaces;

namespace BLL.Services
{
    public class Service<TEntity, TDto> : IService<TEntity, TDto>
    where TEntity : class
    where TDto : class
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TEntity> _repository;

        public Service(IMapper mapper, IRepository<TEntity> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public async Task<Result<TDto>> CreateAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            try
            {
                await _repository.AddAsync(entity);
                return Result<TDto>.Ok(200, _mapper.Map<TDto>(entity));
            }
            catch (Exception ex)
            {
                return Result<TDto>.Fail(500, ex.Message);
            }
        }
    }
}