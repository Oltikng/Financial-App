using FinancialApp.Areas.Identity.Data;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

            if (userId == null)
            {
                return Unauthorized();
            }

            var userSettings = await _context.Settings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userSettings == null)
            {
                userSettings = new Settings
                {
                    PreferredCurrency = CurrencyType.USD, // Default currency
                    UserId = userId
                };

                _context.Settings.Add(userSettings);
                await _context.SaveChangesAsync();
            }

            ViewBag.UserCurrency = userSettings.PreferredCurrency;

            return View(userSettings);
        }

        // POST: /Settings/SaveSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(Settings model)
        {
            var userId = _userManager.GetUserId(User);
            var userSettings = await _context.Settings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (userSettings != null)
            {
                userSettings.PreferredCurrency = model.PreferredCurrency;
                _context.Settings.Update(userSettings);
                await _context.SaveChangesAsync();
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Default currency updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
