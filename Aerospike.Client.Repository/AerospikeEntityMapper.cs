using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Aerospike.Client;

namespace Aerospike.Client.Repository
{
    public class AerospikeEntityMapper: IAerospikeEntityMapper
    {
        private static readonly IDictionary<Type, PropertyInfo[]> PropertyInfoCache;
        private readonly ISerializer _serializer;
        private readonly IBinaryPresenter _binaryPresenter;
        private readonly IAeroTypeSupport _aeroTypeSupport;

        static AerospikeEntityMapper()
        {
            PropertyInfoCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        }

        public AerospikeEntityMapper() : this(new AeroTypeSupport(), new BinaryFormatterSerializer())
        {
        }
        public AerospikeEntityMapper(IAeroTypeSupport aeroTypeSupport) : this(aeroTypeSupport, new BinaryFormatterSerializer())
        {
        }

        public AerospikeEntityMapper(IAeroTypeSupport aeroTypeSupport, ISerializer serializer, IBinaryPresenter binaryPresenter = null)
        {
            _serializer = serializer;
            _binaryPresenter = binaryPresenter;
            _aeroTypeSupport = aeroTypeSupport;
            
        }

        private PropertyInfo[] Get<TEntity>() where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            if (PropertyInfoCache.ContainsKey(entityType) == false)
            {
                PropertyInfoCache.Add(entityType, entityType.GetProperties()
                    .Where(i => i.CanWrite)
                    .Where(i => i.MemberType == MemberTypes.Property).ToArray());
            }

            return PropertyInfoCache[entityType];
        }

        public TEntity GetEntity<TEntity>(Record record) where TEntity : IAeroEntity, new()
        {
            if(record == null)
                throw new ArgumentNullException("record");

            TEntity entity = new TEntity();
            foreach (PropertyInfo propertyInfo in Get<TEntity>())
            {
                SetPropertyValue(propertyInfo, entity, record);
            }

            return entity;
        }

        public IEnumerable<string> GetPropertiesNames<TEntity>() where TEntity : IAeroEntity, new()
        {
            return Get<TEntity>().Select(i => i.Name);
        }

        public IEnumerable<Bin> CreateBins<TEntity>(TEntity entity)where TEntity : IAeroEntity, new()
        {
            foreach (PropertyInfo propertyInfo in Get<TEntity>())
            {
                yield return CreateBin(propertyInfo, entity);
            }
        }
        public Bin CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity) where TEntity : IAeroEntity, new()
        {
            Bin bin = null;
            if (_aeroTypeSupport.CreateBin(propertyInfo,entity, out bin) == false)
            {
                bin = CreateBinSerialized(propertyInfo, entity);
            }

            return bin;
        }

        private void SetPropertyValue<TEntity>(PropertyInfo propertyInfo, TEntity entity, Record record)
            where TEntity : IAeroEntity, new()
        {
            if (_aeroTypeSupport.SetPropertyValue(propertyInfo, entity, record) == false)
            {
                SetPropertyValueSerialized(propertyInfo, entity, record);
            } 
        }

        private Bin CreateBinSerialized<TEntity>(PropertyInfo propertyInfo, TEntity entity)
            where TEntity : IAeroEntity, new()
        {
            var o = propertyInfo.GetValue(entity);
            byte[] data = _serializer.Serialize(o);
            Bin bin = null;
            if (_binaryPresenter != null)
            {
                bin = new Bin(propertyInfo.Name, _binaryPresenter.MakeString(data));
            }
            else
            {
                bin = new Bin(propertyInfo.Name, data);
            }

            return bin;
        }

        private void SetPropertyValueSerialized<TEntity>(PropertyInfo propertyInfo, TEntity entity, Record record)
            where TEntity : IAeroEntity, new()
        {
            byte[] data;
            if (_binaryPresenter != null)
            {
                data = _binaryPresenter.MakeBinary(record.GetString(propertyInfo.Name));
            }
            else
            {
               data = (byte[])record.GetValue(propertyInfo.Name);
            }

            object value = _serializer.Deserialize(propertyInfo.PropertyType, data);
            propertyInfo.SetValue(entity, value);
        }
    }
}