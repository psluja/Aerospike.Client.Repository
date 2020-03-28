using System;
using System.Reflection;
using Aerospike.Client;

namespace Aerospike.CoreClient.Repository
{
    public interface IAeroPropertySupport
    {
        Type EntityType { get; }
        Bin CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity) where TEntity : IAeroEntity, new();
        void SetPropertyValue<TEntity>(Record record, TEntity entity, PropertyInfo propertyInfo) where TEntity : IAeroEntity, new();
    }
}