using System.Security.Cryptography;
using System.Text;

namespace Container_App.utilities
{
    public class Config
    {
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                // Convert hash byte array to a string
                StringBuilder hashBuilder = new StringBuilder();
                foreach (var b in hash)
                {
                    hashBuilder.Append(b.ToString("x2")); // Hexadecimal format
                }
                return hashBuilder.ToString();
            }
        }
    }

    public enum StatusProject
    {
        Active = 1,
        Lock = 2,
        Cancel = 3,
    }

    public enum StatusTask
    {
        Active = 1,
        Complete = 2,
        Cancel = 3,
    }
}
