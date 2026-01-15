create database WorkPro

create table UserLogin
(
	Id uniqueidentifier primary key,
	Username nvarchar(100),
	[Password] nvarchar(100)
)

create table UserProfile
(
	Id uniqueidentifier primary key,
	FullName nvarchar(255),
	Phone nvarchar(15),
	Email nvarchar(255),
	[Address] nvarchar(500),
	IsDel int,
	CreateAt datetime,
	CreateBy uniqueidentifier,
	UserLoginId uniqueidentifier
)


create table Roles
(
	Id uniqueidentifier primary key,
	RoleName nvarchar(255),
	[Description] nvarchar(255),
	CreateAt datetime,
	CreateBy uniqueidentifier,
)

CREATE TABLE Resources (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ResourceName NVARCHAR(100) UNIQUE,
    [Description] NVARCHAR(255)
);


create table [Permissions]
(
	Id uniqueidentifier primary key,
	[Action] nvarchar(100),
	CreateAt datetime,
	CreateBy uniqueidentifier
)

create table RolePermissions
(
	Id uniqueidentifier primary key,
	RoleId uniqueidentifier,
	ResourceId UNIQUEIDENTIFIER,
	PermissionId uniqueidentifier
)

create table UserRoles
(
	Id uniqueidentifier primary key,
	UserId uniqueidentifier,
	RoleId uniqueidentifier
)
go

create proc sp_CreateUser
@Username nvarchar(100),
@Password nvarchar(100),
@FullName nvarchar(255),
@Phone nvarchar(15),
@Email nvarchar(255),
@Address nvarchar(500),
@CreateBy uniqueidentifier
as
begin 
	begin try
		begin transaction;
			declare @Id uniqueidentifier = newid();
			insert into UserLogin(Id, Username, Password) values
			(@Id, @Username, @Password)

			insert into UserProfile(Id, FullName, Phone, Email, Address, IsDel, CreateAt, CreateBy, UserLoginId)
			values
			(NEWID(), @FullName, @Phone, @Email, @Address, 0, GETDATE(), @CreateBy, @Id)
		commit transaction;
	end try
	BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
end;



