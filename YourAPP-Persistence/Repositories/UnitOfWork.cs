using System;
using System.Collections.Generic;
using System.Text;
using YourAPP_Domain.Entities;
using YourAPP_Domain.Interfaces;
using YourAPP_Persistence.Data.DbContext;

namespace YourAPP_Persistence.Repositories
{
    public class UnitOfWork(YourAPPDbContext context)
    {

        private readonly Dictionary<Type, object> repositories = [];
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var EntityType = typeof(TEntity);
            if (repositories.TryGetValue(EntityType, out object? repository))
                return (IGenericRepository<TEntity, TKey>)repository;


            var newRepository = new GenericRepository<TEntity, TKey>(context);
            repositories.Add(EntityType, newRepository);
            return newRepository;

        }




        public async Task<int> SaveChangesAsync()
           => await context.SaveChangesAsync();


    }
}
