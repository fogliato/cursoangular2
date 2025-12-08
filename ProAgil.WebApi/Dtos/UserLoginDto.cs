using System;

namespace ProAgil.WebApi.Dtos
{
    public class UserLoginDto
    {
        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
