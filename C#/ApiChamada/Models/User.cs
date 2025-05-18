using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiChamada.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(191)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(191)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(191)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "USER";

        [Column(TypeName = "datetime(3)")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime(3)")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}