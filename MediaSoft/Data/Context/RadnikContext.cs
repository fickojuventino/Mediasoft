using MediaSoft.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaSoft.Data.Context
{
    public class RadnikContext : IdentityDbContext
    {
        public RadnikContext()
        {
        }

        public RadnikContext(DbContextOptions<RadnikContext> options) : base(options)
        {
        }

        public DbSet<Radnik> Korisnici { get; set; }
        public DbSet<Worker> Workers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Worker>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Radnik");

                entity.Property(e => e.Id)
                .HasMaxLength(50).ValueGeneratedNever();

                entity.Property(e => e.Username).HasMaxLength(50);

                entity.Property(e => e.PasswordHash).HasMaxLength(50);
            });

            builder.Entity<Radnik>(entity => {
                entity.HasKey(e => e.Korisnicko_ime);

                entity.ToTable("Radnici");

                entity.Property(e => e.Korisnicko_ime)
                    .HasMaxLength(50);

                entity.Property(e => e.Ime)
                .HasMaxLength(50);

                entity.Property(e => e.Prezime)
                .HasMaxLength(50);

                entity.Property(e => e.Lozinka)
                .HasMaxLength(50);

                entity.Property(e => e.Pwd)
                .HasMaxLength(10);
            });
        }
    }
}
