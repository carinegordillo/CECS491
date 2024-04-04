using SS.Backend.SharedNamespace;
using SS.Backend.Services.CalendarService;
using System.Data;



namespace SS.Backend.EmailConfirm
{
    public class EmailConfirmService : IEmailConfirmService
    {
        private readonly IEmailConfirmDAO _emailDAO;

        public EmailConfirmService(IEmailConfirmDAO emailDAO)
        {
            _emailDAO = emailDAO;
        }

        public async Task<(string ics, string otp, string body, Response res)> CreateConfirmation(int reservationID)
        {
            var response = new Response();
            //DataRow reservationInput = reservationInfo.ValuesRead.Rows[0];
            //var reservationinfo = new ReservationInfo();
            var baseDirectory = AppContext.BaseDirectory;
            var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
            var icsFilePath = Path.Combine(projectRootDirectory, "CalendarFiles", "SSReservation.ics");
            var infoResponse = await _emailDAO.GetReservationInfo(reservationID);
            string otp;
            string htmlBody;
            if (!infoResponse.HasError && infoResponse.ValuesRead != null && infoResponse.ValuesRead.Rows.Count > 0)
            {
                Console.WriteLine(infoResponse.ErrorMessage);
                DataRow row = infoResponse.ValuesRead.Rows[0];

                var getOtp = new GenOTP();
                otp = getOtp.generateOTP();

                //extract reservation info
                // int resID = infoResponse.ValuesRead.Columns.Contains("reservationID") && row["reservationID"] != DBNull.Value
                //             ? Convert.ToInt32(row["reservationID"])
                //             : -1; // or any other default value you choose

                var location = infoResponse.ValuesRead.Rows[0]["CompanyAddress"].ToString();
                Console.WriteLine(location, reservationID);
                var spaceID = infoResponse.ValuesRead.Rows[0]["spaceID"].ToString();
                Console.WriteLine(spaceID);
                var companyName = infoResponse.ValuesRead.Rows[0]["CompanyName"].ToString();
                Console.WriteLine(companyName);
                //extract and handle reservation date and time 
                DateTime? start = null;
                if (row.Table.Columns.Contains("reservationStartTime") && row["reservationStartTime"] != DBNull.Value)
                {
                    start = DateTime.Parse(row["reservationStartTime"].ToString());
                }
                DateTime? end = null;
                if (row.Table.Columns.Contains("reservationEndTime") && row["reservationEndTime"] != DBNull.Value)
                {
                    end = DateTime.Parse(row["reservationEndTime"].ToString());
                }

                DateTime? date = start.Value;

                if (location == null) response.ErrorMessage = "The 'address' data was not found.";
                if (spaceID == null) response.ErrorMessage = "The 'spaceID' data was not found.";
                //if (resID == null) response.ErrorMessage = "The 'reservationID' data was not found.";
                if (companyName == null) response.ErrorMessage = "The 'CompanyName' data was not found.";
                if (date == null) response.ErrorMessage = "The 'reservationDate' data was not found.";
                if (start == null) response.ErrorMessage = "The 'reservationStartTime' data was not found.";
                if (end == null) response.ErrorMessage = "The 'reservationEndTime' data was not found.";


                //calendar ics creator
                var reservationInfo = new ReservationInfo
                {
                    filePath = icsFilePath,
                    eventName = "SpaceSurfer Reservation",
                    dateTime = date,
                    start = start,
                    end = end,
                    description = $"Reservation at: {companyName} ReservationID: {reservationID} SpaceID: {spaceID}",
                    location = location
                };
                var calendarCreator = new CalendarCreator();
                icsFilePath = await calendarCreator.CreateCalendar(reservationInfo);
                Console.WriteLine(icsFilePath);
                // var firstLine = File.ReadLines(icsFilePath).First();
                // if (firstLine != "BEGIN:VCALENDAR")
                // {
                //     // Log error or handle the situation when the file wasn't updated as expected
                //     Console.WriteLine("The ICS file was not updated correctly.");
                // }
                byte[]? fileBytes = await File.ReadAllBytesAsync(icsFilePath);
                if (string.IsNullOrEmpty(icsFilePath))
                {
                    // Handle the case where ICS file generation failed
                    response.HasError = true;
                    response.ErrorMessage += "Failed to generate the ICS file.";
                }
                // insert to db table
                response = await _emailDAO.InsertConfirmationInfo(reservationID, otp, fileBytes);
                if (response.HasError)
                {
                    response.HasError = true;
                    response.ErrorMessage += "Error inserting confirmation info into database.";
                }

                // create the email body with reservation details
                htmlBody = @"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Reservation Confirmation</title>
                </head>
                <body>
                    <p>Hello!</p>
                    <p>Thank you for reserving a space at SpaceSurfer! Here are the details of your reservation:</p>
                    <ul>
                        <li>Reservation at: <strong>{companyName}</strong></li>
                        <li>Location: <strong>{address}</strong></li>
                        <li>SpaceID: <strong>{spaceID}</strong></li>
                        <li>Date: <strong>{date}</strong></li>
                        <li>Start Time: <strong>{startTime}</strong></li>
                        <li>End Time: <strong>{endTime}</strong></li>
                    </ul>
                    <p>To confirm your reservation, please head over to SpaceSurfers --&gt; Your Reservations --&gt; Active Reservation, 
                    and confirm your Reservation with this code:</p>
                    <p><strong>{otp}</strong></p>
                    <p>Best,<br>PixelPals</p>
                </body>
                </html>";

                string? dateString = reservationInfo.dateTime?.ToString("MMMM d, yyyy");
                string? startTimeString = reservationInfo.start?.ToString("h:mm tt");
                string? endTimeString = reservationInfo.end?.ToString("h:mm tt"); 

                htmlBody = htmlBody.Replace("{companyName}", companyName)
                                    .Replace("{address}", reservationInfo.location)
                                    .Replace("{spaceID}", spaceID)
                                    .Replace("{date}", dateString)
                                    .Replace("{startTime}", startTimeString)
                                    .Replace("{endTime}", endTimeString)
                                    .Replace("{otp}", otp);

            }
            else
            {
                return (null, null, null, new Response { HasError = true, ErrorMessage = "Failed to retrieve reservation info." });
                // response.HasError = true;
                // response.ErrorMessage += "An error occured during reservation confirmation creation. Please try again later.";
            }

            return (icsFilePath, otp, htmlBody,response);
        }

        public async Task<(string ics, string otp, string body, Response res)> ResendConfirmation(int reservationID)
        {
            var response = new Response();
            //DataRow reservationInput = reservationInfo.ValuesRead.Rows[0];
            var reservationinfo = new ReservationInfo();
            var baseDirectory = AppContext.BaseDirectory;
            var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
            reservationinfo.filePath = Path.Combine(projectRootDirectory, "CalendarFiles", "SSReservation.ics");
            string newOtp = string.Empty;
            string icsFile = string.Empty;
            string htmlBody = string.Empty;
            
            // check confirmation status
            var statusResponse = await _emailDAO.GetConfirmInfo(reservationID);
            if (!statusResponse.HasError && statusResponse.ValuesRead != null && statusResponse.ValuesRead.Rows.Count > 0)
            {
                DataRow statusRow = statusResponse.ValuesRead.Rows[0];
                var reservationOtp = statusResponse.ValuesRead.Columns.Contains("reservationOTP") ? statusRow["reservationOTP"].ToString() : null;
                var confirmStatus = statusResponse.ValuesRead.Columns.Contains("confirmStatus") ? statusRow["confirmStatus"].ToString() : null;

                if (reservationOtp == null) response.ErrorMessage += "The 'reservationOtp' data was not found.";
                if (confirmStatus == null) response.ErrorMessage += "The 'confirmStatus' data was not found.";

                if (confirmStatus == "yes") 
                {
                    response.HasError = true;
                    response.ErrorMessage += "Unable to send confirmation Email. Reservation is already confirmed.";
                    //return (icsFile, otp, response);
                }

                //get reservation data
                var infoResponse = await _emailDAO.GetReservationInfo(reservationID);
                if (!infoResponse.HasError && infoResponse.ValuesRead != null && infoResponse.ValuesRead.Rows.Count > 0)
                {
                    DataRow row = infoResponse.ValuesRead.Rows[0];

                    var getOtp = new GenOTP();
                    newOtp = getOtp.generateOTP();

                    //extract reservation info
                    // int resID = infoResponse.ValuesRead.Columns.Contains("reservationID") && row["reservationID"] != DBNull.Value
                    //             ? Convert.ToInt32(row["reservationID"])
                    //             : -1; // or any other default value you choose

                    reservationinfo.location = infoResponse.ValuesRead.Columns.Contains("CompanyAddress") ? row["CompanyAddress"].ToString() : null;
                    var spaceID = infoResponse.ValuesRead.Columns.Contains("spaceID") ? row["spaceID"].ToString() : null;
                    var companyName = infoResponse.ValuesRead.Columns.Contains("CompanyName") ? row["CompanyName"].ToString() : null;
                    //extract and handle reservation date and time 
                    DateTime? start = null;
                    if (row.Table.Columns.Contains("reservationStartTime") && row["reservationStartTime"] != DBNull.Value)
                    {
                        start = DateTime.Parse(row["reservationStartTime"].ToString());
                    }
                    DateTime? end = null;
                    if (row.Table.Columns.Contains("reservationEndTime") && row["reservationEndTime"] != DBNull.Value)
                    {
                        end = DateTime.Parse(row["reservationEndTime"].ToString());
                    }

                    DateTime? date = start.Value;
                    
                    if (reservationinfo.location == null) response.ErrorMessage = "The 'address' data was not found.";
                    if (spaceID == null) response.ErrorMessage = "The 'spaceID' data was not found.";
                    //if (resID == null) response.ErrorMessage = "The 'reservationID' data was not found.";
                    if (companyName == null) response.ErrorMessage = "The 'CompanyName' data was not found.";
                    if (reservationinfo.dateTime == null) response.ErrorMessage = "The 'reservationDate' data was not found.";
                    if (reservationinfo.start == null) response.ErrorMessage = "The 'reservationStartTime' data was not found.";
                    if (reservationinfo.end == null) response.ErrorMessage = "The 'reservationEndTime' data was not found.";
                    

                    //calendar ics creator
                    var reservationInfo = new ReservationInfo
                    {
                        filePath = reservationinfo.filePath,
                        eventName = "SpaceSurfer Reservation",
                        dateTime = reservationinfo.dateTime,
                        start = reservationinfo.start,
                        end = reservationinfo.end,
                        description = $"Reservation at: {companyName} ReservationID: {reservationID} SpaceID: {spaceID}",
                        location = reservationinfo.location
                    };
                    var calendarCreator = new CalendarCreator();
                    icsFile = await calendarCreator.CreateCalendar(reservationInfo);
                    byte[]? fileBytes = await File.ReadAllBytesAsync(icsFile);
                    // if resending, update otp
                    if (reservationOtp != newOtp)
                    {
                        var otpResponse = await _emailDAO.UpdateOtp(reservationID, newOtp);
                        if (!otpResponse.HasError)
                        {
                            response.HasError = false;
                            response.ErrorMessage += $"Successfully updated reservation otp. {otpResponse.HasError}";
                        }
                    }
                    else
                    {
                        response.ErrorMessage += "Failed to generate new reservation otp.";
                    }
                    
                    // create the email body with reservation details
                    htmlBody = @"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Reservation Confirmation</title>
                    </head>
                    <body>
                        <p>Hello!</p>
                        <p>Thank you for reserving a space at SpaceSurfer! Here are the details of your reservation:</p>
                        <ul>
                            <li>Reservation at: <strong>{companyName}</strong></li>
                            <li>Location: <strong>{address}</strong></li>
                            <li>SpaceID: <strong>{spaceID}</strong></li>
                            <li>Date: <strong>{date}</strong></li>
                            <li>Start Time: <strong>{startTime}</strong></li>
                            <li>End Time: <strong>{endTime}</strong></li>
                        </ul>
                        <p>To confirm your reservation, head over to SpaceSurfer --&gt; Personal Overview, and confirm your Reservation with this code:</p>
                        <p><strong>{otp}</strong></p>
                        <p>Best,<br>PixelPals</p>
                    </body>
                    </html>";

                    string? dateString = reservationInfo.dateTime?.ToString("MMMM d, yyyy");
                    string? startTimeString = reservationInfo.start?.ToString("h:mm tt");
                    string? endTimeString = reservationInfo.end?.ToString("h:mm tt"); 

                    htmlBody = htmlBody.Replace("{companyName}", companyName)
                                        .Replace("{address}", reservationInfo.location)
                                        .Replace("{spaceID}", spaceID)
                                        .Replace("{date}", dateString)
                                        .Replace("{startTime}", startTimeString)
                                        .Replace("{endTime}", endTimeString)
                                        .Replace("{otp}", newOtp);

                }
                else
                {
                    response.HasError = true;
                    response.ErrorMessage += "Failed to get reservation data.";
                }
            }
            else
            {
                response.HasError = true;
                response.ErrorMessage += "Failed to check confirmation status.";
            }

            return (icsFile, newOtp, htmlBody, response);
        }

        public async Task<Response> ConfirmReservation(int reservationID, string otp)
        {
            var response = await _emailDAO.GetConfirmInfo(reservationID);
            DataRow statusRow = response.ValuesRead.Rows[0];
            var reservationOtp = response.ValuesRead.Columns.Contains("reservationOTP") ? statusRow["reservationOTP"].ToString() : null;
            var confirmStatus = response.ValuesRead.Columns.Contains("confirmStatus") ? statusRow["confirmStatus"].ToString() : null;

            if (reservationOtp == null) response.ErrorMessage += "The 'reservationOtp' data was not found.";
            if (confirmStatus == null) response.ErrorMessage += "The 'confirmStatus' data was not found.";

            if (confirmStatus == "yes") 
            {
                response.HasError = true;
                response.ErrorMessage += "Unable to confirmation Email. Reservation is already confirmed.";
                return response;
            }
            else
            {
                if (!response.HasError && response.ValuesRead != null && response.ValuesRead.Rows.Count > 0)
                {
                    if (otp == reservationOtp)
                    {
                        var updateResponse = await _emailDAO.UpdateConfirmStatus(reservationID);
                        if (updateResponse.HasError)
                        {
                            response.HasError = true;
                            response.ErrorMessage = "Failed to update confirmation status.";
                        }
                    }
                    else
                    {
                        response.HasError = true;
                        response.ErrorMessage = "OTP does not match. Please try again.";
                    }
                }
                else
                {
                    response.HasError = true;
                    response.ErrorMessage = "Failed to retrieve reservation confirmation info. Please try again later.";
                }
            }
            return response;
        }
    }
}