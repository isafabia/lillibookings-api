namespace Lilliput.Api.Dtos
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal DayRate { get; set; }
    }
}