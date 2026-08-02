namespace Kairos.Api.Models
{
    public class RefreshToken
    {
        public int ID { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
