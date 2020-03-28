using System.Collections.Generic;
using Aerospike.Client;

namespace Aerospike.CoreClient.Repository
{
    public interface IAerospikeEntityMapper
    {
        TEntity GetEntity<TEntity>(Record record) where TEntity : IAeroEntity, new();
        IEnumerable<Bin> CreateBins<TEntity>(TEntity entity) where TEntity : IAeroEntity, new();
        IEnumerable<string> GetPropertiesNames<TEntity>() where TEntity : IAeroEntity, new();
    }
}