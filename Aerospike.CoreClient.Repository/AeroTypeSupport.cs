using Aerospike.Client;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.CoreClient.Repository
{
    public class AeroTypeSupport: IAeroTypeSupport
    {
        private readonly IDictionary<Type, IAeroPropertySupport> _aeroTypeSupprtRegistry;

        public AeroTypeSupport()
        {
            _aeroTypeSupprtRegistry = new Dictionary<Type, IAeroPropertySupport>();

            InitSupportedTypes();
        }

        private void AddTypeSupport(IAeroPropertySupport aeroPropertySupport)
        {
            _aeroTypeSupprtRegistry.Add(new KeyValuePair<Type, IAeroPropertySupport>(aeroPropertySupport.EntityType, aeroPropertySupport));
        }

        public virtual bool CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity, out Bin bin) where TEntity : IAeroEntity, new()
        {
            bin = null;
            if (_aeroTypeSupprtRegistry.ContainsKey(propertyInfo.PropertyType))
            {
                bin = _aeroTypeSupprtRegistry[propertyInfo.PropertyType].CreateBin(propertyInfo, entity);
                return true;
            }

            return false;
        }

        public virtual bool SetPropertyValue<TEntity>(PropertyInfo propertyInfo, TEntity entity, Record record)
            where TEntity : IAeroEntity, new()
        {
            if (_aeroTypeSupprtRegistry.ContainsKey(propertyInfo.PropertyType))
            {
                _aeroTypeSupprtRegistry[propertyInfo.PropertyType].SetPropertyValue(record, entity, propertyInfo);
                return true;
            }
            return false;
        }

        private void InitSupportedTypes()
        {
            AddTypeSupport(new GenericAeroPropertySupport(typeof(string), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (string)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetString(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(int), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (int)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetInt(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(bool), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (bool)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetBool(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(double), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (double)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetDouble(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(byte), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (byte)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetByte(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(long), (propertyInfo, entity) =>
            {
                return new Bin(propertyInfo.Name, (long)propertyInfo.GetValue(entity));
            }, (record, entity, propertyInfo) =>
            {
                propertyInfo.SetValue(entity, record.GetLong(propertyInfo.Name));
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(IList<string>), (propertyInfo, entity) =>
            {
                var entityList = (IList<string>)propertyInfo.GetValue(entity);
                IList list = entityList.ToArray();
                return new Bin(propertyInfo.Name, list);

            }, (record, entity, propertyInfo) =>
            {
                IList list = record.GetList(propertyInfo.Name);
                List<string> entityList = list.Cast<string>().ToList();
                propertyInfo.SetValue(entity, entityList);
            }));

            AddTypeSupport(new GenericAeroPropertySupport(typeof(IDictionary<string, string>), (propertyInfo, entity) =>
            {
                var entityDict = (IDictionary<string, string>)propertyInfo.GetValue(entity);
                IDictionary dict = new ListDictionary();
                foreach (var pair in entityDict)
                {
                    dict.Add(pair.Key, pair.Value);
                }
                return new Bin(propertyInfo.Name, dict);

            }, (record, entity, propertyInfo) =>
            {
                IDictionary dict = record.GetMap(propertyInfo.Name);
                IDictionary<string, string> dictGeneric = new ConcurrentDictionary<string, string>();
                foreach (DictionaryEntry dictionaryEntry in dict)
                {
                    dictGeneric.Add((string)dictionaryEntry.Key, (string)dictionaryEntry.Value);
                }
                propertyInfo.SetValue(entity, dictGeneric);
            }));


        }
    }
}
