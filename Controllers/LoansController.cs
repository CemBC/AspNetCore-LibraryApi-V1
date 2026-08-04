using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;


namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoanService _loanService;

        public LoansController(LoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public ActionResult<List<Loan>> Get()
        {
            return _loanService.GetAll();
        }

        [HttpGet("{id:int}")]
        public ActionResult<Loan> GetById(int id)
        {
            var loan = _loanService.GetById(id);
            if (loan == null) return NotFound();
            
            return loan;
        }

        [HttpPost]
        public ActionResult<Loan> CreateLoan(Loan loanRequest)
        {
            LoanOperationStatus status = _loanService.CreateLoan(
                                                loanRequest.BookId,
                                                loanRequest.MemberId,
                                                out Loan? createdLoan );
            if (status == LoanOperationStatus.BookNotFound) return NotFound("Book Not Found");
            

            if (status == LoanOperationStatus.MemberNotFound) return NotFound("Member Not Found");
            

            if (status == LoanOperationStatus.BookUnavailable) return BadRequest("This book is currently unavailable.");
            

            if (createdLoan is null) return StatusCode(500);
            

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdLoan.Id },
                createdLoan
            );
        }

        [HttpPut("{id:int}/return")]
        public ActionResult ReturnBook(int id)
        {
            LoanOperationStatus status = _loanService.ReturnBook(id);

            if (status == LoanOperationStatus.LoanNotFound) return NotFound("Loan not found");
            

            if (status == LoanOperationStatus.AlreadyReturned) return BadRequest("This book has already been returned.");
            

            if (status == LoanOperationStatus.BookNotFound) return NotFound("Book not found");
            

            return NoContent();
        }
    }
}
