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
        public ActionResult<List<Member>> Get()
        {
            return _memberService.GetAll();
        }

        [HttpGet("{id:int}")]
        public ActionResult<Member> GetById(int id)
        {
            var member = _memberService.GetById(id);
            if (member == null) return NotFound();
            return member;
        }

        [HttpPost]
        public ActionResult<Member> Create(Member member)
        {
            _memberService.AddMember(member);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
        }

        [HttpPut("{id}")]
        public ActionResult<Member> Update(int id, Member updatedMember)
        {
            bool isUpdated = _memberService.Update(id, updatedMember);
            if (!isUpdated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            bool isDeleted = _memberService.Delete(id);
            if(!isDeleted) return NotFound();
            return NoContent();
        }
    }
}
