using System;
using System.Collections.Generic;

namespace GW2Vault.Auth.Repositories
{
    public interface IGenericRepository<T>
        where T : class
    {
        List<T> GetList();
        List<T> GetList(Func<T, bool> predicate);
        T GetById(int id);
        T Insert(T entity);
        T Update(T entity);
        void Delete(T entity);
    }
}
