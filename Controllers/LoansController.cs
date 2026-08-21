using LibraryApi.DTOs.Loans;
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
        public async Task<ActionResult<List<LoanResponse>>> GetAll()
        {
            List<Loan> loans = await _loanService.GetAll();

            List<LoanResponse> response = loans
                .Select(l => new LoanResponse
                {
                    Id = l.Id,
                    BookId = l.BookId,
                    MemberId = l.MemberId,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate
                })
                .ToList();

            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LoanResponse>> GetById(int id)
        {
            Loan? loan = await _loanService.GetById(id);

            if (loan is null)
                return NotFound();

            LoanResponse response = new LoanResponse
            {
                Id = loan.Id,
                BookId = loan.BookId,
                MemberId = loan.MemberId,
                LoanDate = loan.LoanDate,
                ReturnDate = loan.ReturnDate
            };

            return response;
        }

        [HttpPost]
        public async Task<ActionResult<LoanResponse>> CreateLoan(
            CreateLoanRequest request)
        {
            var (status, createdLoan) =  await _loanService.CreateLoan(request);

            if (status == LoanOperationStatus.BookNotFound)
                return NotFound("Book not found.");

            if (status == LoanOperationStatus.MemberNotFound)
                return NotFound("Member not found.");

            if (status == LoanOperationStatus.BookUnavailable)
                return BadRequest("This book is currently unavailable.");

            if (createdLoan is null)
                return StatusCode(500);

            LoanResponse response = new LoanResponse
            {
                Id = createdLoan.Id,
                BookId = createdLoan.BookId,
                MemberId = createdLoan.MemberId,
                LoanDate = createdLoan.LoanDate,
                ReturnDate = createdLoan.ReturnDate
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpPut("{id:int}/return")]
        public async Task<ActionResult> ReturnBook(int id)
        {
            LoanOperationStatus status =
                await _loanService.ReturnBook(id);

            if (status == LoanOperationStatus.LoanNotFound)
                return NotFound("Loan not found.");

            if (status == LoanOperationStatus.AlreadyReturned)
                return BadRequest("This book has already been returned.");

            if (status == LoanOperationStatus.BookNotFound)
                return NotFound("Book not found.");

            return NoContent();
        }
    }
}