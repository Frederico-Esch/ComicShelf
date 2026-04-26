using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Repositories;

internal class CollectionRepository(DataContext context): ICollectionRepository
{
    public void EnsureDbExists()
    {
        const string createTables =
            """
            PRAGMA foreign_keys = off;
            BEGIN TRANSACTION;
            
            -- Table: Collections
            CREATE TABLE IF NOT EXISTS Collections (Id BLOB (16) PRIMARY KEY NOT NULL, Name TEXT NOT NULL, Tags TEXT NOT NULL, Cover BLOB, "Order" INTEGER NOT NULL);
            
            -- Table: Volumes
            CREATE TABLE IF NOT EXISTS Volumes (Id BLOB (16) PRIMARY KEY NOT NULL UNIQUE, CollectionId BLOB (16) REFERENCES Collections (Id) ON DELETE CASCADE NOT NULL, Number INTEGER, Details TEXT, SpecialEdition TEXT, IsOwned INTEGER (1) NOT NULL DEFAULT (0));
            
            COMMIT TRANSACTION;
            PRAGMA foreign_keys = on;
            """;
        context.Database.ExecuteSqlRaw(createTables);
    }

    public List<Collection> GetAllCollections()
    {
        return context
            .Collections
            .OrderBy(c => c.Order)
            .Include(c => c.Volumes)
            .ToList();
    }

    public void Add(Collection collection)
    {
        if (context.Collections.Any(c => c.Order == collection.Order))
        {
            foreach (var c in context.Collections.Where(c => c.Order >=  collection.Order))
            {
                c.Order += 1;
            }
        }
        context.Collections.Add(collection);

    }

    public void AddVolume(Collection collection, Volume volume)
    {
        volume.CollectionId = collection.Id;
        volume.Collection = collection;
        collection.Volumes.Add(volume);

        context.Volumes.Add(volume);
    }

    public void RemoveVolume(Collection collection, Volume volume)
    {
        collection.Volumes.Remove(volume);

        context.Volumes.Remove(volume);
    }

    public void Remove(Collection collection)
    {
        if (context.Collections.Any(c => c.Order == collection.Order))
        {
            foreach (var c in context.Collections.Where(c => c.Order >= collection.Order))
            {
                c.Order -= 1;
            }
        }

        context.Collections.Remove(collection);
    }

    public bool HasChanges() => context.ChangeTracker.Entries().Any(e => e.State != EntityState.Unchanged);

    public void Save()
    {
        context.SaveChanges();
    }

    public void DiscardChanges()
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                    entry.State = EntityState.Unchanged;
                    if (entry.Entity is Volume volume && context.Collections.FirstOrDefault(c => c.Id == volume.CollectionId) is { } collection)
                    {
                        collection.Volumes.Add(volume);
                    }
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
            }
        }
    }
}