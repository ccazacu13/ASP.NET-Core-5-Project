using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LittleFarmCakes.Models
{
    public class Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 
        public string? UserId { get; set; }
        public int? ProductId { get; set; }
        public int Count { get; set; }

        public virtual ApplicationUser? User { get; set; } 
        public virtual Product? Product { get; set; }
    }
}
