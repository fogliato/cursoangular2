using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ProAgil.Domain.Identity
{
    public class User:IdentityUser<int>
    {
        [Column(TypeName = "nvarchar(150)")]
        public string FullName { get; set; } = null!;
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
