using System;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinancialApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;  
using PagedList;
using PagedList.Mvc;
using X.PagedList;


namespace FinancialApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FinancialUser> _userManager;

        public TransactionsController(ILogger<TransactionsController> logger, ApplicationDbContext context, UserManager<FinancialUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.IconList = new Dictionary<string, string>
            {
                {"🏠", "Booking 🏠"},
                {"🍔", "Food 🍔"},
                {"🎉", "Entertainment 🎉"},
                {"🏦", "Taxes 🏦"},
                {"💸", "Salary 💸"},
                {"📈", "Investment 📈"},
                {"🚗", "Rent Car 🚗"},
                {"💰", "Rent 💰"},
                {"💳", "Debt 💳"},
                {"🚌", "Transport 🚌"},
                {"🥦", "Groceries 🥦"},
                {"🌐", "Online Shopping 🌐"},
                {"🛒", "Shopping 🛒"}
            };
            ViewBag.AccountId = new SelectList(_context.Accounts.Where(a => a.UserId == userId), "Id", "Name");
            return View(new InputModel());
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var transaction = new Transaction
                {
                    Category = inputModel.Category,
                    Amount = inputModel.Amount,
                    Date = DateTime.Now,
                    AccountId = inputModel.AccountId,
                    Type = inputModel.Type,
                    Currency = inputModel.Currency,
                    Icon = inputModel.Icon
                };

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction successfully created.");
                return RedirectToAction(nameof(Index));
            }

            var userIdForView = _userManager.GetUserId(User);
            ViewBag.IconList = new Dictionary<string, string>
            {
                {"🏠", "Booking 🏠"},
                {"🍔", "Food 🍔"},
                {"🎉", "Entertainment 🎉"},
                {"🏦", "Taxes 🏦"},
                {"💸", "Salary 💸"},
                {"📈", "Investment 📈"},
                {"🚗", "Rent Car 🚗"},
                {"💰", "Rent 💰"},
                {"💳", "Debt 💳"},
                {"🚌", "Transport 🚌"},
                {"🥦", "Groceries 🥦"},
                {"🌐", "Online Shopping 🌐"},
                {"🛒", "Shopping 🛒"}
            };
            ViewData["AccountId"] = new SelectList(_context.Accounts.Where(a => a.UserId == userIdForView), "Id", "Name", inputModel.AccountId);
            return View(inputModel);
        }

        // GET: Transactions/Index
        public async Task<IActionResult> Index(int? accountId, int? month, int? year, int page = 1, int pageSize = 10, string sortOrder = null)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Fetching transactions for user with Id: {UserId}", userId);

            // Set default values for month and year if not provided
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;

            // Fetch all transactions belonging to the current user
            IQueryable<Transaction> transactionsQuery = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId)
                .Where(t => t.Date.Month == month.Value && t.Date.Year == year.Value);

            if (accountId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.AccountId == accountId.Value);
            }

            // Sorting functionality
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CategorySortParm = String.IsNullOrEmpty(sortOrder) ? "category_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.TypeSortParm = sortOrder == "Type" ? "type_desc" : "Type";

            switch (sortOrder)
            {
                case "category_desc":
                    transactionsQuery = transactionsQuery.OrderByDescending(t => t.Category);
                    break;
                case "Date":
                    transactionsQuery = transactionsQuery.OrderBy(t => t.Date);
                    break;
                case "date_desc":
                    transactionsQuery = transactionsQuery.OrderByDescending(t => t.Date);
                    break;
                case "Type":
                    transactionsQuery = transactionsQuery.OrderBy(t => t.Type);
                    break;
                case "type_desc":
                    transactionsQuery = transactionsQuery.OrderByDescending(t => t.Type);
                    break;
                default:
                    transactionsQuery = transactionsQuery.OrderBy(t => t.Date);
                    break;
            }

            var transactions = await transactionsQuery.ToPagedListAsync(page, pageSize);

            // Pass the accountId to the view for filtering purposes if applicable
            if (accountId.HasValue)
            {
                ViewData["AccountId"] = accountId.Value;
            }

            // Prepare data for pie charts
            var incomeData = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Icon)
                .Select(g => new
                {
                    Icon = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            var expenseData = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Icon)
                .Select(g => new
                {
                    Icon = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            // Pass the data to the view
            ViewBag.IncomeData = incomeData;
            ViewBag.ExpenseData = expenseData;
            ViewData["AccountId"] = accountId;

            return View(transactions);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            var model = new InputModel
            {
                Id = transaction.Id,
                Category = transaction.Category,
                Amount = transaction.Amount,
                Date = DateTime.Now,
                AccountId = transaction.AccountId,
                Type = transaction.Type,
                Currency = transaction.Currency,
                Icon = transaction.Icon
            };

            ViewBag.IconList = new Dictionary<string, string>
            {
                {"🏠", "Booking 🏠"},
                {"🍔", "Food 🍔"},
                {"🎉", "Entertainment 🎉"},
                {"🏦", "Taxes 🏦"},
                {"💸", "Salary 💸"},
                {"📈", "Investment 📈"},
                {"🚗", "Rent Car 🚗"},
                {"💰", "Rent 💰"},
                {"💳", "Debt 💳"},
                {"🚌", "Transport 🚌"},
                {"🥦", "Groceries 🥦"},
                {"🌐", "Online Shopping 🌐"},
                {"🛒", "Shopping 🛒"}
            };
            ViewData["AccountId"] = new SelectList(_context.Accounts.Where(a => a.UserId == userId), "Id", "Name", transaction.AccountId);
            return View(model);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InputModel inputModel)
        {
            if (id != inputModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .Where(t => t.Account.UserId == userId && t.Id == id)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return NotFound();
                }

                transaction.Category = inputModel.Category;
                transaction.Amount = inputModel.Amount;
                transaction.Date = DateTime.Now;
                transaction.AccountId = inputModel.AccountId;
                transaction.Type = inputModel.Type;
                transaction.Currency = inputModel.Currency;
                transaction.Icon = inputModel.Icon;

                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Transaction successfully updated.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(inputModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogWarning("Concurrency issue occurred while editing transaction with Id: {TransactionId}", id);
                        ModelState.AddModelError("", "The transaction was updated by another user. Please reload the page and try again.");
                    }
                }
            }

            var userIdForView = _userManager.GetUserId(User);
            ViewBag.IconList = new Dictionary<string, string>
            {
                {"🏠", "Booking 🏠"},
                {"🍔", "Food 🍔"},
                {"🎉", "Entertainment 🎉"},
                {"🏦", "Taxes 🏦"},
                {"💸", "Salary 💸"},
                {"📈", "Investment 📈"},
                {"🚗", "Rent Car 🚗"},
                {"💰", "Rent 💰"},
                {"💳", "Debt 💳"},
                {"🚌", "Transport 🚌"},
                {"🥦", "Groceries 🥦"},
                {"🌐", "Online Shopping 🌐"},
                {"🛒", "Shopping 🛒"}
            };
            ViewData["AccountId"] = new SelectList(_context.Accounts.Where(a => a.UserId == userIdForView), "Id", "Name", inputModel.AccountId);
            return View(inputModel);
        }

        // GET: Transactions/CreateIncome
        public IActionResult CreateIncome()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name");
            return View("Create", new InputModel { Type = TransactionType.Income });
        }

        // GET: Transactions/CreateExpense
        public IActionResult CreateExpense()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name");
            return View("Create", new InputModel { Type = TransactionType.Expense });
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.UserId == userId && t.Id == id)
                .FirstOrDefaultAsync();

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction with Id: {TransactionId} deleted successfully for user with Id: {UserId}.", id, userId);
            }
            else
            {
                _logger.LogWarning("Attempt to delete a non-existent or unauthorized transaction with Id: {TransactionId}", id);
            }
            return RedirectToAction(nameof(Index));
        }

        // Other action methods remain unchanged

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        // Input Model for Transaction Creation and Editing
        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            public TransactionCategory Category { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            public decimal Amount { get; set; }

            [Required]
            public TransactionType Type { get; set; }

            [Required]
            public int AccountId { get; set; }

            [Required]
            public CurrencyType Currency { get; set; }
            public DateTime Date { get; set; }
            [Required]
            public string Icon { get; set; }
        }
    }
}
