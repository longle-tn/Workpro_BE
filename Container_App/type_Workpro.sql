create type resources_permissions as table
(
	ResourceId uniqueidentifier,
	PermissionId uniqueidentifier
)

create type hotel_images as table
(
	Url nvarchar(1000)
)