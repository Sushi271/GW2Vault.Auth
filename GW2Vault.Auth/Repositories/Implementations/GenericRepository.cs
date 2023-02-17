using System;
using System.Collections.Generic;
using System.Linq;
using GW2Vault.Auth.Data;

namespace GW2Vault.Auth.Repositories.Implementations
{
    public abstract class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        protected AuthContext Context { get; }

        public GenericRepository(AuthContext context)
            => Context = context;

        public List<T> GetList()
            => Context.GetTable<T>().ToList();

        public List<T> GetList(Func<T, bool> predicate)
            => Context.GetTable<T>().Where(predicate).ToList();

        public T GetById(int id)
            => Context.GetTable<T>().Find(id);

        public T Insert(T entity)
        {
            var entry = Context.GetTable<T>().Add(entity);
            Context.SaveChanges();
            return entry.Entity;
        }

        public T Update(T entity)
        {
            var entry = Context.GetTable<T>().Update(entity);
            Context.SaveChanges();
            return entry.Entity;
        }

        public void Delete(T entity)
        {
            Context.GetTable<T>().Remove(entity);
            Context.SaveChanges();
        }
    }
}
