// using MailKit.Security;
// using Microsoft.AspNetCore.Mvc;
// using SS.Backend.Services.EmailService;
// using System.Net.Mail;
// using SS.Backend.EmailConfirm;

// namespace SecurityAPI.Controllers
// {
//     [ApiController]
//     [Route("api/mail")]
//     public class MailController : Controller
//     {
//         private readonly EmailConfirmService _emailConfirm;

//         [HttpPost]
//         public async Task<IActionResult> Send()
//         {
//             (string ics, string otp, var result) = await _emailConfirm.CreateConfirmation(6);
//             string targetEmail = "4sarahsantos@gmail.com";
//             string subject = "Testing Email Confirmation";
//             string msg = $"Hello Sarah,\n\nThis is a test email sent from SpaceSurfers! \nReservation: {ics} \nConfirmation Otp: {otp} \n\nBest,\nPixelPals";

//             try
//             {
//                 await MailSender.SendEmail(targetEmail, subject, msg);
//                 Console.WriteLine("Successfully sent email.");
//                 return Ok("Success");
//             }
//             catch (SmtpException ex)
//             {
//                 Console.WriteLine("SMTP error: " + ex.Message);
//                 return StatusCode(500, "SMTP error: " + ex.Message);
//             }
//             catch (IOException ex)
//             {
//                 Console.WriteLine("IO error: " + ex.Message);
//                 return StatusCode(500, "IO error: " + ex.Message);
//             }
//             catch (AuthenticationException ex)
//             {
//                 Console.WriteLine("Authentication error: " + ex.Message);
//                 return StatusCode(500, "Authentication error: " + ex.Message);
//             }
//             catch (Exception ex) // Catch any other unexpected exceptions
//             {
//                 Console.WriteLine("Error sending email: " + ex.Message);
//                 return StatusCode(500, "Error sending email: " + ex.Message);
//             }
//         }
//     }
// }

using System.Net.Mail;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SS.Backend.Services.EmailService;
using SS.Backend.EmailConfirm;
// Assuming MailSender is using MailKit for sending emails
using MailKit.Net.Smtp;
using System.IO;

namespace SecurityAPI.Controllers
{
    [ApiController]
    [Route("api/mail")]
    public class MailController : ControllerBase
    {
        private readonly ILogger<MailController> _logger;
        private readonly IEmailConfirmService _emailConfirm;

        public MailController(IEmailConfirmService emailConfirm, ILogger<MailController> logger)
        {
            _emailConfirm = emailConfirm ?? throw new ArgumentNullException(nameof(emailConfirm));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] string targetEmail)
        {
            if (string.IsNullOrWhiteSpace(targetEmail))
            {
                return BadRequest("Target email address is required.");
            }

            try
            {
                (string ics, string otp, var result) = await _emailConfirm.CreateConfirmation(6);
                string subject = "Testing Email Confirmation";
                string msg = $"Hello,\n\nThis is a test email sent from SpaceSurfers! \nReservation: {ics} \nConfirmation Otp: {otp} \n\nBest,\nPixelPals";
                
                await MailSender.SendEmail(targetEmail, subject, msg);
                _logger.LogInformation("Successfully sent email to {Email}.", targetEmail);
                return Ok("Success");
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP command error while sending email.");
                return StatusCode(500, "SMTP command error: " + ex.Message);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Authentication error while sending email.");
                return StatusCode(500, "Authentication error: " + ex.Message);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error while sending email.");
                return StatusCode(500, "IO error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");
                return StatusCode(500, "Error sending email: " + ex.Message);
            }
        }
    }
}

