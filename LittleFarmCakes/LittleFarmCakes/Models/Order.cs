using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LittleFarmCakes.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ProductId { get; set; }

        public int Count { get; set; }

        public virtual Product? Product { get; set; }

        public virtual ApplicationUser? User { get; set; } //→ un comentariu apartine unui singur utilizator
    }
}
