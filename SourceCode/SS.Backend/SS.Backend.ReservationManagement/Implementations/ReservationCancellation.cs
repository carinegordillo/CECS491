﻿using SS.Backend.DataAccess;
using SS.Backend.SharedNamespace;
using Microsoft.Data.SqlClient;
using System.Data;


namespace SS.Backend.ReservationManagement{

public class ReservationCancellation
{
    private ISqlDAO _sqldao;

        public ReservationCancellation(ISqlDAO sqldao)
        {
            _sqldao = sqldao;
        }

        public Response checkReservationStatus(string status)
        {
            // Response reservationUpdated = new Response();
            // IReservationStatusUpdater _reservationStatusUpdater = new ReservationStatusUpdater(_sqldao);

            // try{
            //     reservationUpdated = await _reservationStatusUpdater.updateReservtionStatus("dbo.ReservationS");
            // }

            // catch (Exception ex)
            // {
            //     Console.WriteLine($"Error updating reservation status: {ex.Message}");   // log here 
            // }

            if (status == "Cancelled")
            {
                return new Response { HasError =true, ErrorMessage = "Reservation has already been cancelled" };
            }
            else if  (status == "Active")
            {
                return new Response { HasError = false, ErrorMessage = "Reservation is active" };
            }
            else if (status == "Passed")
            {
                return new Response { HasError = false, ErrorMessage = "Cannot cancel a reservtaion that has passed" };
            }
            else
            {
                return new Response { HasError = false, ErrorMessage = "Invalid Status" };
            }
        
        }

        public async Task<Response> CancelReservation(string tableName, int reservationID)
        {
            Response response = new Response();

            var commandBuilder = new CustomSqlCommandBuilder();

            var parameters = new Dictionary<string, object>
                        {
                            { "status", "Cancelled" }
                        };

    
            var updateCommand = commandBuilder.BeginUpdate(tableName)
                                            .Set(parameters)
                                            .Where($"reservationID = {reservationID}")
                                            .AddParameters(parameters)
                                            .Build();

            response = await _sqldao.SqlRowsAffected(updateCommand);

            if (response.HasError == false)
            {
                response.ErrorMessage += "- CancelReservation - command successful -";
                response.HasError = false;
            }
            else
            {
                response.ErrorMessage += $"- CancelReservation - command : {updateCommand.CommandText} not successful -";
                response.HasError = true;

            }
            return response;
        }
    }
}
