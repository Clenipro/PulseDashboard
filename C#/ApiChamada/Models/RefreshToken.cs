namespace ApiChamada.Models
{
    public class RefreshToken
    {
        public string Id { get; set; } = null!;
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}