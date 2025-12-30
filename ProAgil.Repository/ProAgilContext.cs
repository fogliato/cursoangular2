using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProAgil.Domain;
using ProAgil.Domain.Identity;

namespace ProAgil.Repository
{
    public class ProAgilContext : IdentityDbContext<User, Role, int, 
                                                    IdentityUserClaim<int>, UserRole, 
                                                    IdentityUserLogin<int>, IdentityRoleClaim<int>, 
                                                    IdentityUserToken<int>>
    {

        public ProAgilContext(DbContextOptions<ProAgilContext> options) : base(options)
        {

        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<SpeakerEvent> SpeakerEvents { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<SocialNetwork> SocialNetworks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserRole>(
                userRole => {

                    userRole.HasKey(usr => new {usr.UserId, usr.RoleId});
                    
                    userRole.HasOne(ur => ur.Role)
                            .WithMany(r => r.UserRoles)
                            .HasForeignKey(ur => ur.RoleId)
                            .IsRequired();
                    
                    userRole.HasOne(ur => ur.User)
                            .WithMany(r => r.UserRoles)
                            .HasForeignKey(ur => ur.UserId)
                            .IsRequired();
                }
            );

            modelBuilder.Entity<SpeakerEvent>()
                .HasKey(SE => new { SE.EventId, SE.SpeakerId });

        }

    }
}
