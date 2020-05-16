using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProAgil.Domain;

namespace ProAgil.Repository
{
    public class ProAgilRepository : IProAgilRepository
    {
        private readonly ProAgilContext _context;

        public ProAgilRepository(ProAgilContext context)
        {
            _context = context;
        }
        //Gerais
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<Evento[]> GetAllEventoAsync(bool includePalestrantes = false)
        {
            IQueryable<Evento> query = _context.Eventos
                .Include(e => e.Lotes)
                .Include(e => e.RedesSociais);
            if (includePalestrantes)
                query.Include(e => e.PalestrantesEventos).ThenInclude(p => p.Palestrante);
            query.OrderByDescending(e => e.DataEvento);
            return await query.ToArrayAsync();
        }

        //EVENTOS
        public async Task<Evento> GetAllEventoAsyncById(int id, bool includePalestrantes)
        {
            IQueryable<Evento> query = _context.Eventos
                .Include(e => e.Lotes)
                .Include(e => e.RedesSociais);
            if (includePalestrantes)
                query.Include(e => e.PalestrantesEventos).ThenInclude(p => p.Palestrante);

            query.Where(e => e.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Evento[]> GetAllEventoAsyncByTema(string Tema, bool includePalestrantes)
        {
            IQueryable<Evento> query = _context.Eventos
                .Include(e => e.Lotes)
                .Include(e => e.RedesSociais);
            if (includePalestrantes)
                query.Include(e => e.PalestrantesEventos).ThenInclude(p => p.Palestrante);
            query.OrderByDescending(e => e.DataEvento)
                .Where(e => e.Tema.ToLower().Contains(Tema.ToLower()));
            return await query.ToArrayAsync();
        }

        //PALESTRANTES
        public async Task<Palestrante> GetAllPalestranteAsync(int id, bool includeEvento = false)
        {
            IQueryable<Palestrante> query = _context.Palestrante
                .Include(p => p.RedesSociais);
            if (includeEvento)
                query.Include(p => p.PalestrantesEventos).ThenInclude(p => p.Evento);
            query.Where(p => p.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Palestrante[]> GetAllPalestrantesAsyncByName(string nome, bool includeEvento)
        {
            IQueryable<Palestrante> query = _context.Palestrante
                .Include(p => p.RedesSociais);
            if (includeEvento)
                query.Include(p => p.PalestrantesEventos).ThenInclude(p => p.Evento);
            query.Where(p => p.Nome.ToLower().Contains(nome.ToLower()));
            return await query.ToArrayAsync();
        }

    }
}