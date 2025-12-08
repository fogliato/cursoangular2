using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class RedeSocialDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Campo {0} é Obrigatório")]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "O Campo {0} é Obrigatório")]
        public string URL { get; set; } = null!;
    }
}
