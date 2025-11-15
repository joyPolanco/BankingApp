using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace BankingApp.Core.Application.Services
{
    public class CommerceService : GenericService<Commerce, CommerceDto>, ICommerceService
    {
        private readonly ICommerceRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICommerceUserRepository _commerceUserRepository;

        public CommerceService(ICommerceRepository repo, IMapper mapper, ICommerceUserRepository commerceUserRepository, IUnitOfWork unitOfWork) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _commerceUserRepository = commerceUserRepository;

        }

        public async Task<bool>  CommerceAlreadyHasUser(int CommerceId)
        {
           return await _commerceUserRepository.GetAllQuery().AnyAsync(r => r.CommerceId == CommerceId);
        }
        public async Task <OperationResultDto>SetUser (int CommerceId, string UserId)
        {
            var response = new OperationResultDto() { IsSuccessful = true };

            var existing = await _commerceUserRepository.GetAllQuery().Where(r => r.UserId == UserId && r.CommerceId == CommerceId).ToListAsync();

            if (existing != null && existing.Count()>=1)
            {
                response.IsSuccessful = false;

                return response;

   

            }
            try
            {
                await _commerceUserRepository.AddAsync(new CommerceUser { CommerceId = CommerceId, UserId = UserId, Id = 0 });
                return response;
            }
            catch
            {
                response.IsInternalError=true;
                return response;

            }
        }


 

        public async Task<CommercePaginationDto> GetAllActiveFiltered(int page = 1, int pageSize = 20)
        {
            var query = _repo.GetAllQuery()
                             .Where(r => r.IsActive)
                             .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            if (page > 0 && pageSize > 0)
            {
                int skip = (page - 1) * pageSize;
                query = (IOrderedQueryable<Commerce>)query.Skip(skip).Take(pageSize);
            }

            var data = await query.ToListAsync();

            var result = new CommercePaginationDto
            {
                Data = _mapper.Map<List<CommerceDto>>(data),
                TotalCount = totalCount,
                CurrentPage = totalCount == 0 ? 0 : page,
                PagesCount = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return result;
        }



        public async Task<List<string>> GetCommerceAssociates(int commerceId)
        {
           return await _commerceUserRepository.GetAllQuery().Where(r => r.CommerceId == commerceId).Select(r => r.UserId).ToListAsync();
        }
    }
}
