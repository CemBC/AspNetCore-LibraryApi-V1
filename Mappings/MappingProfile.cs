using AutoMapper;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;

namespace LibraryApi.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<LoanExtensionRequest, LoanExtensionResponse>()
            .ForMember(dest => dest.BookTitle,
                opt => opt.MapFrom(
                src => src.Loan.Book.Title));

        CreateMap<LoanRequest, LoanRequestResponse>()
            .ForMember(dest => dest.BookTitle,
                opt => opt.MapFrom(
                src => src.Book.Title))
            .ForMember(dest => dest.MemberName,
                opt => opt.MapFrom(
                src => src.Member.FullName));

        CreateMap<Book, BookResponse>();

        CreateMap<CreateBookRequest, Book>();

        CreateMap<UpdateBookRequest, Book>();


        CreateMap<Member, MemberResponse>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email));

        CreateMap<CreateMemberRequest, Member>();

        CreateMap<UpdateMemberRequest, Member>();


        CreateMap<Loan, LoanResponse>()
            .ForMember(
                dest => dest.BookTitle,
                opt => opt.MapFrom(
                src => src.Book.Title))
            .ForMember(
                dest => dest.MemberName,
                opt => opt.MapFrom(
                src => src.Member.FullName));
    }
}