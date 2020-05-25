using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class RedeSocialDto
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        [Url]
        public string URL { get; set; }
    }
}