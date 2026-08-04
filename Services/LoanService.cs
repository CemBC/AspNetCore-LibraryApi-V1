using LibraryApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryApi.Services
{
    public class LoanService
    {
        private readonly List<Loan> _loans = new();
        private readonly BookService _bookService;
        private readonly MemberService _memberService;

        public LoanService(BookService bookService, MemberService memberService)
        {
            _bookService = bookService;
            _memberService = memberService;
        }

        public List<Loan> GetAll()
        {
            return _loans;
        }

        public Loan? GetById(int id)
        {
            foreach(Loan loan in _loans)
            {
                if (loan.Id == id) return loan;
            }
            return null;
        }

        public LoanOperationStatus CreateLoan(int bookId , int memberId , out Loan? createdLoan)
        {
            createdLoan = null;

            Book? book = _bookService.GetById(bookId);
            if(book is null) return LoanOperationStatus.BookNotFound;
            
            Member? member =  _memberService.GetById(memberId);
            if(member is null) return LoanOperationStatus.MemberNotFound;

            if (!book.IsAvailable) return LoanOperationStatus.BookUnavailable;

            createdLoan = new Loan
            {
                Id = GetNextId(),
                BookId = bookId,
                MemberId = memberId,
                LoanDate = DateTime.Now,
                ReturnDate = null
            };

            _loans.Add(createdLoan);

            book.IsAvailable = false;

            return LoanOperationStatus.Success;
        }

        public LoanOperationStatus ReturnBook(int loanId)
        {
            Loan? loan = GetById(loanId);

            if (loan is null) return LoanOperationStatus.LoanNotFound;
            
            if (loan.ReturnDate is not null) return LoanOperationStatus.AlreadyReturned;
            

            Book? book = _bookService.GetById(loan.BookId);

            if (book is null) return LoanOperationStatus.BookNotFound;
            

            loan.ReturnDate = DateTime.Now;
            book.IsAvailable = true;

            return LoanOperationStatus.Success;
        }

        private int GetNextId()
        {
            int highestId = 0;

            foreach (Loan loan in _loans)
            {
                if (loan.Id > highestId) highestId = loan.Id;
            }

            return highestId + 1;
        }
    }
}
