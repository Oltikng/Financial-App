using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace FinancialApp.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly IRazorViewEngine _viewEngine;
        private readonly IWebHostEnvironment _env;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, IRazorViewEngine viewEngine, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _logger = logger;
            _viewEngine = viewEngine;
            _env = env;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
                var view = viewResult.View;

                var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var tempDataProvider = actionContext.HttpContext.RequestServices.GetService<ITempDataProvider>();
                var tempData = new TempDataDictionary(actionContext.HttpContext, tempDataProvider);

                var viewContext = new ViewContext(actionContext, view, viewData, tempData, sw, new HtmlHelperOptions());

                await view.RenderAsync(viewContext);
                return sw.ToString();
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = smtpSettings["SmtpServer"];
            var smtpPort = int.Parse(smtpSettings["SmtpPort"]);
            var senderName = smtpSettings["SenderName"];
            var senderEmail = smtpSettings["SenderEmail"];
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"]);
            var timeout = int.Parse(smtpSettings["Timeout"]);

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using (var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                Timeout = timeout
            })
            {
                try
                {
                    await client.SendMailAsync(message);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogError($"Email sending failed: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
