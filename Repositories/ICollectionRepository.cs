using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories;

public interface ICollectionRepository
{
    public void EnsureDbExists();
    public List<Collection> GetAllCollections();
    public void Add(Collection collection);
    public void AddVolume(Collection collection, Volume volume);
    public void RemoveVolume(Collection collection, Volume volume);
    public void Remove(Collection collection);
    public bool HasChanges();
    public void Save();
    public void DiscardChanges();
}
