using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FinancialApp.Areas.Identity.Pages.Account
{
    public class EmailSenderModel : PageModel
        {
            private readonly IEmailSender _emailSender;

            public EmailSenderModel (IEmailSender emailSender)
            {
                _emailSender = emailSender;
            }

        public string? UserName { get; set; }
        public string ConfirmationLink { get; set; }
        public async Task OnGetAsync()
            {
                // Example of sending an email
                await _emailSender.SendEmailAsync("example@example.com", "Subject", "Email body");
            }

    }
}
