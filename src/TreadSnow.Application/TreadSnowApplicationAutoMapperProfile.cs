using AutoMapper;
using System.Collections.Generic;
using TreadSnow.Accounts;
using TreadSnow.Departments;
using TreadSnow.Lookups;
using TreadSnow.Pets;
using TreadSnow.Teams;
using TreadSnow.UploadFiles;
using Volo.Abp.Identity;

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

        // 附件
        CreateMap<UploadFile, UploadFileDto>();
        CreateMap<CreateUploadFileDto, UploadFile>();
        CreateMap<UpdateUploadFileDto, UploadFile>();

        // 部门
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();

        // 团队
        CreateMap<Team, TeamDto>();
        CreateMap<CreateTeamDto, Team>();
        CreateMap<UpdateTeamDto, Team>();

        // 下拉列表
        CreateMap<IdentityUser, UserLookupDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.UserName));
        CreateMap<Team, TeamLookupDto>();
    }
}
