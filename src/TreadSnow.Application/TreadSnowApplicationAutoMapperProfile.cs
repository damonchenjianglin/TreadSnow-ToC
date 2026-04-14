using AutoMapper;
using System.Collections.Generic;
using TreadSnow.Accounts;
using TreadSnow.Authors;
using TreadSnow.Books;
using TreadSnow.Pets;
using TreadSnow.UploadFiles;

namespace TreadSnow;

public class TreadSnowApplicationAutoMapperProfile : Profile
{
    public TreadSnowApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Author, AuthorDto>();

        CreateMap<Author, AuthorLookupDto>();

        //�û�
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
