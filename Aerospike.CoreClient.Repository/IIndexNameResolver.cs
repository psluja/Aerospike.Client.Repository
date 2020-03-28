using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.CoreClient.Repository
{
    public interface IIndexNameResolver
    {
        string GetIndexName<TEntity>(string propertyName) where TEntity : IAeroEntity, new();
    }
}
