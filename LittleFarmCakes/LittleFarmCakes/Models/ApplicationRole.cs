using Microsoft.AspNetCore.Identity;

namespace LittleFarmCakes.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? RoleName { get; set; }
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
    }
}
