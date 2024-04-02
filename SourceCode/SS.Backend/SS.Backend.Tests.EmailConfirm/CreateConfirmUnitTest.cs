using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.Backend.EmailConfirm;
using SS.Backend.SharedNamespace;
using SS.Backend.Services.CalendarService;
using SS.Backend.DataAccess;
using System.Data;
using System.Threading.Tasks;
using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace SS.Backend.Tests.EmailConfirm;

[TestClass]
public class CreateConfirmUnitTest
{
    private EmailConfirmService _emailConfirm;
    private IEmailConfirmDAO _emailDAO;
    private SqlDAO _sqlDao;
    private ConfigService _configService;

    [TestInitialize]
    public void Setup()
    {
        
        var baseDirectory = AppContext.BaseDirectory;
        var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
        var configFilePath = Path.Combine(projectRootDirectory, "Configs", "config.local.txt");
        _configService = new ConfigService(configFilePath);
        _sqlDao = new SqlDAO(_configService);
        _emailDAO = new EmailConfirmDAO(_sqlDao);
        _emailConfirm = new EmailConfirmService(_emailDAO);
    }

    private async Task CleanupTestData()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
        var configFilePath = Path.Combine(projectRootDirectory, "Configs", "config.local.txt");

        ConfigService configFile = new ConfigService(configFilePath);
        var connectionString = configFile.GetConnectionString();
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                string sql1 = $"DELETE FROM dbo.ConfirmReservations WHERE [reservationID] = '6'";
                //string sql2 = $"DELETE FROM dbo.Reservations WHERE [spaceID] = 'SPACE103'";

                using (SqlCommand command1 = new SqlCommand(sql1, connection))
                {
                    await command1.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            //     using (SqlCommand command2 = new SqlCommand(sql2, connection))
            //     {
            //         await command2.ExecuteNonQueryAsync().ConfigureAwait(false);
            //     }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during test cleanup: {ex}");
        }
    }

    [TestMethod]
    public async Task InsertConfirmInfo_Success()
    {
        //Arrange
        Stopwatch timer = new Stopwatch();
        Response result = new Response();
        int reservationID = 6;
        var response = new Response();
        var baseDirectory = AppContext.BaseDirectory;
        var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
        var calendarFilePath = Path.Combine(projectRootDirectory, "CalendarFiles", "SSReservation.ics");
        string otp = string.Empty;
        string icsFile = string.Empty;
        byte[] fileBytes = null;

        var infoResponse = await _emailDAO.GetReservationInfo(reservationID);
        if (!infoResponse.HasError && infoResponse.ValuesRead != null && infoResponse.ValuesRead.Rows.Count > 0)
        {
            DataRow row = infoResponse.ValuesRead.Rows[0];

            var getOtp = new GenOTP();
            otp = getOtp.generateOTP();

            //extract reservation info
            // int resID = infoResponse.ValuesRead.Columns.Contains("reservationID") && row["reservationID"] != DBNull.Value
            //             ? Convert.ToInt32(row["reservationID"])
            //             : -1; // or any other default value you choose

            var address = infoResponse.ValuesRead.Columns.Contains("CompanyAddress") ? row["CompanyAddress"].ToString() : null;
            var spaceID = infoResponse.ValuesRead.Columns.Contains("spaceID") ? row["spaceID"].ToString() : null;
            var companyName = infoResponse.ValuesRead.Columns.Contains("CompanyName") ? row["CompanyName"].ToString() : null;
            //extract and handle reservation date and time 
            var date = row.Table.Columns.Contains("reservationDate") ? Convert.ToDateTime(row["reservationDate"]) : (DateTime?)null;
            var startTime = row.Table.Columns.Contains("reservationStartTime") ? (DateTime?)DateTime.Parse(date?.ToShortDateString() + " " + row["reservationStartTime"].ToString()) : null;
            var endTime = row.Table.Columns.Contains("reservationEndTime") ? (DateTime?)DateTime.Parse(date?.ToShortDateString() + " " + row["reservationEndTime"].ToString()) : null;

            
            if (address == null) response.ErrorMessage = "The 'address' data was not found.";
            if (spaceID == null) response.ErrorMessage = "The 'spaceID' data was not found.";
            //if (resID == null) response.ErrorMessage = "The 'reservationID' data was not found.";
            if (companyName == null) response.ErrorMessage = "The 'CompanyName' data was not found.";
            if (date == null) response.ErrorMessage = "The 'reservationDate' data was not found.";
            if (startTime == null) response.ErrorMessage = "The 'reservationStartTime' data was not found.";
            if (endTime == null) response.ErrorMessage = "The 'reservationEndTime' data was not found.";
            

            //calendar ics creator
            var reservationInfo = new ReservationInfo
            {
                filePath = calendarFilePath,
                eventName = "SpaceSurfer Reservation",
                dateTime = date,
                start = startTime,
                end = endTime,
                description = $"Reservation at: {companyName} \nReservationID: {reservationID} \nSpaceID: {spaceID}",
                location = address
            };
            var calendarCreator = new CalendarCreator();
            icsFile = await calendarCreator.CreateCalendar(reservationInfo);
            fileBytes = await File.ReadAllBytesAsync(icsFile);
        }

        //Act
        timer.Start();
        result = await _emailDAO.InsertConfirmationInfo(reservationID, otp, fileBytes);
        timer.Stop();

        //Assert
        Assert.IsFalse(result.HasError, result.ErrorMessage);
        Assert.IsNotNull(fileBytes);
        Assert.IsNotNull(otp);
        Assert.IsTrue(timer.ElapsedMilliseconds <= 3000);

        //Cleanup
        await CleanupTestData().ConfigureAwait(false);

    }

    [TestMethod]
    public async Task CreateConfirm_Success()
    {
        //Arrange
        Stopwatch timer = new Stopwatch();
        Response result = new Response();
        int reservationID = 6;

        //Act
        timer.Start();
        (string icsFile, string otp, result) = await _emailConfirm.CreateConfirmation(reservationID);
        //result = await _emailDAO.InsertConfirmationInfo(reservationID, otp, fileBytes);
        timer.Stop();

        //Assert
        Assert.IsFalse(result.HasError, result.ErrorMessage);
        Assert.IsNotNull(icsFile);
        Assert.IsNotNull(otp);
        Assert.IsTrue(timer.ElapsedMilliseconds <= 3000);

        //Cleanup
        await CleanupTestData().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CreateConfirm_InvalidInputs_Fail()
    {
        //Arrange
        Stopwatch timer = new Stopwatch();
        Response result = new Response();
        int reservationID = -1;

        //Act
        timer.Start();
        (string icsFile, string otp, result) = await _emailConfirm.CreateConfirmation(reservationID);
        timer.Stop();

        //Assert
        Assert.IsTrue(result.HasError, "Expected CreateConfirmation to fail with invalid input.");
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage), "Expected an error message for invalid input.");
        Assert.IsNotNull(icsFile);
        Assert.IsNotNull(otp);
        Assert.IsTrue(timer.ElapsedMilliseconds <= 3000);

        //Cleanup
        await CleanupTestData().ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CreateConfirm_Timeout_Fail()
    {
        //Arrange
        int reservationID = 6;
        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(3000));

        //Act
        var operationTask = _emailConfirm.CreateConfirmation(reservationID);
        var completedTask = await Task.WhenAny(operationTask, timeoutTask);

        // Assert
        if (completedTask == operationTask)
        {
            // Operation completed before timeout, now it's safe to await it and check results
            (string fileBytes, string otp, Response response) = await operationTask;

            // Assert the operation's success
            Assert.IsFalse(response.HasError, response.ErrorMessage);
            Assert.IsNotNull(otp);
        }
        else
        {
            // Fail the test if we hit the timeout
            Assert.Fail("The CreateConfirmation operation timed out.");
        }

        //Cleanup
        await CleanupTestData().ConfigureAwait(false);
    }

    // [TestMethod]
    // public async Tas CreateConfirm_DBRetrieval_Fail()
    // {

    // }
}