using FinancialApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FinancialApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FinancialApp.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<FinancialUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<FinancialUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            // Get user settings
            var userSettings = await _context.Settings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userSettings == null)
            {
                userSettings = new Settings
                {
                    PreferredCurrency = CurrencyType.USD,
                    UserId = userId
                };

                _context.Settings.Add(userSettings);
                await _context.SaveChangesAsync();
            }

            ViewBag.UserCurrency = userSettings.PreferredCurrency;

            // Retrieve all accounts for the current user
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Retrieve all transactions for the current month from these accounts
            var transactions = await _context.Transactions
                .Where(t => accounts.Select(a => a.Id).Contains(t.AccountId) &&
                            t.Date.Month == DateTime.Now.Month &&
                            t.Date.Year == DateTime.Now.Year)
                .ToListAsync();

            // Calculate total income and expenses
            var totalIncomes = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var totalBalance = totalIncomes - totalExpenses;

            // Prepare data for the view
            ViewData["TotalIncomes"] = totalIncomes;
            ViewData["TotalExpenses"] = totalExpenses;
            ViewData["TotalBalance"] = totalBalance;

            // Pass the accounts and user data to the view
            ViewData["FirstName"] = user.FirstName;
            ViewData["LastName"] = user.LastName;

            // Pass the accounts to the view
            return View(accounts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
