using Domain;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class CollectionRepository
{
    private DataContext context = new();
    public List<Collection> GetAllCollections()
    {
        return context
            .Collections
            .Include(c => c.Volumes)
            .ToList();
    }

    public void Add(Collection collection)
    {
        context.Collections.Add(collection);
    }

    public void Remove(Collection collection)
    {
        context.Collections.Remove(collection);
    }

    public void Save()
    {
        context.SaveChanges();
    }
}