using SS.Backend.SharedNamespace;
using SS.Backend.ReservationManagement;


namespace SS.Backend.ReservationManagers{

    public class ReservationCancellationManager
    {
        private readonly string SS_RESERVATIONS_TABLE = "dbo.reservations";
        private readonly IReservationCancellationService _reservationCancellationService;
        private readonly IReservationValidationService _reservationValidationService;

        private readonly IReservationRequirements _reservationRequirements = new SpaceSurferReservationRequirements();
        
    

        public ReservationCancellationManager(IReservationCancellationService reservationCancellationService, IReservationValidationService reservationValidationService)
        {
            _reservationCancellationService = reservationCancellationService;
            _reservationValidationService = reservationValidationService;
            
        }

        public async Task<Response> CancelSpaceSurferSpaceReservationAsync(UserReservationsModel userReservationsModel, string? tableNameOverride = null)
        {
            
            Response response = new Response();
            Response reservationCancellationResponse = new Response();

            string tableName = tableNameOverride ?? SS_RESERVATIONS_TABLE;

           
                
            ReservationValidationFlags flags = ReservationValidationFlags.ReservationStatusIsActive;
        
            Response validationResponse = await _reservationValidationService.ValidateReservationAsync(userReservationsModel, flags, _reservationRequirements);
            
            if (validationResponse.HasError)
            {
                response.ErrorMessage += "Reservation did not pass validation checks: " + validationResponse.ErrorMessage;
                response.HasError = true;
            }
            

            else
            {
                

                if (userReservationsModel.ReservationID.HasValue)
                {
                   try
                    {
                        
                        reservationCancellationResponse =  await _reservationCancellationService.CancelReservationAsync(tableName, userReservationsModel.ReservationID.Value);
                        if (reservationCancellationResponse.HasError)
                        {
                            
                            response.ErrorMessage = "CancelSpaceSurferSpaceReservationAsync, could not create Reservation.";
                            response.HasError = true;
                        }
                        else
                        {
                            response.ErrorMessage = "CancelSpaceSurferSpaceReservationAsync, Reservation created successfully.";
                            response.HasError = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        response.HasError = true;
                        response.ErrorMessage = ex.Message;
                    }
                }
                else
                {

                    reservationCancellationResponse = new Response { HasError = true, ErrorMessage = "Reservation ID is null." };
                }
            }
            return response;
        }
    }
}

    