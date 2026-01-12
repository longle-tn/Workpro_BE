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

insert into UserLogin(Id, Username, Password) values
(NEWID(), 'longlt', '123456')
select * from UserProfile

insert into UserProfile (Id, FullName, Phone, Email, Address,
IsDel, CreateAt, UserLoginId) values
(NEWID(), N'Lê Thành Long', '0933026960', 'thanhlongle978@gmail.com',
N'129 Tầm Lanh, Phước Thạnh, Tây Ninh', 0, GETDATE(), 
'58D2F21D-4FF5-427C-8929-6D8D0FFD6505')

insert into Roles(Id, RoleName, Description, CreateAt) values
(NEWID(), N'Admin', N'Nhóm quyền dành cho quản trị viên', GETDATE())

insert into Roles(Id, RoleName, Description, CreateAt) values
(NEWID(), N'Staff', N'Nhóm quyền dành cho nhân viên', GETDATE())

insert into Resources(Id, ResourceName, Description) values
(NEWID(), N'Projects', N''),
(NEWID(), N'Tasks', N'')

insert into Permissions(Id, Action, CreateAt) values
(NEWID(), N'View', GETDATE()),
(NEWID(), N'Create', GETDATE()),
(NEWID(), N'Update', GETDATE()),
(NEWID(), N'Delete', GETDATE())

select * from Roles
select * from Resources
select * from Permissions

insert into RolePermissions(Id, RoleId, ResourceId, PermissionId) values
(NEWID(), 'C24517D8-0B66-40E4-89B3-55229F797BDE', 
'61259117-4AF4-4E98-9A5E-8A056461BA3C', 
'9FAC50B9-924D-4F69-AACA-50388C8B8827'),

(NEWID(), 'C24517D8-0B66-40E4-89B3-55229F797BDE', 
'61259117-4AF4-4E98-9A5E-8A056461BA3C', 
'701F7CEE-F353-48A9-AF0F-E3706C4DC84D'),

(NEWID(), 'C24517D8-0B66-40E4-89B3-55229F797BDE', 
'61259117-4AF4-4E98-9A5E-8A056461BA3C', 
'B068251B-24F5-4530-B859-E6A28D9E29E5'),

(NEWID(), 'C24517D8-0B66-40E4-89B3-55229F797BDE', 
'61259117-4AF4-4E98-9A5E-8A056461BA3C', 
'9FAC50B9-924D-4F69-AACA-50388C8B8827')

select * from Roles
select * from UserLogin

insert into UserRoles(Id, UserId, RoleId) values
(NEWID(), '58D2F21D-4FF5-427C-8929-6D8D0FFD6505',
'C24517D8-0B66-40E4-89B3-55229F797BDE')

SELECT 
    r.RoleName,
    res.ResourceName,
    p.[Action]
FROM UserLogin u
JOIN UserRoles ur 
    ON u.Id = ur.UserId
JOIN Roles r 
    ON ur.RoleId = r.Id
JOIN RolePermissions rp 
    ON r.Id = rp.RoleId
JOIN Resources res 
    ON rp.ResourceId = res.Id
JOIN Permissions p 
    ON rp.PermissionId = p.Id
WHERE u.Id = '58D2F21D-4FF5-427C-8929-6D8D0FFD6505'
ORDER BY res.ResourceName, p.[Action];

