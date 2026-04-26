using Domain;
using Repositories;

namespace TestingDataBase;

public static class Program
{
    public static void Main()
    {
        var repository = new CollectionRepository();
        var collections = repository.GetAllCollections();

        if (collections.FirstOrDefault(c => c.Name.Contains("teste")) is not { } collection) return;

        var lastVolume = 1;
        for (var i = 0; i < 100; i++)
        {
            if (i % 2 == 0)
            {
                repository.AddVolume(collection, new Volume() { IsOwned = false, Number = ++lastVolume });
            }
            else
            {
                repository.AddVolume(collection, new Volume() { IsOwned = false, SpecialEdition = $"Special Ed: {lastVolume}.5"});
            }
        }
        //repository.AddVolume(collection, new Volume() { IsOwned = false, Number = 1 });

        repository.Save();
    }
}