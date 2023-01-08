using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUserRole : IdentityRole<int>
    {
        public AppUser User { get; set; }

        public AppRole Role { get; set; }
    }
}
