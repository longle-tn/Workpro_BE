using Container_App.Data;
using Container_App.Model.ProjectUsers;
using Container_App.utilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Container_App.Repository.ProjectUserRepository
{
    public class ProjectUserRepository : IProjectUserRepository
    {
        private readonly SqlQueryHelper _sqlQueryHelper;
        public ProjectUserRepository(SqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }
        public async Task AddUserToProjectAsync(ProjectUser projectUser)
        {
            string sql = @"
                INSERT INTO ""ProjectUser"" (""ProjectId"", ""UserId"", ""Role"", ""JoinedAt"")
                VALUES (@ProjectId, @UserId, @Role, @JoinedAt)";

            var parameters = new[]
            {
                new NpgsqlParameter("@ProjectId", projectUser.ProjectId),
                new NpgsqlParameter("@UserId", projectUser.UserId),
                new NpgsqlParameter("@Role", projectUser.Role),
                new NpgsqlParameter("@JoinedAt", projectUser.JoinedAt)
            };

            await _sqlQueryHelper.ExecuteNonQueryAsync(sql, parameters);
        }



        public async Task<bool> IsUserInProjectAsync(int projectId, int userId)
        {
            string sql = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM ""ProjectUsers""
                    WHERE ""ProjectId"" = @ProjectId AND ""UserId"" = @UserId
                )";

            var parameters = new[]
            {
                new NpgsqlParameter("@ProjectId", projectId),
                new NpgsqlParameter("@UserId", userId)
            };

            var result = await _sqlQueryHelper.ExecuteScalarAsync<bool>(sql, parameters);
            return result;
        }

    }
}
