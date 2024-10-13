using FinancialApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using FinancialApp.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace FinancialApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FinancialUser> _userManager;

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context, UserManager<FinancialUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Account/
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Fetching accounts for user with Id: {UserId}", userId);

            // Get the full FinancialUser object
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(); // Handle the case where the user isn't found
            }

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

            // Retrieve all transactions for the current month
            var transactions = await _context.Transactions
                .Where(t => accounts.Select(a => a.Id).Contains(t.AccountId) &&
                            (t.Type == TransactionType.Expense || t.Type == TransactionType.Income) &&
                            t.Date.Month == DateTime.Now.Month &&
                            t.Date.Year == DateTime.Now.Year)
                .ToListAsync();

            // Calculate total balance for each account
            var totalBalances = accounts.ToDictionary(
                account => account.Id,
                account => transactions
                    .Where(t => t.AccountId == account.Id)
                    .GroupBy(t => t.Date.Date)
                    .Select(g => g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount))
                    .Sum()
            );

            // Group transactions by AccountId and Date
            var dailyBalances = transactions
                .GroupBy(t => new { t.AccountId, t.Date.Date })
                .Select(g => new
                {
                    AccountId = g.Key.AccountId,
                    AccountName = accounts.First(a => a.Id == g.Key.AccountId).Name,
                    Date = g.Key.Date,
                    Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount),
                    Balance = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount) -
                              g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
                })
                .GroupBy(x => new { x.AccountId, x.AccountName })
                .Select(g => new
                {
                    AccountId = g.Key.AccountId,
                    AccountName = g.Key.AccountName,
                    Data = g.OrderBy(x => x.Date).Select(x => new { x.Date, x.Balance })
                })
                .ToList();

            // Pass LastName to the view
            ViewData["LastName"] = user.LastName;
            ViewData["DailyBalances"] = dailyBalances;
            ViewData["TotalBalances"] = totalBalances;

            return View(accounts);
        }
        // GET: /Account/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.IconList = new List<string>
            {
                "🏦","💸","📈","💳"
            };
            return View();
        }

        // POST: /Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InputModel inputModel)
        {
            _logger.LogInformation($"Creating account with Name: {inputModel.Name}, Category: {inputModel.Category}");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                var account = new Account
                {
                    Name = inputModel.Name,
                    Category = inputModel.Category,
                    Icon = inputModel.Icon,
                    UserId = userId
                };
                
                try
                {
                    _context.Add(account);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Account successfully created.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the account.");
                    ModelState.AddModelError("", "An error occurred while saving the account. Please try again.");
                }
            }

            ViewBag.IconList = new List<string>
            {
                "🏦","💸","📈","💳"
            };

            return View(inputModel);
        }

        // GET: /Account/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var account = await _context.Accounts
                .Where(a => a.UserId == userId && a.Id == id)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            var model = new InputModel
            {
                Id = account.Id,
                Name = account.Name,
                Category = account.Category,
                Icon = account.Icon
            };

            return View(model);
        }

        // POST: /Account/Edit/5
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
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var account = await _context.Accounts
                        .Where(a => a.UserId == userId && a.Id == id)
                        .FirstOrDefaultAsync();

                    if (account == null)
                    {
                        return NotFound();
                    }

                    account.Name = inputModel.Name;
                    account.Category = inputModel.Category;
                    account.Icon = inputModel.Icon;

                    _context.Update(account);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Account successfully updated.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogWarning("Concurrency issue occurred while editing account with Id: {AccountId}", id);
                        ModelState.AddModelError("", "The account was updated by another user. Please reload the page and try again.");
                    }
                }
            }

            return View(inputModel);
        }

        // GET: /Account/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var account = await _context.Accounts
                .Where(a => a.UserId == userId && a.Id == id)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: /Account/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var account = await _context.Accounts
                .Where(a => a.UserId == userId && a.Id == id)
                .FirstOrDefaultAsync();

            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Account with Id: {AccountId} deleted successfully for user with Id: {UserId}.", id, userId);
            }
            else
            {
                _logger.LogWarning("Attempt to delete a non-existent or unauthorized account with Id: {AccountId}", id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }

        // Input Model for Account 
        public class InputModel
        {
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }

            [Required]
            public AccountCategory Category { get; set; }

            [Required]
            public string Icon { get; set; }
        }
    }
}
