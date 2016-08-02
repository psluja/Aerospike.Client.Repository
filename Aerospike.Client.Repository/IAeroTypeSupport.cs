using System.Reflection;

namespace Aerospike.Client.Repository
{
    public interface IAeroTypeSupport
    {
        bool CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity, out Bin bin)
            where TEntity : IAeroEntity, new();

        bool SetPropertyValue<TEntity>(PropertyInfo propertyInfo, TEntity entity, Record record)
            where TEntity : IAeroEntity, new();
    }
}