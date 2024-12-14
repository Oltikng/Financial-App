using FinancialApp.Areas.Identity.Data;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<FinancialUser> _userManager;

        public SettingsController(ApplicationDbContext context, UserManager<FinancialUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Settings/
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userSettings = await _context.Settings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (userSettings == null)
            {
                // Initialize settings if they do not exist
                userSettings = new Settings
                {
                    PreferredCurrency = CurrencyType.USD, // Default currency
                    UserId = userId
                };

                _context.Settings.Add(userSettings);
                await _context.SaveChangesAsync();
            }

            return View(userSettings);
        }

        // POST: /Settings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveSettings(string defaultCurrency)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var settings = await _context.Settings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
               
                settings = new Settings
                {
                    UserId = userId,
                    DefaultCurrency = defaultCurrency
                };
                _context.Settings.Add(settings);
            }
            else
            {
                // Update existing settings
                settings.DefaultCurrency = defaultCurrency;
                _context.Settings.Update(settings);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Default currency updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
