alter proc sp_CreateUser
@Username nvarchar(100),
@Password nvarchar(100),
@FullName nvarchar(255),
@Phone nvarchar(15),
@Email nvarchar(255),
@Address nvarchar(500),
@CreateBy uniqueidentifier,
@RoleId uniqueidentifier
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

			insert into UserRoles(Id, RoleId, UserId) values
			(NEWID(), @RoleId, @Id)
		commit transaction;
	end try
	BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
end;
go

create proc sp_Login
@Username nvarchar(100),
@Password nvarchar(100)
as
begin
	select ul.Id, Username, FullName, Email,
	Phone, Address
	from UserLogin ul
	join UserProfile up on ul.Id = up.UserLoginId
	where ul.Username = @Username and ul.Password = @Password
	and IsDel = 0
end
go

create proc sp_GetListPermissionByUser
@UserId uniqueidentifier
as
begin
	SELECT DISTINCT
    res.ResourceName,
    p.[Action]
	FROM UserRoles ur
	JOIN Roles r 
		ON ur.RoleId = r.Id
	JOIN RolePermissions rp 
		ON r.Id = rp.RoleId
	JOIN Resources res 
		ON rp.ResourceId = res.Id
	JOIN [Permissions] p 
		ON rp.PermissionId = p.Id
	WHERE ur.UserId = @UserId
	ORDER BY res.ResourceName, p.[Action]
end;
go

create proc sp_PermissionInRole
@RoleId uniqueidentifier,
@ListResourcePermissions resources_permissions readonly
as
begin 
	begin try
		begin transaction;
			--xóa hết tất cả các quyền của role
			delete from RolePermissions where RoleId = @RoleId

			--insert lại các quyền mới
			insert into RolePermissions(Id, RoleId, ResourceId, PermissionId)
			select NEWID(), @RoleId, ResourceId, PermissionId from @ListResourcePermissions
		commit transaction;
	end try
	BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
end

