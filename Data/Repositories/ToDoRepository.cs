using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public class ToDoRepository<T> : IRepository<T> where T : class
    {
        public ToDoRepository()
        {
            Context = ContextSingleton.GetInstance();
            Set = Context.Set<T>();
        }

        public IEnumerable<T> Get()
        {
            return Set.AsEnumerable();
        }

        public IEnumerable<T> GetByPredicate(Expression<Func<T, bool>> expression)
        {
            return Set.Where(expression);
        }

        public bool Add(T item)
        {
            try
            {
                Set.Add(item);
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public bool Remove(T item)
        {
            try
            {
                Set.Remove(item);
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                ErrorHandler.Handle(e);

                return false;
            }

            return true;
        }

        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        public void Refresh()
        {
            Context = ContextSingleton.GetInstance();
            Set = Context.Set<T>();
        }

        private ToDoContext Context;
        private DbSet<T> Set;
    }
}