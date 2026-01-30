namespace Container_App.Model.Users
{
    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int IsDel { get; set; }
        public DateTime? CreateAt { get; set; }
        public Guid? CreateBy { get; set; }
    }
}
