using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProAgil.WebApi.Dtos
{
    public class EventoDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength=2, ErrorMessage="O local deve ter no mimo 100 caracteres e no mínimo")]
        public string Local { get; set; }
        [Required]
        public DateTime DataEvento { get; set; }
        [Required]
        public string Tema { get; set; }
        [Required]
        [Range(2,50000, ErrorMessage="A quantidade de pessoas deve ser entre 2 e 50 000")]
        public int QtdPessoas { get; set; }
        public string Lote { get; set; }
        public string ImagemUrl { get; set; }
        [Phone]
        public string Telefone { get; set; }
        [EmailAddress (ErrorMessage="Informe um e-mail válido")]
        public string Email { get; set; }
        public List<LoteDto> Lotes { get; set; }
        public List<RedeSocialDto> RedesSociais { get; set; }
        public List<PalestranteDto> Palestrantes { get; set; }
    }
}