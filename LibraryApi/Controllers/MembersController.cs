using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LibraryApi.Services.Interfaces;
namespace LibraryApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private readonly IMemberService _memberService;
    private readonly IValidator<CreateMemberRequest> _createMemberValidator;
    private readonly IValidator<UpdateMemberRequest> _updateMemberValidator;
    private readonly IValidator<MemberQuery> _memberQueryValidator;
    private readonly IMapper _mapper;

    public MembersController(
       IMemberService memberService,
        IValidator<CreateMemberRequest> createMemberValidator,
        IValidator<UpdateMemberRequest> updateMemberValidator,
        IMapper mapper,
        IValidator<MemberQuery> memberQueryValidator)
    {
        _memberService = memberService;
        _createMemberValidator = createMemberValidator;
        _updateMemberValidator = updateMemberValidator;
        _mapper = mapper;
        _memberQueryValidator = memberQueryValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<MemberResponse>>> GetAll(
        [FromQuery] MemberQuery query)
    {
        var validationResult =
            await _memberQueryValidator.ValidateAsync(query);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var response =
            await _memberService.GetAllAsync(query);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberResponse>> GetById(int id)
    {
        Member member =
            await _memberService.GetByIdAsync(id);

        MemberResponse response =
            _mapper.Map<MemberResponse>(member);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(
        CreateMemberRequest request)
    {
        var validationResult =
            await _createMemberValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        Member member =
            await _memberService.CreateAsync(request, CurrentUserId);

        MemberResponse response =
            _mapper.Map<MemberResponse>(member);

        return CreatedAtAction(
            nameof(GetById),
            new { id = member.Id },
            response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateMemberRequest request)
    {
        var validationResult =
            await _updateMemberValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        await _memberService.UpdateAsync(
            id,
            request,
            CurrentUserId);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _memberService.DeleteAsync(
            id,
            CurrentUserId);

        return NoContent();
    }

    [HttpGet("{id:int}/loans")]
    public async Task<ActionResult<List<LoanResponse>>> GetMemberLoans(int id)
    {
        var response =
            await _memberService.GetMemberLoansAsync(id);

        return Ok(response);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        await _memberService.ResetPasswordAsync(
            id,
            CurrentUserId);

        return NoContent();
    }
}