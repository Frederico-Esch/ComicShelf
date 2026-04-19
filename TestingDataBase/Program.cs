using Domain;
using Repositories;

namespace TestingDataBase;

public static class Program
{
    public static void Main()
    {
        using var dbContext = new Repositories.DataContext();

        if (dbContext.Collections.FirstOrDefault(c => c.Name == "Teste") is not { } collection)
        {
            Console.WriteLine("Failed to find Collection 'Teste'");
            return;
        }

    }
}