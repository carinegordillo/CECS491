﻿using Microsoft.AspNetCore.Mvc;
using SS.Backend.Security;
using SS.Backend.Services.EmailService;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthNController : Controller
    {
        private readonly SSAuthService _authService;
        private readonly IConfiguration _config;

        public AuthNController(SSAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("sendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] AuthenticationRequest request)
        {
            var (otp, response) = await _authService.SendOTP_and_SaveToDB(request);

            if (response.HasError == false)
            {
                string targetEmail = request.UserIdentity;
                string subject = "Verfication Code";
                string msg = "Verification code: " + otp;
                await MailSender.SendEmail(targetEmail, subject, msg);
            }

            if (response.HasError)
            {
                return BadRequest(response.ErrorMessage);
            }

            return Ok(new { otp });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
        {
            var (principal, response) = await _authService.Authenticate(request);

            if (response.HasError)
            {
                return BadRequest(response.ErrorMessage);
            }

            var rolesDictionary = principal.Claims;
            var token = _authService.GenerateAccessToken(principal.UserIdentity, rolesDictionary);

            return Ok(token);
        }

        [HttpPost("decodeToken")]
        public async Task<IActionResult> decodeToken([FromBody] string accessToken)
        {
            List<string> info = _authService.GetRolesFromToken(accessToken);

            return Ok(info);
        }
    }
}
