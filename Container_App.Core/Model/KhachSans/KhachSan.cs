using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Core.Model.KhachSans
{
    public class KhachSan
    {
        public Guid Id { get; set; }
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
        public DateTime NgayTao { get; set; }
        public string FullName { get; set; }
        public long RowNum { get; set; }
        public int TotalRow { get; set; }
        public List<string> Urls { get; set; }
    }
}
