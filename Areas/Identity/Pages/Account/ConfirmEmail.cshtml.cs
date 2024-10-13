using FinancialApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace FinancialApp.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<FinancialUser> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(UserManager<FinancialUser> userManager, ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string userId = null, string code = null)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                _logger.LogInformation("User email confirmed.");
                return RedirectToPage("/Account/Login");
            }
            else
            {
                _logger.LogWarning("Error confirming user email.");
                return Page();
            }
        }
    }
}
