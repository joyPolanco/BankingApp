using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Interfaces;


namespace BankingApp.Core.Application.Services
{
    public class GenericService<Entity, EntityDto> : IGenericService<Entity, EntityDto> where Entity : class where EntityDto : class
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Entity> _repo;

        public GenericService(IGenericRepository<Entity> repo, IMapper mapper){
            _mapper = mapper;
            _repo = repo;
        }



        public virtual async Task<EntityDto?> AddAsync(EntityDto entityDto)
        {
            try
            {

                var entity = _mapper.Map<Entity>(entityDto);
                await _repo.AddAsync(entity);
                var dto = _mapper.Map<Entity, EntityDto>(entity);
                return dto;
            }
            catch
            {
                return default;
            }
        }


        public async Task DeleteAsync(int id)
        {
            try
            {

                await _repo.DeleteAsync(id);
            }
            catch
            {
            }
        }




        public async Task<EntityDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);

            var dto = _mapper.Map<EntityDto>(entity);
            return dto;
        }




        public async Task DeleteRangeAsync(List<EntityDto> entityDtos)
        {
            try
            {
                var entities = _mapper.Map<List<Entity>>(entityDtos);
              await  _repo.DeleteRangeAsync(entities);
            }

            catch
            {

            }
        }



        public async Task<List<EntityDto>?> GetAllList()
        {
            var list = await _repo.GetAllList() ?? [];
            var dtos = _mapper.Map<List<EntityDto>>(list);
            return dtos;
        }

        public async Task<List<EntityDto>?> GetAllListWithInclude(List<string> properties)
        {
            var list = await _repo.GetAllListWithInclude(properties) ?? [];
            var dtos = _mapper.Map<List<EntityDto>>(list);
            return dtos;
        }

        

        public async Task<EntityDto?> UpdateAsync(int id, EntityDto entityDto)
        {
            var entity = _mapper.Map<Entity>(entityDto);

            var entry = await _repo.UpdateAsync(id, entity);

            var dto = _mapper.Map<EntityDto>(entry);
            return dto;
        }



        public async  Task  UpdateRangeAsync(List<EntityDto> entityDtos)
        {
            var entities = _mapper.Map<List<Entity>>(entityDtos);
            await _repo.UpdateRangeAsync(entities);
        }
    }
}
