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

    public void Save()
    {
        context.SaveChanges();
    }
}