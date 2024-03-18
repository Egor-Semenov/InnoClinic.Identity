using Microsoft.AspNetCore.Identity;

namespace InnoClinic.Identity.Models.Entities
{
    public sealed class AppUser : IdentityUser
    {
        public bool IsPasswordConfirmed { get; set; }
    }
}
