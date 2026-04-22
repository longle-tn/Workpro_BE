namespace Container_App.Model.KhachSans
{
    public class KhachSanCreateDto
    {     
        public Guid NguoiTao { get; set; }
        public string TenKhachSan { get; set; }
        public string MoTa { get; set; }
        public string DiaChi { get; set; }
        public string ThanhPho { get; set; }
        public double? ViDo { get; set; }
        public double? KinhDo { get; set; }
        public int SoSao { get; set; }
        public string GioNhanPhong { get; set; }
        public string GioTraPhong { get; set; }
        public string TrangThai { get; set; }    
        public List<IFormFile> Files { get; set; }
    }
}
