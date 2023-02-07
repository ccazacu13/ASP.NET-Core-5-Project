using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LittleFarmCakes.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 de caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]

        public string Description { get; set; }

        [Url]
        public string Picture { get; set; }

        [Required(ErrorMessage = "Pretul produsului este obligatoriu")]

        public float Price { get; set; }
        public float Rating { get; set; }

        public bool Valid { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]

        public int? CategoryId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Cart>? Carts{ get; set; }

        public virtual ICollection<Order>? Orders { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Ratings { get; set; }
    }
}
