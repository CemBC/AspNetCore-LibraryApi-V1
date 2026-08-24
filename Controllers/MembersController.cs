using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly MemberService _memberService;

    private readonly IValidator<CreateMemberRequest> _createMemberValidator;
    private readonly IValidator<UpdateMemberRequest> _updateMemberValidator;

    private readonly IMapper _mapper;


    public MembersController(
        MemberService memberService,
        IValidator<CreateMemberRequest> createMemberValidator,
        IValidator<UpdateMemberRequest> updateMemberValidator,
        IMapper mapper)
    {
        _memberService = memberService;
        _createMemberValidator = createMemberValidator;
        _updateMemberValidator = updateMemberValidator;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberResponse>>> GetAll()
    {
        List<Member> members =
            await _memberService.GetAllAsync();


        List<MemberResponse> response =
            _mapper.Map<List<MemberResponse>>(members);


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
            return BadRequest(validationResult.Errors);


        Member member =
            await _memberService.CreateAsync(request);


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
            return BadRequest(validationResult.Errors);


        await _memberService.UpdateAsync(id, request);


        return NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _memberService.DeleteAsync(id);


        return NoContent();
    }
}