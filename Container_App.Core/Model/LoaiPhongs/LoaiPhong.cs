using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Core.Model.LoaiPhongs
{
    public class LoaiPhong
    {
        public Guid Id { get; set; }    
        public Guid KhachSanId { get; set; }
        public string TenLoaiPhong { get; set; }
        public int SoKhachToiDa { get; set; }
        public string KieuGiuong { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
