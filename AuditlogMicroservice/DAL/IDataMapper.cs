using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.DAL
{
    public interface IDataMapper<T, Key>
    {
        void Insert(T item);
    }
}
