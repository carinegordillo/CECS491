using SS.Backend.DataAccess;
using SS.Backend.SharedNamespace;
using System.Data.SqlClient;
using SS.Backend.Services.AccountCreationService;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SS.Backend.Tests.AccountCreationTest
{
    [TestClass]
    public class AccountCreationTests
    {
        [TestMethod]
        public async Task CreateUserAccount_Success()
        {
            // AccountCreation accountcreation = new AccountCreation(SqlDAO sqlDao, ICustomSqlCommandBuilder commandBuilder);
            UserInfo userInfo = new UserInfo();
            AccountCreation accountcreation = new AccountCreation(userInfo);
            
            var validUserInfo = new UserInfo{
                username = "mshowing@example.com",
                dob = new DateTime(1990, 1, 1),
                firstname = "Jane",
                lastname = "Doe", 
                role = 5,
                status = "yes", 
                backupEmail = "test@backup.com"
            };
    
            var userAccount_success_parameters = new Dictionary<string, object>
            {
                { "username", validUserInfo.username},
                {"birthDate", validUserInfo.dob}   
            };

            var userProfile_success_parameters = new Dictionary<string, object>
            {
                {"hashedUsername", "newnew"},
                { "FirstName", validUserInfo.firstname},
                { "LastName", validUserInfo.lastname}, 
                {"backupEmail", validUserInfo.backupEmail},
                {"appRole", validUserInfo.role}, 
            };
            
            var activeAccount_success_parameters = new Dictionary<string, object>
            {
                {"hashedUsername", "newnew"},
                {"isActive", validUserInfo.status} 
            };

            var tableData = new Dictionary<string, Dictionary<string, object>>
            {
                { "userAccount", userAccount_success_parameters },
                { "userProfile", userProfile_success_parameters },
                { "activeAccount", activeAccount_success_parameters}
            };

            var response = await accountcreation.CreateUserAccount(validUserInfo, tableData);
        //    Assert.IsFalse(response.HasError, response.ErrorMessage);
            Assert.IsFalse(response.HasError, response.ErrorMessage);
        }
    }
}