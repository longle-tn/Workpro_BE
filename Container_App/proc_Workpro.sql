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

alter proc sp_Login
@Username nvarchar(100),
@Password nvarchar(100)
as
begin
	select ul.Id, Username, FullName, Email,
	Phone, Address, RoleName, RoleId
	from UserLogin ul
	join UserProfile up on ul.Id = up.UserLoginId
	join UserRoles ur on ul.Id = ur.UserId
	join Roles r on r.Id = ur.RoleId
	where ul.Username = @Username and ul.Password = @Password
	and IsDel = 0
end
go

alter proc sp_GetListPermissionByUser
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

select * from KhachSan

go

alter proc sp_TaoKhachSan
@NguoiTao uniqueidentifier,
@TenKhachSan NVARCHAR(255),
@MoTa NVARCHAR(MAX),
@DiaChi NVARCHAR(500),
@ThanhPho NVARCHAR(100),
@ViDo FLOAT,
@KinhDo FLOAT,
@SoSao INT,
@GioNhanPhong TIME,
@GioTraPhong TIME,
@TrangThai NVARCHAR(50),
@ListImage hotel_images readonly
as
begin
	declare @Id UNIQUEIDENTIFIER = NEWID();
	begin try
		begin transaction;
	insert into KhachSan(Id, NguoiTao, TenKhachSan, MoTa, DiaChi, ThanhPho,
	ViDo, KinhDo, SoSao, GioNhanPhong, GioTraPhong, TrangThai,NgayTao)values
	(@Id, @NguoiTao, @TenKhachSan, @MoTa, @DiaChi, @ThanhPho,
	@ViDo, @KinhDo, @SoSao, @GioNhanPhong, @GioTraPhong, @TrangThai, GETDATE())

	INSERT INTO KhachSanImages (KhachSanId, Url)
	SELECT @Id, Url
	FROM @ListImage
	commit transaction;
	end try
	BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
end;
go

create proc sp_ThemTienIch
@TenTienIch nvarchar(100),
@Icon nvarchar(100)
as
begin
	insert into TienIch(Id, TenTienIch, Icon)values
	(NEWID(), @TenTienIch, @Icon)
end;
go

create proc sp_ThemLoaiPhong
@KhachSanId uniqueidentifier,
@TenLoaiPhong nvarchar(255),
@SoKhachToiDa int,
@KieuGiuong nvarchar(100),
@MoTa nvarchar(max)
as
begin
	insert into LoaiPhong(Id, KhachSanId, TenLoaiPhong, SoKhachToiDa, KieuGiuong, MoTa, NgayTao) values
	(NEWID(), @KhachSanId, @TenLoaiPhong, @SoKhachToiDa, @KieuGiuong, @MoTa, GETDATE())
end;
go

alter proc sp_ThemPhong
@LoaiPhongId UNIQUEIDENTIFIER,
@SoPhong NVARCHAR(50),
@Tang int,
@TrangThai NVARCHAR(50)
as
begin
	insert into Phong(Id, LoaiPhongId, SoPhong, Tang, TrangThai) values
	(NEWID(), @LoaiPhongId, @SoPhong, @Tang, @TrangThai)
end
go



create proc sp_LayDanhSachKhachSanAdmin
 @Keyword nvarchar(255),
 @ThanhPho nvarchar(255),
 @ViDo float,
 @KinhDo float,
 @SoSao int,
 @TrangThai nvarchar(100),
 @StartRow int,
 @EndRow int
 as
 begin
	set nocount on;
	declare @sql nvarchar(max);

	SET @sql = N'
        SELECT  *
        FROM (
            SELECT ks.Id, ks.TenKhachSan, ks.Mota, ks.DiaChi, 
			ks.ThanhPho, ks.KinhDo, ks.ViDo, ks.SoSao,
			convert(nvarchar(10), ks.GioNhanPhong, 108) as GioNhanPhong, 
			convert(nvarchar(10), ks.GioTraPhong, 108) as GioTraPhong, 
			ks.TrangThai, up.FullName,
            ROW_NUMBER() OVER (ORDER BY NgayTao desc) AS RowNum,
			COUNT(*) OVER() AS TotalRow
            FROM  KhachSan ks
			join UserProfile up on ks.NguoiTao = up.UserLoginId
            WHERE   1 = 1
    ';
	
	IF @Keyword IS NOT NULL AND @Keyword <> ''
    SET @sql += N' AND TenKhachSan = @Keyword';

    -- Lọc theo thành phố
    IF @ThanhPho IS NOT NULL AND @ThanhPho <> ''
        SET @sql += N' AND ThanhPho = @ThanhPho';

    -- Lọc theo số sao
    IF @SoSao IS NOT NULL AND @SoSao > 0
        SET @sql += N' AND SoSao = @SoSao';

	if @TrangThai is not null and @TrangThai <> ''
		set @sql += N' and TrangThai = @TrangThai'

    -- Lọc theo tọa độ (bán kính ~10km, dùng công thức khoảng cách đơn giản)
    IF @ViDo IS NOT NULL and @ViDo <> 0 AND @KinhDo IS NOT NULL and @KinhDo <> 0
        SET @sql += N' AND (
            SQRT(
                POWER((ViDo  - @ViDo)  * 111320, 2) +
                POWER((KinhDo - @KinhDo) * 111320 * COS(RADIANS(@ViDo)), 2)
            ) <= 10000
        )';

    -- Phân trang bằng ROW_NUMBER
    SET @sql += N'
        ) AS Paged
        WHERE RowNum BETWEEN @StartRow AND @EndRow
        ORDER BY RowNum
    ';
    -- Thực thi với sp_executesql để tránh SQL Injection
    EXEC sp_executesql @sql,
        N'@Keyword  NVARCHAR(255),
          @ThanhPho NVARCHAR(255),
          @ViDo     FLOAT,
          @KinhDo   FLOAT,
          @SoSao    INT,
		  @TrangThai nvarchar(100),
          @StartRow INT,
          @EndRow   INT',
        @Keyword, @ThanhPho, @ViDo, @KinhDo, @SoSao, @TrangThai, @StartRow, @EndRow;
END;
go

create proc sp_LayDanhSachKhachSanOwner
 @Keyword nvarchar(255),
 @ThanhPho nvarchar(255),
 @ViDo float,
 @KinhDo float,
 @SoSao int,
 @TrangThai nvarchar(100),
 @UserId uniqueidentifier,
 @StartRow int,
 @EndRow int
 as
 begin
	set nocount on;
	declare @sql nvarchar(max);

	SET @sql = N'
        SELECT  *
        FROM (
            SELECT ks.Id, ks.TenKhachSan, ks.Mota, ks.DiaChi, 
			ks.ThanhPho, ks.KinhDo, ks.ViDo, ks.SoSao,
			convert(nvarchar(10), ks.GioNhanPhong, 108) as GioNhanPhong, 
			convert(nvarchar(10), ks.GioTraPhong, 108) as GioTraPhong, 
			ks.TrangThai, up.FullName,
            ROW_NUMBER() OVER (ORDER BY NgayTao desc) AS RowNum,
			COUNT(*) OVER() AS TotalRow
            FROM  KhachSan ks
			join UserProfile up on ks.NguoiTao = up.UserLoginId
            WHERE   ks.NguoiTao = @UserId
    ';
	
	IF @Keyword IS NOT NULL AND @Keyword <> ''
    SET @sql += N' AND TenKhachSan = @Keyword';

    -- Lọc theo thành phố
    IF @ThanhPho IS NOT NULL AND @ThanhPho <> ''
        SET @sql += N' AND ThanhPho = @ThanhPho';

    -- Lọc theo số sao
    IF @SoSao IS NOT NULL AND @SoSao > 0
        SET @sql += N' AND SoSao = @SoSao';

	if @TrangThai is not null and @TrangThai <> ''
		set @sql += N' and TrangThai = @TrangThai'

    -- Lọc theo tọa độ (bán kính ~10km, dùng công thức khoảng cách đơn giản)
    IF @ViDo IS NOT NULL and @ViDo <> 0 AND @KinhDo IS NOT NULL and @KinhDo <> 0
        SET @sql += N' AND (
            SQRT(
                POWER((ViDo  - @ViDo)  * 111320, 2) +
                POWER((KinhDo - @KinhDo) * 111320 * COS(RADIANS(@ViDo)), 2)
            ) <= 10000
        )';

    -- Phân trang bằng ROW_NUMBER
    SET @sql += N'
        ) AS Paged
        WHERE RowNum BETWEEN @StartRow AND @EndRow
        ORDER BY RowNum
    ';
    -- Thực thi với sp_executesql để tránh SQL Injection
    EXEC sp_executesql @sql,
        N'@Keyword  NVARCHAR(255),
          @ThanhPho NVARCHAR(255),
          @ViDo     FLOAT,
          @KinhDo   FLOAT,
          @SoSao    INT,
		  @TrangThai nvarchar(100),
		  @UserId uniqueidentifier,
          @StartRow INT,
          @EndRow   INT',
        @Keyword, @ThanhPho, @ViDo, @KinhDo, @SoSao, @TrangThai, @UserId, @StartRow, @EndRow;
END;
go

create proc sp_CheckRoleAdmin
@UserId uniqueidentifier
as
begin
	select ul.Id as UserId, r.RoleName, r.Id as RoleId
	from UserLogin ul
	join UserRoles ur on ul.Id = ur.UserId
	join Roles r on ur.RoleId = r.Id
	where ul.Id = @UserId
end;
go

alter proc sp_InsertRefreshToken
@UserId uniqueidentifier,
@Token nvarchar(1000),
@ExpiryDate int,
@FullName nvarchar(255),
@RoleId uniqueidentifier,
@RoleName nvarchar(255)
as
begin
	insert into RefreshToken(UserId, Token, ExpiryDate, CreatedDate, Status, FullName, RoleId, RoleName) values
	(@UserId, @Token, @ExpiryDate, GETDATE(), 1, @FullName, @RoleId, @RoleName)
end;
go

create proc sp_InsertBanner
@Title nvarchar(500),
@Subtitle nvarchar(1000),
@Url nvarchar(1000),
@IsActive int
as
begin
	insert into Banner(Title, Subtitle, Url, IsActive, CreatedDate) values
	(@Title, @Subtitle, @Url, @IsActive, GETDATE())
end;
go

create proc sp_GetAllBanner
@Keyword nvarchar(255),
@IsActive int,
@StartRow int,
@EndRow int
as
begin
	set nocount on;
	declare @sql nvarchar(max);

	SET @sql = N'
        SELECT  *
        FROM Banner where 1 = 1
    ';
	
	IF @Keyword IS NOT NULL AND @Keyword <> ''
    SET @sql += N' AND Title = @Keyword';

    -- Lọc theo thành phố
    IF @IsActive IS NOT NULL AND @IsActive <> -1
        SET @sql += N' AND IsActive = @IsActive';

    -- Phân trang bằng ROW_NUMBER
    SET @sql += N'
        ) AS Paged
        WHERE RowNum BETWEEN @StartRow AND @EndRow
        ORDER BY RowNum
    ';
    -- Thực thi với sp_executesql để tránh SQL Injection
    EXEC sp_executesql @sql,
        N'@Keyword  NVARCHAR(255),
          @IsActive int,   
          @StartRow INT,
          @EndRow   INT',
        @Keyword, @IsActive, @StartRow, @EndRow;
end;
