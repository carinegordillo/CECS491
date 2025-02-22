


//using SS.Backend.DataAccess;
//using System.IO;
//using System.Threading.Tasks;
//using SS.Backend.Waitlist;
//using SS.Backend.ReservationManagement;
//using SS.Backend.SharedNamespace;
//using Microsoft.Data.SqlClient;
//using SS.Backend.Services.LoggingService;

//using System.Data;
//namespace SS.Backend.Tests.ReservationManagement{

//   [TestClass]
//   public class ReservationReadServiceUnitTests
//   {
//       private SqlDAO _sqlDao;
//       private ConfigService _configService;
//       private ReservationCreatorService  _reservationCreatorService;

//       private ReservationManagementRepository _reservationManagementRepository;

//       private ReservationReadService _reservationReadService;
//       private ILogTarget _logTarget;
//       private  ILogger _logger;
//       private WaitlistService waitlistService;
       
       
        

//       string MANUAL_ID_TABLE = "dbo.NewManualIDReservations";
//       string USER_HASH = "Yu86Ho6KDmtOeP687I/AHNE4rhxoCzZDs9v/Mpe+SZw=";

//       [TestInitialize]
//       public void Setup()
//       {

//           var baseDirectory = AppContext.BaseDirectory;
//           var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
//           var configFilePath = Path.Combine(projectRootDirectory, "Configs", "config.local.txt");
//           _configService = new ConfigService(configFilePath);
//           _sqlDao = new SqlDAO(_configService);
//           _logger = new Logger(_logTarget);
//           waitlistService = new WaitlistService(_sqlDao);

    
//           _reservationManagementRepository = new ReservationManagementRepository(_sqlDao);


//           _reservationReadService = new ReservationReadService(_reservationManagementRepository,_logger);

//           _reservationCreatorService= new ReservationCreatorService(_reservationManagementRepository, waitlistService);

//       }

        
//       [TestMethod]
//       public async Task ReadAllUserReservations_Pass()
//       {
//           Response reservationCreationResult = new Response();
//           Response reservationReadResult = new Response();
           
//           UserReservationsModel userReservationsModel1 = new UserReservationsModel
//           {
//               ReservationID = 6001,
//               CompanyID = 1,
//               FloorPlanID = 4,
//               SpaceID = "S2-FP4",
//               ReservationStartTime = new DateTime(2025, 01, 01, 13, 00, 00), 
//               ReservationEndTime = new DateTime(2025, 01, 01, 15, 00, 00), 
//               Status = ReservationStatus.Active,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel1);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel2 = new UserReservationsModel
//           {
//               ReservationID = 6002,
//               CompanyID = 1,
//               FloorPlanID = 4,
//               SpaceID = "S2-FP4",
//               ReservationStartTime = new DateTime(2024, 03, 07, 10, 00, 00), 
//               ReservationEndTime = new DateTime(2024, 03, 07, 11, 00, 00), 
//               Status = ReservationStatus.Active,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel2);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel3 = new UserReservationsModel
//           {
//               ReservationID = 6003,
//               CompanyID = 1,
//               FloorPlanID = 4,
//               SpaceID = "S2-FP4",
//               ReservationStartTime = new DateTime(2024, 01, 01, 14, 00, 00), 
//               ReservationEndTime = new DateTime(2024, 01, 01, 15, 00, 00), 
//               Status = ReservationStatus.Active,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel3);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           reservationReadResult = await _reservationReadService.GetAllUserReservations(MANUAL_ID_TABLE , USER_HASH);

//           var expectedIds = new HashSet<int> { 6001, 6002, 6003 }; // IDs of the created reservations
//           var actualIds = new HashSet<int>();

//           foreach (DataRow row in reservationReadResult.ValuesRead.Rows)
//           {
//               actualIds.Add((int)row["ReservationID"]); // Assuming "ReservationID" is the column name
//           }

//           Assert.IsTrue(expectedIds.SetEquals(actualIds));

//       }


//       [TestMethod]
//       public async Task ReadUserActiveReservations_Pass()
//       {
//           Response reservationCreationResult = new Response();
//           Response reservationReadResult = new Response();
//           UserReservationsModel userReservationsModel1 = new UserReservationsModel
//           {
//               ReservationID = 7001,
//               CompanyID = 1,
//               FloorPlanID = 4,
//               SpaceID = "S5-FP4",
//               ReservationStartTime = new DateTime(2025, 01, 01, 13, 00, 00), 
//               ReservationEndTime = new DateTime(2025, 01, 01, 15, 00, 00), 
//               Status = ReservationStatus.Active,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel1);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel2 = new UserReservationsModel
//           {
//               ReservationID = 7002,
//               CompanyID = 1,
//               FloorPlanID = 4,
//               SpaceID = "S5-FP4",
//               ReservationStartTime = new DateTime(2024, 03, 01, 10, 00, 00), 
//               ReservationEndTime = new DateTime(2024, 03, 01, 11, 00, 00), 
//               Status = ReservationStatus.Cancelled,
//               UserHash = USER_HASH
//           };

            
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel2);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel5 = new UserReservationsModel
//           {
//               ReservationID = 6010,
//               CompanyID = 2,
//               FloorPlanID = 2,
//               SpaceID = "S5-FP2",
//               ReservationStartTime = new DateTime(2024, 01, 01, 14, 00, 00), 
//               ReservationEndTime = new DateTime(2024, 01, 01, 15, 00, 00), 
//               Status = ReservationStatus.Passed,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel5);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel3 = new UserReservationsModel
//           {
//               ReservationID = 7003,
//               CompanyID = 2,
//               FloorPlanID = 2,
//               SpaceID = "S5-FP2",
//               ReservationStartTime = new DateTime(2022, 01, 01, 11, 00, 00), 
//               ReservationEndTime = new DateTime(2022, 01, 01, 12, 00, 00), 
//               Status = ReservationStatus.Passed,
//               UserHash = USER_HASH
//           };
//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel3);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           UserReservationsModel userReservationsModel4 = new UserReservationsModel
//           {
//               ReservationID = 7004,
//               CompanyID = 2,
//               FloorPlanID = 2,
//               SpaceID = "S4-FP2",
//               ReservationStartTime = new DateTime(2027, 01, 01, 13, 00, 00), 
//               ReservationEndTime = new DateTime(2027, 01, 01, 15, 00, 00), 
//               Status = ReservationStatus.Active,
//               UserHash = USER_HASH
//           };

//           reservationCreationResult = await _reservationCreatorService.CreateReservationWithManualIDAsync(MANUAL_ID_TABLE,userReservationsModel4);
//           Assert.IsFalse(reservationCreationResult.HasError);

//           reservationReadResult = await _reservationReadService.GetUserActiveReservations(MANUAL_ID_TABLE , USER_HASH);

//           var expectedIds = new HashSet<int> { 7004, 7001 }; 
//           var actualIds = new HashSet<int>();

//           foreach (DataRow row in reservationReadResult.ValuesRead.Rows)
//           {
//               actualIds.Add((int)row["reservationID"]);
                
//           }
//           Console.WriteLine("Expected IDs: " + string.Join(",", expectedIds));
//           Console.WriteLine("Actual IDs: " + string.Join(",", actualIds));
            
//           Console.WriteLine(reservationReadResult.ErrorMessage);

//           Assert.IsTrue(expectedIds.SetEquals(actualIds));

//       }

        
        
        
        
//       [TestCleanup]
//       public void Cleanup()
//       {

//           var testReservtaionIds = new List<int> { 6001, 6002, 6003,6010, 7001,7002,7003,7004 };
//           var commandBuilder = new CustomSqlCommandBuilder();

//           var deleteCommand = commandBuilder.BeginDelete(MANUAL_ID_TABLE)
//                                           .Where($"reservationID IN ({string.Join(",", testReservtaionIds)})")
//                                           .Build();
                                            
//           _sqlDao.SqlRowsAffected(deleteCommand);

//       }
//   }
    
//}