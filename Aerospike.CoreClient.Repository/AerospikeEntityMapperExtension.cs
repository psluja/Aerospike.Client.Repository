using Aerospike.Client;
using System.Linq;

namespace Aerospike.CoreClient.Repository
{
    public static class AerospikeEntityMapperExtension
    {
        public static Bin[] CreateBinsArray<TEntity>(this IAerospikeEntityMapper aerospikeEntityMapper, TEntity entity)
            where TEntity : IAeroEntity, new()
        {
            return aerospikeEntityMapper.CreateBins(entity).ToArray();
        }
    }
}