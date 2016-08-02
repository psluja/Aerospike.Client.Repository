using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.Client.Repository
{
    public class DefaultIndexNameResolver:IIndexNameResolver
    {
        public string GetIndexName<TEntity>(string propertyName) where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            return string.Format("{0}-{1}_Index", entityType.Name.ToLower(), propertyName.ToLower());
        }
    }
}
