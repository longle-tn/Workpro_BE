using Container_App.Data;
using Container_App.Repository.AuthRepository;
using Container_App.Repository.MenuRepository;
using Container_App.Repository.PermissionRepository;
using Container_App.Repository.ProjectRepository;
using Container_App.Repository.ProjectUserInviteRepository;
using Container_App.Repository.ProjectUserRepository;
using Container_App.Repository.RoleMenuAccessRepository;
using Container_App.Repository.RolePermissionsRepository;
using Container_App.Repository.RoleRepository;
using Container_App.Repository.TaskRepository;
using Container_App.Repository.UserRepository;
using Container_App.Repository.UserRoleRepository;
using Container_App.Services.AuthService;
using Container_App.Services.MenuService;
using Container_App.Services.PermissionService;
using Container_App.Services.ProjectService;
using Container_App.Services.RoleMenuAccessService;
using Container_App.Services.RolePermissionsService;
using Container_App.Services.RoleService;
using Container_App.Services.TaskService;
using Container_App.Services.UserRoleService;
using Container_App.Services.UserService;
using Container_App.utilities;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Connection String
var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));
#endregion

#region Add Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectUserRepository, ProjectUserRepository>();
builder.Services.AddScoped<IProjectUserInviteRepository, ProjectUserInviteRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<SqlQueryHelper>();
builder.Services.AddScoped<Config>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRolePermissionsRepository, RolePermissionsRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRoleMenuAccessRepository, RoleMenuAccessRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
#endregion

#region Add Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRolePermissionsService, RolePermissionsService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRoleMenuAccessService, RoleMenuAccessService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITaskService, TaskService>();
#endregion


var jwtSecretKey = "pr5Oyw1J3I8E04g3XsPf5d8wPT9W2bMcwCm6qzHoOoI=";

builder.Configuration["JWT_SECRET_KEY"] = jwtSecretKey;
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("JWT_SECRET_KEY must be set in the .env file.");
}
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowall", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Cấu hình ứng dụng lắng nghe HTTP
builder.WebHost.UseUrls("http://0.0.0.0:5925");

builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Please enter into field the word 'Bearer' following by space and JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        //Scheme = "bearer", // must be lower case
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                        {
                                            {securityScheme, new string[] { }}
                                        });
});

var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection(); // Đặt trước UseRouting
//app.UseHttpsRedirection(); // Đặt trước UseRouting // bỏ chạy http thâu
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Không cần gọi MapControllers ở đây
});

app.Run();
