using LibraryApi.DTOs.Members;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly MemberService _memberService;


    public MembersController(MemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberResponse>>> GetAll()
    {
        var members = await _memberService.GetAllAsync();


        var response = members.Select(member => new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email

        }).ToList();


        return Ok(response);
    }



    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberResponse>> GetById(int id)
    {
        var member = await _memberService.GetByIdAsync(id);


        if (member == null)
            return NotFound();


        var response = new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email
        };


        return Ok(response);
    }


    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(
        CreateMemberRequest request)
    {
        var member = await _memberService.CreateAsync(request);


        var response = new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email
        };


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
        var updated = await _memberService.UpdateAsync(id, request);


        if (!updated)
            return NotFound();


        return NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _memberService.DeleteAsync(id);


        if (!deleted)
            return NotFound();


        return NoContent();
    }
}