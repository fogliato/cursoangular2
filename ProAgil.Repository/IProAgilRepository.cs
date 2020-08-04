using System.Threading.Tasks;
using ProAgil.Domain;

namespace ProAgil.Repository
{
    public interface IProAgilRepository
    {
        void Add<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();
        //Eventos
        Task<Evento[]> GetAllEventoAsyncByTema(string tema, bool includePalestrantes);
        Task<Evento[]> GetAllEventoAsync(bool includePalestrantes);
        Task<Evento> GetEventoByIdAsync(int id, bool includePalestrantes);

        //PALESTRANTE
        Task<Palestrante[]> GetAllPalestrantesAsync(bool includeEvento);
        Task<Palestrante[]> GetAllPalestrantesAsyncByName(string nome, bool includeEvento);
        Task<Palestrante> GetPalestranteByIdAsync(int id, bool includeEvento);
        void DeleteRange<T>(T[] entity) where T : class;
    }
}