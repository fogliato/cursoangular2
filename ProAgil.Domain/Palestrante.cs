using System.Collections.Generic;

namespace ProAgil.Domain
{
    public class Palestrante
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? MiniCurriculo { get; set; }
        public string? ImagemUrl { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public List<RedeSocial> RedesSociais { get; set; } = new List<RedeSocial>();
        public List<PalestranteEvento> PalestrantesEventos { get; set; } = new List<PalestranteEvento>();
    }
}
