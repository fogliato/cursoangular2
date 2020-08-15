using System;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class ContatoDto
    {
        [Required]
        public string NomeCompleto { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Mensagem { get; set; }
    }
}
