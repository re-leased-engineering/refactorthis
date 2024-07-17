using RefactorThis.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    public class Repository<T> : IRepository<T> where T : Entity, new()
    {
        private readonly List<T> _items = new List<T>();

        public T Get(string key) => _items.FirstOrDefault(e => e.Id == key);

        public void Save(T item)
        {
            //saves the item
        }

        public void Add(T item)
        {
            if (!_items.Contains(item)) _items.Add(item);
        }
    }
}