﻿using System;
using Microsoft.AspNetCore.Mvc;
using SS.Backend.Security;
using SS.Backend.Services.EmailService;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using System.Threading.Tasks;
using SS.Backend.UserDataProtection;
using SS.Backend.Services.DeletingService;

namespace UserDataProtectionAPI.Controllers
{
    [ApiController]
    [Route("api/userDataProtection")]
    public class UserDataProtectionController : Controller
    {
        private readonly SSAuthService _authService;
        private readonly UserDataProtection _userDataProtection;
        //private readonly IAccountDeletion _accountDeletion;

        //public UserDataProtectionController(SSAuthService authService, UserDataProtection userDataProtection, IAccountDeletion accountDeletionService)
        public UserDataProtectionController(SSAuthService authService, UserDataProtection userDataProtection)
        {
            _authService = authService;
            _userDataProtection = userDataProtection;
            //_accountDeletion = accountDeletionService;
        }

        [HttpPost("accessData")]
        public async Task<IActionResult> accessData([FromBody] string userHash)
        {
            string? accessToken = HttpContext.Request.Headers["Authorization"];
            if (accessToken != null && accessToken.StartsWith("Bearer "))
            {
                accessToken = accessToken.Substring("Bearer ".Length).Trim();
                var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

                if (claimsJson != null)
                {
                    var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                    if (claims.TryGetValue("Role", out var role) && (role == "1" || role == "2" || role == "3"))
                    {
                        bool closeToExpTime = _authService.CheckExpTime(accessToken);
                        if (closeToExpTime)
                        {
                            SSPrincipal principal = new SSPrincipal();
                            principal.UserIdentity = _authService.ExtractSubjectFromToken(accessToken);
                            principal.Claims = _authService.ExtractClaimsFromToken_Dictionary(accessToken);
                            var newToken = _authService.CreateJwt(Request, principal);

                            try
                            {
                                Console.WriteLine("Attempting to access data.");
                                var userData = await _userDataProtection.accessData_Manager(userHash);
                                Console.WriteLine("Successfully accessed data.");

                                try
                                {
                                    await _userDataProtection.sendAccessEmail_Manager(userData);
                                    Console.WriteLine("Successfully sent email.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred while sending email: {ex.Message}");
                                }

                                return Ok( newToken );
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, ex.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                Console.WriteLine("Attempting to access data.");
                                var userData = await _userDataProtection.accessData_Manager(userHash);
                                Console.WriteLine("Successfully accessed data.");

                                try
                                {
                                    await _userDataProtection.sendAccessEmail_Manager(userData);
                                    Console.WriteLine("Successfully sent email.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred while sending email: {ex.Message}");
                                }
                                return Ok();
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, ex.Message);
                            }

                        }
                    }
                    
                    else if (role == "4" || role == "5")
                    {
                        bool closeToExpTime = _authService.CheckExpTime(accessToken);
                        if (closeToExpTime)
                        {
                            SSPrincipal principal = new SSPrincipal();
                            principal.UserIdentity = _authService.ExtractSubjectFromToken(accessToken);
                            principal.Claims = _authService.ExtractClaimsFromToken_Dictionary(accessToken);
                            var newToken = _authService.CreateJwt(Request, principal);

                            try
                            {
                                Console.WriteLine("Attempting to access data.");
                                var userData = await _userDataProtection.accessData_GeneralUser(userHash);
                                Console.WriteLine("Successfully accessed data.");
                                
                                try
                                {
                                    await _userDataProtection.sendAccessEmail_General(userData);
                                    Console.WriteLine("Successfully sent email.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred while sending email: {ex.Message}");
                                }

                                return Ok(newToken);
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, ex.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                Console.WriteLine("Attempting to access data.");
                                var userData = await _userDataProtection.accessData_GeneralUser(userHash);
                                Console.WriteLine("Successfully accessed data.");

                                try
                                {
                                    await _userDataProtection.sendAccessEmail_General(userData);
                                    Console.WriteLine("Successfully sent email.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred while sending email: {ex.Message}");
                                }

                                return Ok();
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, ex.Message);
                            }

                        }
                    }
                    else
                    {
                        return BadRequest("Unauthorized role.");
                    }

                }
                else
                {
                    return BadRequest("Invalid token.");
                }
            }
            else
            {
                return BadRequest("Unauthorized. Access token is missing or invalid.");
            }
        }

        [HttpPost("deleteData")]
        public async Task<IActionResult> DeleteData([FromBody] string userHash)
        {
            string? accessToken = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.StartsWith("Bearer "))
            {
                return BadRequest("Unauthorized. Access token is missing or invalid.");
            }

            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (string.IsNullOrEmpty(claimsJson))
            {
                return BadRequest("Invalid token.");
            }

            var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

            if (!claims.TryGetValue("Role", out var role))
            {
                return BadRequest("Role not found in claims.");
            }

            bool closeToExpTime = _authService.CheckExpTime(accessToken);

            try
            {
                var userData = new UserDataModel();
                if (role == "1" || role == "2" || role == "3")
                {
                    userData = await _userDataProtection.accessData_Manager(userHash);
                    Console.WriteLine("Attempting to send delete email.");

                    await _userDataProtection.sendDeleteEmail_Manager(userData);

                    Console.WriteLine("Attempting to delete logs.");
                    await _userDataProtection.deleteData(userHash);
                    Console.WriteLine("Successfully deleted data.");
                }
                else if (role == "4" || role == "5")
                {
                    Console.WriteLine("Attempting to access data.");
                    userData = await _userDataProtection.accessData_GeneralUser(userHash);
                    Console.WriteLine("Attempting to send delete email.");

                    await _userDataProtection.sendDeleteEmail_General(userData);

                    Console.WriteLine("Attempting to delete logs.");
                    await _userDataProtection.deleteData(userHash);
                    Console.WriteLine("Successfully deleted data.");
                }
                else
                {
                    return BadRequest("Invalid role.");
                }

                if (closeToExpTime)
                {
                    var principal = new SSPrincipal
                    {
                        UserIdentity = _authService.ExtractSubjectFromToken(accessToken),
                        Claims = _authService.ExtractClaimsFromToken_Dictionary(accessToken)
                    };
                    var newToken = _authService.CreateJwt(Request, principal);

                    return Ok(newToken);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("checkTokenExp")]
        public IActionResult checkTokenExp()
        {

            string? accessToken = HttpContext.Request.Headers["Authorization"];
            if (accessToken != null && accessToken.StartsWith("Bearer "))
            {
                accessToken = accessToken.Substring("Bearer ".Length).Trim();
                bool tokenExpired = _authService.IsTokenExpired(accessToken);
                if (tokenExpired)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            else
            {
                return BadRequest("Unauthorized. Access token is missing or invalid.");
            }
        }

    }
}

