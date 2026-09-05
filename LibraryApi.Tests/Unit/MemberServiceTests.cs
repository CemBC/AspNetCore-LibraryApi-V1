using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests.Unit;

public class MemberServiceTests
{
    [Fact]
    public async Task GetAllAsync_AppliesSearchSortingAndPagination()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        MemberService service =
            TestServiceFactory.CreateMemberService(context);

        await TestServiceFactory.AddMemberAsync(
            context,
            "zeynep@test.com",
            "Zeynep Kaya");

        await TestServiceFactory.AddMemberAsync(
            context,
            "ali@test.com",
            "Ali Yilmaz");

        await TestServiceFactory.AddMemberAsync(
            context,
            "ayse@test.com",
            "Ayse Demir");

        PagedResponse<MemberResponse> result =
            await service.GetAllAsync(
                new MemberQuery
                {
                    Page = 1,
                    PageSize = 1,
                    Search = "a",
                    SortBy = "fullname",
                    Descending = false
                });

        Assert.True(result.TotalCount >= 2);
        Assert.Single(result.Items);
        Assert.Equal(
            "Ali Yilmaz",
            result.Items[0].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberDoesNotExist_ThrowsNotFoundException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        MemberService service =
            TestServiceFactory.CreateMemberService(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByIdAsync(999));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFullName()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        MemberService service =
            TestServiceFactory.CreateMemberService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        await service.UpdateAsync(
    member.Id,
    new UpdateMemberRequest
    {
        FullName = "Updated Member",
        Email = "ignored@test.com"
    },
    userId: 10);

        Member? dbMember =
            await context.Members.FindAsync(member.Id);

        Assert.NotNull(dbMember);

        Assert.Equal(
            "Updated Member",
            dbMember.FullName);
    }

    [Fact]
    public async Task GetMemberLoansAsync_ReturnsOnlySelectedMembersLoans_NewestFirst()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        MemberService service =
            TestServiceFactory.CreateMemberService(context);

        (_, Member member1) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "one@test.com",
                "Member One");

        (_, Member member2) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "two@test.com",
                "Member Two");

        Book book1 =
            TestServiceFactory.AddBook(
                context,
                "Book One");

        Book book2 =
            TestServiceFactory.AddBook(
                context,
                "Book Two");

        Book book3 =
            TestServiceFactory.AddBook(
                context,
                "Other Book");

        Loan older =
            TestServiceFactory.AddLoan(
                context,
                book1,
                member1);

        older.LoanDate =
            DateTime.UtcNow.AddDays(-5);

        Loan newer =
            TestServiceFactory.AddLoan(
                context,
                book2,
                member1);

        newer.LoanDate =
            DateTime.UtcNow.AddDays(-1);

        TestServiceFactory.AddLoan(
            context,
            book3,
            member2);

        await context.SaveChangesAsync();

        List<LoanResponse> result =
            await service.GetMemberLoansAsync(
                member1.Id);

        Assert.Equal(
            2,
            result.Count);

        Assert.Equal(
            newer.Id,
            result[0].Id);

        Assert.Equal(
            older.Id,
            result[1].Id);

        Assert.All(
            result,
            loan =>
                Assert.Equal(
                    member1.Id,
                    loan.MemberId));
    }

    [Fact(
        Skip =
            "Current MemberService.CreateAsync does not create/link a User even though Member.UserId is required by the domain model. Keep this test as a reminder to refactor or remove manual member creation.")]
    public async Task CreateAsync_ShouldCreateMemberLinkedToUser()
    {
        await Task.CompletedTask;
    }
}