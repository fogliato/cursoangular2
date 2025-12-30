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
        
        // Events
        Task<Event[]> GetAllEventsByThemeAsync(string theme, bool includeSpeakers);
        Task<Event[]> GetAllEventsAsync(bool includeSpeakers);
        Task<Event?> GetEventByIdAsync(int id, bool includeSpeakers);
        Task<Event[]> GetLatestEvents();

        // Speakers
        Task<Speaker[]> GetAllSpeakersAsync(bool includeEvents);
        Task<Speaker[]> GetAllSpeakersByNameAsync(string name, bool includeEvents);
        Task<Speaker?> GetSpeakerByIdAsync(int id, bool includeEvents);
        void DeleteRange<T>(T[] entity) where T : class;
    }
}
