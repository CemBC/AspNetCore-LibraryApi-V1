using AutoMapper;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Members;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;

namespace LibraryApi.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookResponse>();

        CreateMap<CreateBookRequest, Book>();

        CreateMap<UpdateBookRequest, Book>();


        CreateMap<Member, MemberResponse>();

        CreateMap<CreateMemberRequest, Member>();

        CreateMap<UpdateMemberRequest, Member>();


        CreateMap<Loan, LoanResponse>();
    }
}