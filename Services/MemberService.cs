using LibraryApi.Models;

namespace LibraryApi.Services
{
    public class MemberService
    {
        private readonly List<Member> _members = new()
        {
            new Member
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john.doe@example.com"
            },
            new Member
            {
                Id = 2,
                FullName = "Jane Smith",
                Email = "jane.smith@example.com"
            }
        };

        public List<Member> GetAll() { return _members; }

        public Member? GetById(int id)
        {
            foreach (Member member in _members)
            {
                if (member.Id == id) return member;
            }
            return null;
        }

        public void AddMember(Member member)
        {
            member.Id = GetNextId();
            _members.Add(member);
        }

        private int GetNextId()
        {
            int highestId = 0;

            foreach (Member member in _members)
            {
                if (member.Id > highestId) highestId = member.Id;
            }

            return highestId + 1;
        }

        public bool Update(int id, Member updatedMember)
        {
            foreach (Member member in _members)
            {
                if (member.Id == id)
                {
                    member.FullName = updatedMember.FullName;
                    member.Email = updatedMember.Email;

                    return true;
                }
            }

            return false;
        }

        public bool Delete(int id)
        {
            Member? memberToDelete = null;
            foreach (Member member in _members)
            {
                if (member.Id == id)
                {
                    memberToDelete = member;
                    break;
                }
            }

            if (memberToDelete is null) return false;
           
            _members.Remove(memberToDelete);

            return true;
        }
    }
}
