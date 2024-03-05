﻿//using SS.Backend.DataAccess;
//using SS.Backend.SharedNamespace;

//namespace SS.Backend.Security.AuthN
//{
//    public class Authenticator : IAuthenticator
//    {
//        private readonly GenOTP genotp;
//        private readonly Hashing hasher;
//        private readonly SqlDAO sqldao;

//        public Authenticator(GenOTP genotp, Hashing hasher, SqlDAO sqldao)
//        {
//            this.genotp = genotp;
//            this.hasher = hasher;
//            this.sqldao = sqldao;
//        }
//        public async Task<(string otp, Response res)> SendOTP_and_SaveToDB(AuthenticationRequest authRequest)
//        {
//            var builder = new CustomSqlCommandBuilder();
//            Response result = new Response();
//            string user = authRequest.UserIdentity;

//            try
//            {
//                // check if user is registered, ie if user has a role
//                var selectCommand = builder
//                    .BeginSelectAll()
//                    .From("UserRoles")
//                    .Where($"Username = '{user}'")
//                    .Build();
//                result = await sqldao.ReadSqlResult(selectCommand).ConfigureAwait(false);
//                if (result.ValuesRead == null || result.ValuesRead.Rows.Count == 0)
//                {
//                    result.HasError = true;
//                    result.ErrorMessage = "User does not exist.";
//                    return (null, result);
//                }

//                // generate the otp and salt
//                string otp = genotp.generateOTP();
//                string salt = hasher.GenSalt();

//                // hash the otp
//                string hashedOTP = hasher.HashData(otp, salt);

//                // check if username already tried authenticating before, ie username exists in otp table
//                var otpRead = builder
//                    .BeginSelectAll()
//                    .From("OTP")
//                    .Where($"Username = '{user}'")
//                    .Build();
//                result = await sqldao.ReadSqlResult(otpRead).ConfigureAwait(false);
//                if (result.ValuesRead == null || result.ValuesRead.Rows.Count == 0)
//                {
//                    // create and execute the sql command to insert the username, otp, salt, and timestamp to the DB
//                    var parameters = new Dictionary<string, object>
//                    {
//                        { "OTP", hashedOTP },
//                        { "Salt", salt },
//                        { "Timestamp", DateTime.UtcNow },
//                        { "Username", user }
//                    };
//                    var insertCommand = builder
//                        .BeginInsert("OTP")
//                        .Columns(parameters.Keys)
//                        .Values(parameters.Keys)
//                        .AddParameters(parameters)
//                        .Build();
//                    result = await sqldao.SqlRowsAffected(insertCommand).ConfigureAwait(false);

//                    // user and otp is returned so it can be used by the manager to send the otp to the user
//                    return (otp, result);
//                }
//                else
//                {
//                    var parameters = new Dictionary<string, object>
//                    {
//                        { "OTP", hashedOTP },
//                        { "Salt", salt },
//                        { "Timestamp", DateTime.UtcNow },
//                        { "Username", user }
//                    };
//                    var updateCommand = builder
//                        .BeginUpdate("OTP")
//                        .Set(parameters)
//                        .Where("Username = @username")
//                        .AddParameters(parameters)
//                        .Build();

//                    //update the otp and salt in table for that user
//                    result = await sqldao.SqlRowsAffected(updateCommand).ConfigureAwait(false);

//                    // user and otp is returned so it can be used by the manager to send the otp to the user
//                    return (otp, result);
//                }
//            }
//            catch (Exception ex)
//            {
//                result.HasError = true;
//                result.ErrorMessage = ex.Message;
//                return (null, result);
//            }
//        }
//        public async Task<(SSPrincipal principal, Response res)> Authenticate(AuthenticationRequest authRequest)
//        {
//            var builder = new CustomSqlCommandBuilder();
//            Response result = new Response();

//            #region Validate arguments
//            if (authRequest is null)
//            {
//                throw new ArgumentNullException(nameof(authRequest));
//            }

//            if (System.String.IsNullOrWhiteSpace(authRequest.UserIdentity))
//            {
//                throw new ArgumentException($"{nameof(authRequest.UserIdentity)} must be valid");
//            }

//            if (System.String.IsNullOrWhiteSpace(authRequest.Proof))
//            {
//                throw new ArgumentException($"{nameof(authRequest.UserIdentity)} must be valid");
//            }
//            #endregion

//            string hashedUsername = authRequest.UserIdentity;
//            string proof = authRequest.Proof;

//            try
//            {
//                // create and execute sql command to read the hashedOTP from the DB
//                var selectCommand = builder
//                    .BeginSelectAll()
//                    .From("OTP")
//                    .Where($"Username = '{hashedUsername}'")
//                    .Build();
//                result = await sqldao.ReadSqlResult(selectCommand).ConfigureAwait(false);
//                string dbOTP = (string)result.ValuesRead[0][1];
//                string dbSalt = (string)result.ValuesRead[0][2];
//                DateTime timestamp = (DateTime)result.ValuesRead[0][3];
//                TimeSpan timeElapsed = DateTime.UtcNow - timestamp;

//                // compare the otp stored in DB with user inputted otp
//                string HashedProof = hasher.HashData(proof, dbSalt);
//                if (dbOTP == HashedProof)
//                {
//                    if (timeElapsed.TotalMinutes > 2)
//                    {
//                        result.HasError = true;
//                        result.ErrorMessage = "OTP has expired.";
//                        return (null, result);
//                    }
//                    else
//                    {
//                        // they match and not expired, so get the roles from the DB for that user
//                        var readRoles = builder
//                            .BeginSelectAll()
//                            .From("UserRoles")
//                            .Where($"Username = '{hashedUsername}'")
//                            .Build();
//                        result = await sqldao.ReadSqlResult(readRoles).ConfigureAwait(false);
//                        if (result.ValuesRead.Count > 0)
//                        {
//                            string roles = (string)result.ValuesRead[0][1];

//                            // populate the principal
//                            SSPrincipal principal = new SSPrincipal();
//                            principal.UserIdentity = hashedUsername;
//                            principal.Claims.Add("Roles", roles);

//                            result.HasError = false;

//                            return (principal, result);
//                        }
//                        else
//                        {
//                            result.HasError = true;
//                            result.ErrorMessage = "No roles found for the user.";
//                            return (null, result);
//                        }
//                    }

//                }
//                else
//                {
//                    result.HasError = true;
//                    result.ErrorMessage = "Failed to authenticate.";
//                    return (null, result);
//                }
//            }
//            catch (Exception ex)
//            {
//                result.ErrorMessage = ex.Message;
//                return (null, result);
//            }

//        }
//    }
//}
