
// using SS.Backend.DataAccess;
// using System.IO;
// using System.Threading.Tasks;
// using SS.Backend.UserManagement;
// using Microsoft.Data.SqlClient;
// using System.Data;

// namespace SS.Backend.Tests.UserManagement;

// [TestClass]
// public class AccountRecoveryTests
// {
//    private AccountRecovery _accountRecovery;
//     private AccountDisabler _accountDisabler;
//     private IUserManagementDao _userManagementDao;
//     private IAccountRecoveryModifier _accountRecoveryModifier;
//     private SqlDAO _sqlDao;
//     private ConfigService _configService;




//    [TestInitialize]
//    public void Setup()
//    {

//         var baseDirectory = AppContext.BaseDirectory;
//         var projectRootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../"));
//         var configFilePath = Path.Combine(projectRootDirectory, "Configs", "config.local.txt");
//         _configService = new ConfigService(configFilePath);
//         _sqlDao = new SqlDAO(_configService);
//         _userManagementDao = new UserManagementDao(_sqlDao);
//         _accountRecoveryModifier = new AccountRecoveryModifier(_userManagementDao);

//        _accountRecovery = new AccountRecovery(_accountRecoveryModifier, _userManagementDao);
//        _accountDisabler = new AccountDisabler(_userManagementDao);
//    }

//    [TestMethod]
//    public async Task SendInitialRecoveryRequest_Pass()
//    {
//        // Arrange
//        var userHash = "testUsername";

//        // Act
//        var response = await _accountRecovery.createRecoveryRequest(userHash);

//        // Assert

//        Assert.IsFalse(response.HasError);

//    }

//    [TestMethod]
//    public async Task RecoverAccount_AdminDecision_True_ShouldPass()
//    {
//        // Arrange
//        var userHash = "testUserHash4";
//        bool adminDecision = true; 

//        // Act
//        var response = await _accountRecovery.RecoverAccount(userHash, adminDecision);

//        // Assert
//        Assert.IsFalse(response.HasError);
//    }


//    [TestMethod]
//    public async Task SendRecoveryRequest_AlreadyActiveAccount_ShouldPass()
//    {
//        // Arrange

//         var userHash = "testUserHash1";

//        var response = await _accountRecovery.createRecoveryRequest(userHash);


//        // Act
//        response = await _accountRecovery.createRecoveryRequest(userHash);

//        // Assert
//        Assert.IsFalse(response.HasError); 
//    }

//    [TestMethod]
//    public async Task RecoverAccount_NonExistentUser_ShouldFail()
//    {
//        // Arrange
//        var userHash = "nonExistentUserHash";
//        bool adminDecision = true; 

//        // Act
//        var response = await _accountRecovery.RecoverAccount(userHash, adminDecision);

//        // Assert
//        Console.Write(response.ErrorMessage);
//        Assert.IsTrue(response.HasError);

//    }

//    [TestMethod]
//    public async Task RecoverAccount_WithoutAdminApproval_ShouldFail()
//    {
//        // Arrange
//        var userHash = "someUserHash";
//        bool adminDecision = false; 

//        // Act
//        var response = await _accountRecovery.RecoverAccount(userHash, adminDecision);

//        // Assert
//        Assert.IsTrue(response.HasError);
//        Assert.AreEqual("Recovery request denied by admin.", response.ErrorMessage); 
//    }


//    [TestMethod]
//    public async Task SendRecoveryRequest_MultipleConcurrentRequests_ShouldPass()
//    {
//        // Arrange
//        var userHash = "userForConcurrentTesting";

//        // Act
//        var task1 = _accountRecovery.createRecoveryRequest(userHash);
//        var task2 = _accountRecovery.createRecoveryRequest(userHash);
//        var task3 = _accountRecovery.createRecoveryRequest(userHash);

//        // Await all tasks to complete
//        var responses = await Task.WhenAll(task1, task2, task3);

//        // Assert

//        foreach (var response in responses)
//        {
//            Assert.IsFalse(response.HasError);

//        }
//    }

//    [TestMethod]
//    public async Task CreateRecoveryRequest_ValidUserHash_ShouldSucceed()
//    {
//        // Arrange
//        var userHash = "userHash1";
//        string additionalInfo = "Need to recover account due to forgotten password.";

//        // Actx
//        var response = await _accountRecovery.createRecoveryRequest(userHash, additionalInfo);

//        // Assert
//        Console.Write(response.ErrorMessage);
//        Assert.IsFalse(response.HasError);
//    }
// }
