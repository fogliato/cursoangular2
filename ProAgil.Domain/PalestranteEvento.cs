namespace ProAgil.Domain
{
    public class PalestranteEvento
    {
        public int PalestranteId { get; set; }
        public Palestrante Palestrante { get; set; } = null!;
        public int EventoId { get; set; }
        public Evento Evento { get; set; } = null!;
    }
}
