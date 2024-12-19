using Container_App.Data;
using Container_App.Model.Users;
using Container_App.utilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Container_App.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlQueryHelper _sqlQueryHelper;
        private readonly Config _config;

        public UserRepository(SqlQueryHelper sqlQueryHelper, Config config)
        {
            _sqlQueryHelper = sqlQueryHelper;
            _config = config;
        }


        public async Task<List<Users>> GetUsers(PagedResult page)
        {
            try
            {
                string sqlQuery = @"
                SELECT * FROM Users 
                WHERE (@SearchTerm IS NULL OR FullName ILIKE '%' || @SearchTerm || '%')
                AND IsDel = 0
                ORDER BY UserId
                OFFSET @Offset LIMIT @PageSize";


                var parameters = new[]
                {
                new NpgsqlParameter("@SearchTerm", page.SearchTerm ?? (object)DBNull.Value),
                new NpgsqlParameter("@Offset", (page.PageNumber - 1) * page.PageSize),
                new NpgsqlParameter("@PageSize", page.PageSize),
             };
                return await _sqlQueryHelper.ExecuteQueryAsync<Users>(sqlQuery, parameters);
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public async Task<Users> GetUserById(int id)
        {
            string sql = "SELECT * FROM \"Users\" WHERE \"UserId\" = @UserId";

            // Tạo NpgsqlParameter cho UserId
            var parameters = new[]
            {
                new NpgsqlParameter("@UserId", id)
            };

            return await _sqlQueryHelper.ExecuteQuerySingleAsync<Users>(sql, parameters);
        }

        public async Task<int> CreateUser(Users user)
        {
            // Mã hóa mật khẩu người dùng
            user.Password = _config.HashPassword(user.Password);

            // Lấy ID lớn nhất và cộng thêm 1 cho UserId mới
            string sqlGetMaxId = "SELECT COALESCE(MAX(\"UserId\"), 0) + 1 FROM public.\"Users\";";
            user.UserId = await _sqlQueryHelper.ExecuteScalarAsync<int>(sqlGetMaxId);

            // Câu lệnh INSERT
            string sqlInsert = @"
            INSERT INTO public.""Users"" (""UserId"", ""Username"", ""Password"", ""FullName"", ""IsDel"", ""Address"", ""Email"", ""Phone"")
            VALUES(@UserId, @Username, @Password, @FullName, @IsDel, @Address, @Email, @Phone);";

            // Tạo NpgsqlParameter cho các tham số trong câu lệnh INSERT
            var parameters = new[]
            {
                new NpgsqlParameter("@UserId", user.UserId),
                new NpgsqlParameter("@Username", user.Username),
                new NpgsqlParameter("@Password", user.Password),
                new NpgsqlParameter("@FullName", user.FullName),
                new NpgsqlParameter("@IsDel", user.IsDel),
                new NpgsqlParameter("@Address", user.Address),
                new NpgsqlParameter("@Email", user.Email),
                new NpgsqlParameter("@Phone", user.Phone),
            };

            // Thực thi câu lệnh INSERT
            int rowsAffected = await _sqlQueryHelper.ExecuteNonQueryAsync(sqlInsert, parameters);
            return rowsAffected; // Trả về số bản ghi bị ảnh hưởng
        }


        public async Task<int> UpdateUser(Users user)
        {
            string sql = @"
                    UPDATE public.""Users""
                    SET ""FullName"" = @FullName,
                        ""Address"" = @Address,
                        ""Email"" = @Email,
                        ""Phone"" = @Phone
                    WHERE ""UserId"" = @UserId";

            // Tạo SqlParameter cho các tham số trong câu lệnh UPDATE
            var parameters = new[]
            {
                new NpgsqlParameter("@UserId", user.UserId),
                new NpgsqlParameter("@FullName", user.FullName),
                new NpgsqlParameter("@Address", user.Address),
                new NpgsqlParameter("@Email", user.Email),
                new NpgsqlParameter("@Phone", user.Phone),
            };

            int rowsAffected = await _sqlQueryHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected; // Trả về số bản ghi bị ảnh hưởng
        }

        public async Task<int> DeleteUser(int id)
        {
            string sql = @"
                    UPDATE  public.""Users""
                    SET ""IsDel"" = true
                    WHERE ""UserId"" = @UserId";

            // Tạo NpgsqlParameter cho UserId
            var parameters = new[]
            {
                new NpgsqlParameter("@UserId", id)
            };

            int rowsAffected = await _sqlQueryHelper.ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected; // Trả về số bản ghi bị ảnh hưởng
        }

        public Task<long> CheckAdmin(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> CountRecord()
        {
            string sql = @"select count(*) from ""Users"" where ""IsDel"" = false";
            return await _sqlQueryHelper.ExecuteScalarAsync<int>(sql);
        }
    }
}
