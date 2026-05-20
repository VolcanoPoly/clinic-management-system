namespace ClinicAPI.DTOs
{
    /// <summary>
    /// DTO for login request with email and password
    /// </summary>
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
