using System;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class LoteDto
    {
        public int Id { get; set; }
        
        [Required]
        public string Nome { get; set; }
        
        [Required]
        public decimal Preco { get; set; }
        
        public DateTime? DataInicio { get; set; }
        
        public DateTime? DataFim { get; set; }
        
        [Range(2,50000, ErrorMessage="A quantidade de pessoas deve ser entre 2 e 50 000")]
        public int Quantidade { get; set; }       
        
    }
}