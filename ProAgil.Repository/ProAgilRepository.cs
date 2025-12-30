using System;
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
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        
        // General
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

        public async Task<Event[]> GetAllEventsAsync(bool includeSpeakers = false)
        {
            IQueryable<Event> query = _context.Events
                .Include(c => c.Batches)
                .Include(c => c.SocialNetworks);

            if (includeSpeakers)
            {
                query = query
                    .Include(pe => pe.SpeakerEvents)
                    .ThenInclude(p => p.Speaker);
            }

            query = query.AsNoTracking()
                .OrderBy(c => c.Id);

            return await query.ToArrayAsync();
        }

        // Events
        public async Task<Event?> GetEventByIdAsync(int id, bool includeSpeakers)
        {
            IQueryable<Event> query = _context.Events
                .Include(e => e.Batches)
                .Include(e => e.SocialNetworks);
            if (includeSpeakers)
                query = query.Include(e => e.SpeakerEvents).ThenInclude(p => p.Speaker);

            return await query.Where(e => e.Id == id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<Event[]> GetAllEventsByThemeAsync(string theme, bool includeSpeakers)
        {
            IQueryable<Event> query = _context.Events
                .Include(e => e.Batches)
                .Include(e => e.SocialNetworks);
            if (includeSpeakers)
                query = query.Include(e => e.SpeakerEvents).ThenInclude(p => p.Speaker);
            query = query.OrderByDescending(e => e.EventDate)
                .Where(e => e.Theme.ToLower().Contains(theme.ToLower()));
            return await query.ToArrayAsync();
        }

        // Speakers
        public async Task<Speaker?> GetSpeakerByIdAsync(int id, bool includeEvents = false)
        {
            IQueryable<Speaker> query = _context.Speakers.Where(p => p.Id == id)
                .Include(p => p.SocialNetworks);
            if (includeEvents)
                query = query.Include(p => p.SpeakerEvents).ThenInclude(p => p.Event);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Speaker[]> GetAllSpeakersByNameAsync(string name, bool includeEvents)
        {
            IQueryable<Speaker> query = _context.Speakers
                .Include(p => p.SocialNetworks);
            if (includeEvents)
                query = query.Include(p => p.SpeakerEvents).ThenInclude(p => p.Event);
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
            return await query.ToArrayAsync();
        }

        public void DeleteRange<T>(T[] entityArray) where T : class
        {
            _context.RemoveRange(entityArray);
        }

        public async Task<Speaker[]> GetAllSpeakersAsync(bool includeEvents)
        {
            IQueryable<Speaker> query = _context.Speakers
                .Include(p => p.SocialNetworks);
            if (includeEvents)
                query = query.Include(p => p.SpeakerEvents).ThenInclude(p => p.Event);
            
            return await query.ToArrayAsync();
        }

        public async Task<Event[]> GetLatestEvents()
        {
            return await _context.Events.AsNoTracking().OrderByDescending(c => c.Id).Take(5).ToArrayAsync();
        }
    }
}
