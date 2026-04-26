using System.Runtime.InteropServices;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Repositories;

internal class DataContext(IConfiguration configuration) : DbContext
{
    //private const string ConnectionString = @"Data Source='E:\DEV\ComicShelf\Database\ComicShelf.db'";
    
    public DbSet<Collection> Collections { get; set; }
    public DbSet<Volume> Volumes { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (configuration["DataSource"] is string dataSource && File.Exists(dataSource))
        {
            optionsBuilder.UseSqlite($"Data Source='{dataSource}'");
        }
        else
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            optionsBuilder.UseSqlite($"Data Source='{currentDirectory}\\ComicShelf.db'");
        }
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("Collections").HasKey(c => c.Id);
            entity.Property(c => c.Id)
                .HasColumnName("Id")
                .HasConversion(
                    v => v.ToByteArray(),
                    v => MemoryMarshal.Read<Guid>(v)
                );

            entity.Property(c => c.Name)
                .HasColumnName("Name");

            var convertTagsToHasSet = (string v) => v.Length > 0 ? v.Split(";").ToHashSet() : [];
            entity.Property(c => c.Tags)
                .HasColumnName("Tags")
                .HasConversion(
                    v => string.Join(";", v),
                    v => convertTagsToHasSet(v)
                );

            entity.Property(c => c.Cover)
                .HasColumnName("Cover");

            entity.Property(c => c.Order)
                .HasColumnName("Order");
        });

        modelBuilder.Entity<Volume>(entity =>
        {
            entity.ToTable("Volumes").HasKey(v => v.Id);

            entity.Property(v => v.Id)
                .HasColumnName("Id")
                .HasConversion(
                    v => v.ToByteArray(),
                    v => MemoryMarshal.Read<Guid>(v)
                );

            entity.Property(v => v.CollectionId)
                .HasColumnName("CollectionId")
                .HasConversion(
                    v => v.ToByteArray(),
                    v => MemoryMarshal.Read<Guid>(v)
                );

            entity.Property(v => v.Number)
                .HasColumnName("Number");

            entity.Property(v => v.SpecialEdition)
                .HasColumnName("SpecialEdition");

            entity.Property(v => v.IsOwned)
                .HasColumnName("IsOwned");

            entity.Property(v => v.Details)
                .HasColumnName("Details");

            entity
            .HasOne(v => v.Collection)
            .WithMany(c => c.Volumes)
            .HasForeignKey(v => v.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}