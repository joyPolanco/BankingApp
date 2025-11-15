

using AutoMapper;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.ViewModels.TransaccionExpres;
using BankingApp.Core.Application.ViewModels.TransactionToCreditCard;
using BankingApp.Core.Application.ViewModels.TransactionToLoan;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class CreateTransactionDtoAndViewModelsMappingprofile : Profile
    {



        public CreateTransactionDtoAndViewModelsMappingprofile()
        {




            CreateMap<CreateTransactionDto, CreateTransactionExpressViewModel>()
                 .ReverseMap()
                 .ForMember(s => s.Status, opt => opt.Ignore())
                 .ForMember(s => s.DateTime, opt => opt.Ignore())
                 .ForMember(s => s.AccountId, opt => opt.Ignore())
                 .ForMember(s => s.Description, opt => opt.Ignore())
                 .ForMember(s => s.Type, opt => opt.Ignore())
                 .ForMember(s => s.AccountNumber, opt => opt.Ignore());


            CreateMap<ValidateAccountNumberResponseDto, DataBeneficiaryExpressViewModel>()
                .ForMember(s => s.IdBeneficiary, opt => opt.MapFrom(src => src.BeneficiaryId));


            CreateMap<ValidateAccountNumberResponseDto, BeneficiaryToTransactionViewModel>()
                .ReverseMap();
          
            


            CreateMap<CreateTransactionDto, CreateTransactionToCreditCardViewModel>()
               .ReverseMap()
               .ForMember(s => s.Origin, opt => opt.MapFrom(src => src.Account))
               .ForMember(s => s.Amount, opt => opt.MapFrom(src => src.Amount))
               .ForMember(s => s.Beneficiary, opt => opt.MapFrom(src => src.CreditCard))
               .ForMember(s => s.Status, opt => opt.Ignore())
               .ForMember(s => s.DateTime, opt => opt.Ignore())
               .ForMember(s => s.AccountId, opt => opt.Ignore())
               .ForMember(s => s.Description, opt => opt.Ignore())
               .ForMember(s => s.Type, opt => opt.Ignore())
               .ForMember(s => s.AccountNumber, opt => opt.Ignore());




            CreateMap<CreateTransactionDto, CreateTransactionToLoanViewModal>()
               .ReverseMap()
               .ForMember(s => s.Origin, opt => opt.MapFrom(src => src.Cuenta))
               .ForMember(s => s.Amount, opt => opt.MapFrom(src => src.Amount))
               .ForMember(s => s.Beneficiary, opt => opt.MapFrom(src => src.PublicId))
               .ForMember(s => s.Status, opt => opt.Ignore())
               .ForMember(s => s.DateTime, opt => opt.Ignore())
               .ForMember(s => s.AccountId, opt => opt.Ignore())
               .ForMember(s => s.Description, opt => opt.Ignore())
               .ForMember(s => s.Type, opt => opt.Ignore())
               .ForMember(s => s.AccountNumber, opt => opt.Ignore());







            CreateMap<CreateTransactionDto, CreateTransactionToBeneficiaryViewModel>()
               .ReverseMap()
               .ForMember(s => s.Origin, opt => opt.MapFrom(src => src.Origen))
               .ForMember(s => s.Amount, opt => opt.MapFrom(src => src.Amount))
               .ForMember(s => s.Beneficiary, opt => opt.MapFrom(src => src.Beneficiary))
               .ForMember(s => s.Status, opt => opt.Ignore())
               .ForMember(s => s.DateTime, opt => opt.Ignore())
               .ForMember(s => s.AccountId, opt => opt.Ignore())
               .ForMember(s => s.Description, opt => opt.Ignore())
               .ForMember(s => s.Type, opt => opt.Ignore())
               .ForMember(s => s.AccountNumber, opt => opt.Ignore());





            
        }


    }
}
