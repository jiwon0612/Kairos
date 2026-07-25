using Microsoft.AspNetCore.Identity;

namespace Kairos.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
