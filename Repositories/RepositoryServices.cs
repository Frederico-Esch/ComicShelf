using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public static class RepositoryServices
    {
        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services.AddDbContext<DataContext>();
            services.AddScoped<ICollectionRepository, CollectionRepository>();
        }
    }
}
