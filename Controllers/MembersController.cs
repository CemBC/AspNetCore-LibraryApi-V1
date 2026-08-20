using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
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
        public async Task<ActionResult<List<Member>>> Get()
        {
            return await _memberService.GetAll();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Member>>  GetById(int id)
        {
            var member = await _memberService.GetById(id);
            if (member == null) return NotFound();
            return member;
        }

        [HttpPost]
        public async Task<ActionResult<Member>> Create(Member member)
        {
            await _memberService.AddMember(member);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
        }

        [HttpPut("{id}")]
        public async  Task<ActionResult<Member>> Update(int id, Member updatedMember)
        {
            bool isUpdated = await _memberService.Update(id, updatedMember);
            if (!isUpdated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            bool isDeleted = await _memberService.Delete(id);
            if(!isDeleted) return NotFound();
            return NoContent();
        }
    }
}
