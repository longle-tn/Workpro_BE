using Container_App.Common.Shared;
using Container_App.Core.Interface.Users;
using Container_App.Core.Model.Users;
using Container_App.Data.Connection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Service.Services.Users
{
    public class UserServices : IUserServices
    {
        private readonly IStoredProcedureExecutor _executor;
        public UserServices(IStoredProcedureExecutor executor)
        {
            _executor = executor;
        }
        
        public async Task<int> Insert(UserProfile user)
        {
            try
            {
                string hash = Hash.HashPassword(user.Password);
                var arr = new[]
                {
                    new SqlParameter("@Username", user.UserName),
                    new SqlParameter("@Password", hash),
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@Phone", user.Phone),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@Address", user.Address),
                    new SqlParameter("@CreateBy", (object?)user.CreateBy ?? DBNull.Value)
                };
                return await _executor.ExecuteAsync("sp_CreateUser", arr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message,
                "Error when creating user. Username={Username}, Email={Email}",
                user.UserName,
                user.Email);

                throw;
            }
        }
    }
}
