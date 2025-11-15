using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, CreateBeneficiaryDto> , IBeneficiaryService
    {

        private readonly IMapper _mapper;
        private readonly IBeneficiaryRepository repo;
        private readonly IAccountRepository accountRepository;
        private readonly IAccountServiceForWebAPP serviceForWebApi;
        private ISavingsAccountServiceForWebApp serviceBank;


        public BeneficiaryService(IMapper mapper, IBeneficiaryRepository repo, IAccountRepository accountRepository, IAccountServiceForWebAPP serviceForWebApi, ISavingsAccountServiceForWebApp serviceBank) : base(repo,mapper)
        {

            this.repo = repo;
            this.accountRepository = accountRepository; 
            this._mapper = mapper;  
            this.serviceForWebApi = serviceForWebApi;  
            this.serviceBank = serviceBank; 


        }



        public async Task<List<DataBeneficiaryDto>> GetBeneficiaryList(string idUser)
        {


            try
            {
              
                var beneficiaries = await  repo.GetBeneficiariesByIdCliente(idUser);

                var listBeneficiaries = new List<DataBeneficiaryDto>();
                foreach (var item in beneficiaries)
                {

                   var beneficiary = await  serviceForWebApi.GetUserById(item.BeneficiaryId);
                   var account = await serviceBank.GetAccountByClientId(item.BeneficiaryId);

                    if (account == null || beneficiary == null)
                        continue;

                    var entity = _mapper.Map<DataBeneficiaryDto>(beneficiary);
                    entity.Id = item.Id;
                    entity.IdBeneficiary = beneficiary.Id;
                    entity.Number = account.Number;

                    listBeneficiaries.Add(entity);  
                    
                }


                return listBeneficiaries;
            }
            catch (Exception ex)
            {

                return new List<DataBeneficiaryDto>();

            }
        }



        public async Task<ValidateAcountNumberExistResponseDto> ValidateAccountNumberExist(string number)
        {
            var response = new ValidateAcountNumberExistResponseDto();

            try
            {
                var entities = await accountRepository.GetAllList();

                if (entities == null || !entities.Any())
                {
                    response.IsExist = false;
                    return response;
                }

                var account = entities.FirstOrDefault(s => s.Number == number && s.Status == AccountStatus.ACTIVE);

                if (account == null)
                {
                    response.IsExist = false;
                    return response;
                }

                response.IdBeneficiary = account.UserId;

               
                var user = await serviceForWebApi.GetUserById(account.UserId);
                if (user != null)
                {
                    response.NameBeneficiary = $"{user.Name} {user.LastName}";
                }

                response.IsExist = true;
                return response;
            }
            catch (Exception)
            {
                response.IsExist = false;
                return response;
            }
        }






        public async Task<bool> ValidateAccountNumber(string number,string  idCliente)
        {



            try
            {

                var beneficiaries = await repo.GetBeneficiariesByIdCliente(idCliente);

                foreach (var entity in beneficiaries)
                {

                    var exist = await serviceBank.GetAccountByClientId(entity.BeneficiaryId);

                    if(exist != null && exist.Number == number)
                    {

                        return false;
                         
                        
                    }
                }



                return true;


            }
            catch (Exception ex)
            {


                return false;
            
            
            }

        }





    }
}
