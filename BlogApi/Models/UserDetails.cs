namespace EcorpAPI.Models
{
    public class UserDetails : BaseModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Gender { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsDeactivated { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ContactNumber { get; set; }
        public Guid SaltKey { get; set; }
        public string? Password { get; set; }
        public string? ResetToken { get; set; }
        public byte[]? Image { get; set; }
    }

    public class LoginDetail
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public partial class ChangePassword
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string Password { get; set; }
    }
    public class ResetPassword
    {
        public string ResetToken { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public partial class ForgotPassword
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
    }

    public partial class AdminResetPasswordModel
    {
        public string ResetToken { get; set; }
        public string? Password { get; set; }
    }
}
