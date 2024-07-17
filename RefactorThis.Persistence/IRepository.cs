using RefactorThis.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence
{
    public interface IRepository<T> where T : Entity
    {
        T Get(string key);
        void Save(T item);
        void Add(T item);
    }
}