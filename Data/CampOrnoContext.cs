using CampOrno.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CampOrno.Data
{
    public class CampOrnoContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public CampOrnoContext(DbContextOptions<CampOrnoContext> options)
        : base(options)
        {
        }

        public CampOrnoContext(DbContextOptions<CampOrnoContext> options, IHttpContextAccessor httpContextAccessor)
       : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
            //UserName = (UserName == null) ? "Unknown" : UserName;
            UserName = UserName ?? "Unknown";
        }

        public DbSet<Camper> Campers { get; set; }
        public DbSet<CamperDiet> CamperDiets { get; set; }
        public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<Compound> Compounds { get; set; }
        public DbSet<CounselorCompound> CounselorCompounds { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("CO");

            //Many to Many Intersection
            modelBuilder.Entity<CamperDiet>()
            .HasKey(c => new { c.CamperID, c.DietaryRestrictionID });

            //Many to Many Intersection
            modelBuilder.Entity<CounselorCompound>()
            .HasKey(c => new { c.CompoundID, c.CounselorID });

            //Add a unique index to the Camper Email
            modelBuilder.Entity<Camper>()
            .HasIndex(p => p.eMail)
            .IsUnique();
            //Add a unique index to the Counselor SIN
            modelBuilder.Entity<Counselor>()
            .HasIndex(p => p.SIN)
            .IsUnique();

            //Add this so you don't get Cascade Delete
            modelBuilder.Entity<CamperDiet>()
                .HasOne(pc => pc.DietaryRestriction)
                .WithMany(c => c.CamperDiets)
                .HasForeignKey(pc => pc.DietaryRestrictionID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CounselorCompound>()
                .HasOne(pc => pc.Counselor)
                .WithMany(c => c.CounselorCompounds)
                .HasForeignKey(pc => pc.CounselorID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CounselorCompound>()
                .HasOne(pc => pc.Compound)
                .WithMany(c => c.CounselorCompounds)
                .HasForeignKey(pc => pc.CompoundID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Camper>()
                .HasOne(pc => pc.Compound)
                .WithMany(c => c.Campers)
                .HasForeignKey(pc => pc.CompoundID)
                .OnDelete(DeleteBehavior.Restrict);


        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}

