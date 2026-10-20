using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary.Repositories
{
    public interface ICRUD<T>
    {
        public T Create(T item);
        public T Delete(int id);
        public T Update(T newItem, int id);
        public T GetById(int id);
        public List<T> GetAll();
    }
}
