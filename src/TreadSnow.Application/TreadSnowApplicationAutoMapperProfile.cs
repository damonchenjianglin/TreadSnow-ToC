using AutoMapper;
using System.Collections.Generic;
using TreadSnow.Accounts;
using TreadSnow.Pets;
using TreadSnow.UploadFiles;

namespace TreadSnow;

public class TreadSnowApplicationAutoMapperProfile : Profile
{
    public TreadSnowApplicationAutoMapperProfile()
    {
        //会员
        CreateMap<Account, AccountDto>();
        CreateMap<CreateAccountDto, Account>();
        CreateMap<UpdateAccountDto, Account>();

        // 宠物
        CreateMap<Pet, PetDto>();
        CreateMap<CreatePetDto, Pet>();
        CreateMap<UpdatePetDto, Pet>();
        CreateMap<Account, AccountLookupDto>();

        // ����
        CreateMap<UploadFile, UploadFileDto>();
        CreateMap<CreateUploadFileDto, UploadFile>();
        CreateMap<UpdateUploadFileDto, UploadFile>();
    }
}
