using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data;
using SS.Backend.SharedNamespace;
using SS.Backend.ReservationManagement;
using SS.Backend.ReservationManagers;
using SS.Backend.SpaceManager;
using SS.Backend.DataAccess;
using SS.Backend.Security;
using System.Text.Json;
using System.Net.Mail;
using MailKit.Security;
using SS.Backend.Services.EmailService;
using SS.Backend.EmailConfirm;
using MailKit.Net.Smtp;
using System.IO;

using Microsoft.AspNetCore.Http.HttpResults;



namespace SpaceBookingCenterAPI.Controllers;

[ApiController]
[Route("api/v1/spaceBookingCenter/reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationCreationManager _reservationCreationManager;
    private readonly IReservationCancellationManager _reservationCancellationManager;
    private readonly IReservationModificationManager _reservationModificationManager;
    private readonly IReservationReaderManager _reservationReaderManager;
    private readonly IAvailibilityDisplayManager _availibilityDisplayManager;
    private readonly SSAuthService _authService;
    private readonly IConfiguration _config;
    private readonly IEmailConfirmService _emailConfirm;

    public ReservationController(IReservationCreationManager reservationCreationManager,
                                 IReservationCancellationManager reservationCancellationManager,
                                 IReservationModificationManager reservationModificationManager,
                                 IReservationReaderManager reservationReaderManager,
                                 IAvailibilityDisplayManager availibilityDisplayManager,
                                 SSAuthService authService, IConfiguration config,
                                 IEmailConfirmService emailConfirm)
                                 
    {
       _reservationCreationManager = reservationCreationManager;
       _reservationCancellationManager = reservationCancellationManager;
       _reservationModificationManager = reservationModificationManager;
       _reservationReaderManager = reservationReaderManager;
       _availibilityDisplayManager = availibilityDisplayManager;
       _emailConfirm = emailConfirm;

       _authService = authService;
       _config = config;
       
    }

    [HttpGet("ListReservations")]
    public async Task<IActionResult> ListUserReservations(string userName)
    {
        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (claimsJson != null)
            {
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                if (claims.TryGetValue("Role", out var role) && role == "1" || role == "2" || role == "3" || role == "4" || role == "5")
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
                            var reservations = await _reservationReaderManager.GetAllUserSpaceSurferSpaceReservationAsync(userName);
                            return Ok(new { reservations, newToken });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            var reservations = await _reservationReaderManager.GetAllUserSpaceSurferSpaceReservationAsync(userName);
                            return Ok(reservations);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
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

    [HttpGet("ListActiveReservations")]
    public async Task<IActionResult> ListUserActiveReservations(string userName)
    {

        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (claimsJson != null)
            {
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                if (claims.TryGetValue("Role", out var role) && role == "1" || role == "2" || role == "3" || role == "4" || role == "5")
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
                            var reservations = await _reservationReaderManager.GetAllUserActiveSpaceSurferSpaceReservationAsync(userName);
                            return Ok(new { reservations, newToken });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            var reservations = await _reservationReaderManager.GetAllUserActiveSpaceSurferSpaceReservationAsync(userName);
                            return Ok(reservations);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
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

    [HttpPost("CreateReservation")]
    public async Task<IActionResult> CreateReservation([FromBody] UserReservationsModel reservation)
    {
 
        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (claimsJson != null)
            {
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                if (claims.TryGetValue("Role", out var role) && role == "1" || role == "2" || role == "3" || role == "4" || role == "5")
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
                            var response = await _reservationCreationManager.CreateSpaceSurferSpaceReservationAsync(reservation);
                            return Ok(new { response, newToken });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            var response = await _reservationCreationManager.CreateSpaceSurferSpaceReservationAsync(reservation);
                            Console.WriteLine(response.ErrorMessage);
                            // send confirmation
                            if(!response.HasError)
                            {
                                if(reservation.ReservationID.HasValue)
                                {
                                    //get reservation ID
                                    SSPrincipal principal = new SSPrincipal();
                                    principal.UserIdentity = _authService.ExtractSubjectFromToken(accessToken);
                                    
                                    // create confirmation information
                                    int reservationID = reservation.ReservationID.Value;
                                    (string ics, string otp, string body, Response result) = await _emailConfirm.CreateConfirmation(reservationID);
                                    if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(ics) || string.IsNullOrEmpty(body) || result.HasError)
                                    {
                                        return BadRequest("Failed to create email confirmation.");
                                    }
                                    // send email
                                    await MailSender.SendConfirmEmail(principal.UserIdentity, ics, body);
                                }
                            }
                            return Ok(response);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
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

    [HttpPut("UpdateReservation")]
    public async Task<IActionResult> UpdateReservation([FromBody] UserReservationsModel reservation)
    {
         string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (claimsJson != null)
            {
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                if (claims.TryGetValue("Role", out var role) && role == "1" || role == "2" || role == "3" || role == "4" || role == "5")
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
                            var response = await _reservationModificationManager.ModifySpaceSurferSpaceReservationAsync(reservation);
                            return Ok(new { response, newToken });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            var response = await _reservationModificationManager.ModifySpaceSurferSpaceReservationAsync(reservation);
                            Console.WriteLine("in modification");
                            Console.WriteLine(response.ErrorMessage);
                            return Ok(response);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
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

    [HttpPut("CancelReservation")]
    public async Task<IActionResult> CancelReservation([FromBody] UserReservationsModel reservation)
    {
        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            var claimsJson = _authService.ExtractClaimsFromToken(accessToken);

            if (claimsJson != null)
            {
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(claimsJson);

                if (claims.TryGetValue("Role", out var role) && role == "1" || role == "2" || role == "3" || role == "4" || role == "5")
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
                            var response = await _reservationCancellationManager.CancelSpaceSurferSpaceReservationAsync(reservation);
                            return Ok(new { response, newToken });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            var response = await _reservationCancellationManager.CancelSpaceSurferSpaceReservationAsync(reservation);
                            return Ok(response);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, "Internal server error: " + ex.Message);
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

    [HttpGet("CheckAvailability")]
    public async Task<IActionResult> CheckAvailability(int companyId, DateTime startTime, DateTime endTime)
    {
        try
        {
            var response = await _availibilityDisplayManager.CheckAvailabilityAsync(companyId, startTime, endTime);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }



    [HttpGet("checkTokenExp")]
    public  IActionResult checkTokenExp()
    {

        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
            bool tokenExpired =  _authService.IsTokenExpired(accessToken);
            if (tokenExpired)
            {
                Console.WriteLine("Token is expired.");
                return Ok(true);
            }
            else
            {
                Console.WriteLine("Token is not expired.");
                return Ok(false);
            }
        }
        else
        {
            Console.WriteLine("Token is missing or invalid.");
            return BadRequest("Unauthorized. Access token is missing or invalid.");
        }
    }

}
