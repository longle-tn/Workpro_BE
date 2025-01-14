
using Container_App.Model.Projects;
using Container_App.Model.Tasks;
using Container_App.utilities;
using Npgsql;

namespace Container_App.Repository.TaskRepository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly SqlQueryHelper _sqlQueryHelper;

        public TaskRepository(SqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }
        public async Task<int> AddTaskAsync(Tasks task, int userId)
        {
            task.Status = (int)StatusTask.Active;
            string sqlGetMaxId = @"SELECT COALESCE(MAX(""TaskId""), 0) + 1 FROM ""Task"";";
            task.TaskId = await _sqlQueryHelper.ExecuteScalarAsync<int>(sqlGetMaxId);

            string sqlInsert = @"
            INSERT INTO public.""Task"" (""TaskId"", ""ProjectId"", ""AssignedTo"", ""TaskName"", ""Description"", ""DueDate"", ""Status"", ""CreatedAt"", ""UpdatedAt""
            ""CreatedBy"", ""CreatedAt"")
            VALUES(@TaskId, @ProjectId, @AssignedTo, @TaskName, @Description, @DueDate, @Status, @CreatedAt, @UpdateAt);";

            // Tạo NpgsqlParameter cho các tham số trong câu lệnh INSERT
            var parameters = new[]
            {
                new NpgsqlParameter("@TaskId", task.TaskId),
                new NpgsqlParameter("@ProjectId", task.ProjectId),
                new NpgsqlParameter("@AssignedTo", task.AssignedTo),
                new NpgsqlParameter("@TaskName", task.TaskName),
                new NpgsqlParameter("@Description", task.Description),
                new NpgsqlParameter("@DueDate", task.DueDate),
                new NpgsqlParameter("@Status", task.Status),
                new NpgsqlParameter("@CreatedAt", DateTime.Now),
                new NpgsqlParameter("@UpdateAt", DateTime.Now)
            };
            // Thực thi câu lệnh INSERT
            return await _sqlQueryHelper.ExecuteNonQueryAsync(sqlInsert, parameters);
        }
    }
}
