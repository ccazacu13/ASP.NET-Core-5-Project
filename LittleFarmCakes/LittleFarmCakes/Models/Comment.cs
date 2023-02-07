using System.ComponentModel.DataAnnotations;

namespace LittleFarmCakes.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }
        public int? Stars { get; set; }
        public DateTime Date { get; set; }

        public int? ProductId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } //→ un comentariu apartine unui singur utilizator

        public virtual Product? Product { get; set; }
    }
}
