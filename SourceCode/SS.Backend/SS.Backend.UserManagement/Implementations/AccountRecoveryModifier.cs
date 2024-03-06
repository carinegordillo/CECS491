using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SS.Backend.SharedNamespace;


namespace SS.Backend.UserManagement
{
    public class AccountRecoveryModifier : IAccountRecoveryModifier
    {

        private IUserManagementDao _userManagementDao;

        public AccountRecoveryModifier(IUserManagementDao userManagementDao)
        {
            _userManagementDao = userManagementDao;
        }

        /* 
        * This method is used to update the status of a user account in the ActiveAccount table to enabled
        * @param userhash - the hashed username of the user
        * @return Response - the response object
        */

        public async Task<Response> EnableAccount(string hashedUsername){

            Console.WriteLine("Enabling account");


            Response table1Result = await _userManagementDao.GeneralModifier("hashedUsername", hashedUsername, "IsActive", "yes", "dbo.activeAccount");
            Response table2Result = new Response();

            Console.WriteLine("hased username: " + hashedUsername  );

            if (table1Result.HasError == false){

                table1Result.ErrorMessage += "- Updated account status to enabled successful -";
                table2Result = await ResolveRequest(hashedUsername, "accepted");

                if (table2Result.HasError == false)
                {
                    table2Result.ErrorMessage += "- Updated request status to successful -";
                }
                else
                {
                    table1Result.ErrorMessage += "- Could not update request status to successful - ";
                }
            }
            else{
                 
                 
                 table1Result.ErrorMessage += "- Could not update account status to enabled - ";

            }

            Console.WriteLine(table1Result.ErrorMessage + " " + table2Result.ErrorMessage + " - Enable Account -");

            return table1Result;

        }

        /* 
        * This method is used to update the status of a user request in the userRequests table to accepted or denied, 
        * indicating it has been resolved and should not longer be considered by the admin
        * @param userHash - the hashed username of the user
        * @param resolveStatus - the status to update the request to
        * @return Response - the response object
        */

        public async Task<Response> ResolveRequest(string userHash, string resolveStatus){
            Console.WriteLine("Resolving account");

            Response response = new Response();

            try{
                response = await _userManagementDao.GeneralModifier("userHash", userHash, "status" , resolveStatus, "dbo.userRequests");
                 DateTime now = DateTime.Now;
                response = await _userManagementDao.GeneralModifier("userHash", userHash, "resolveDate" , now, "dbo.userRequests");
            }
            catch (Exception e){
                response.HasError = true;
                response.ErrorMessage += e.Message + "- Resolve Request  Failed -"; 
            }
            


            return response;
        }


        /* 
        * This method is used to update the status of a user account in the ActiveAccount table to pending
        * @param userhash - the hashed username of the user
        * @return Response - the response object
        */
        public async Task<Response> PendingRequest(string userhash){

            Response result = await _userManagementDao.GeneralModifier("hashedUsername", userhash, "IsActive", "pending", "dbo.activeAccount");

            if (result.HasError == false){
                result.ErrorMessage += "- Updated account status to pending successful -";
            }
            else{
                 result.ErrorMessage += "- Could not update account status to pending - ";

            }
            return result;
        }

        public async Task<Response> ReadUserPendingRequests(){
            
            Response response = new Response();
            
            response = await _userManagementDao.readTableWhere("status", "Pending", "dbo.userRequests");

            if (response.HasError == false)
            {
                response.ErrorMessage += "- ReadUserRequests successful. -";
            }
            else
            {
                response.ErrorMessage += "- ReadUserRequests Failed - ";
            }

            return response;
        }


    }
}