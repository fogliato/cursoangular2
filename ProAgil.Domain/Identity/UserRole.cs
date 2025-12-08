using System;
using Microsoft.AspNetCore.Identity;

namespace ProAgil.Domain.Identity
{
    public class UserRole:IdentityUserRole<int>
    {
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
