using System;
using System.Reflection;
using Aerospike.Client;

namespace Aerospike.CoreClient.Repository
{
    public class GenericAeroPropertySupport: IAeroPropertySupport
    {
        private readonly Func<PropertyInfo, IAeroEntity, Bin> _createBinFunc;
        private readonly Action<Record, IAeroEntity, PropertyInfo> _setPropertyValueFunc;
        private readonly Type _type;

        public Type EntityType {
            get { return _type;}
        }

        public GenericAeroPropertySupport(
            Type type,
            Func<PropertyInfo, IAeroEntity, Bin> createBinFunc,
            Action<Record, IAeroEntity, PropertyInfo> setPropertyValueFunc)
        {
            _type = type;
            _setPropertyValueFunc = setPropertyValueFunc;
            _createBinFunc = createBinFunc;
        }

        public Bin CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity) where TEntity : IAeroEntity, new()
        {
            return _createBinFunc(propertyInfo, entity);
        }

        public void SetPropertyValue<TEntity>(Record record, TEntity entity, PropertyInfo propertyInfo) where TEntity : IAeroEntity, new()
        {
            _setPropertyValueFunc(record, entity, propertyInfo);
        }
    }
}