using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class SocialNetworkDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "The field {0} is required")]
        public string URL { get; set; } = null!;
    }
}

