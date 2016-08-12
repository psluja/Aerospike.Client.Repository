# Aerospike.Client.Repository
This is simple repository pattern implementation on top of Aerospike Client. Aerospike DB uses records and bins inside them. This repository implementation maps properties of your entity to proper bin type and save it as a single record. Property types that can be use are: ```int, string, bool, double, byte, long, IList<string>, IDictionary<string, string>```. Every type that is not listed will be serialized using BinaryFormatter and saved as binary data bin.

# Entities
```C#
          public class MyCustomEntity:IAeroEntity
          {
                    public string Key { get; set; }
                    public string Name { get; set; }
          }
```  
# Basic usage
```C#
          using (var client = new AsyncClient(null, "localhost", 3000))
          {
              AerospikeRepository repository = new AerospikeRepository(client, "test");
          
              repository.DropStringIndex<MyCustomEntity>(entity => entity.Name);
              repository.CreateStringIndex<MyCustomEntity>(entity => entity.Name);
          
              MyCustomEntity myCustomEntity = new MyCustomEntity
              {
                  Key = "myUniqueKey_1",
                  Name = "someName"
              };
          
              repository.AddEntity(myCustomEntity).Wait();
          
              var entities = repository.GetEntitiesEquals<MyCustomEntity>(i => i.Name, "someName").Take(1).ToList();
          
          }
```
# Custom type
To add support of a custom type, just create a class that inherits from ```Aerospike.Client.Repository.AeroTypeSupport``` class and implement two methods:
```C#
    [Serializable]
    public class MyObject
    {
        public string Value { get; set; }
    }

    public class MyCustomTypes:AeroTypeSupport
    {
        public override bool CreateBin<TEntity>(PropertyInfo propertyInfo, TEntity entity, out Bin bin)
        {
            if (propertyInfo.PropertyType == typeof (MyObject))
            {
                var myObject = (MyObject) propertyInfo.GetValue(entity);
                bin = new Bin(propertyInfo.Name, myObject.Value);
                return true;
            }
            
            return base.CreateBin(propertyInfo, entity, out bin);
        }

        public override bool SetPropertyValue<TEntity>(PropertyInfo propertyInfo, TEntity entity, Record record)
        {
            if (propertyInfo.PropertyType == typeof (MyObject))
            {
                MyObject myObject = new MyObject
                {
                    Value = record.GetString(propertyInfo.Name)
                };
                propertyInfo.SetValue(entity, myObject);

                return true;
            }
            return base.SetPropertyValue(propertyInfo, entity, record);
        }
    }
``` 
... and the just:
```C#
erospikeRepository repository = new AerospikeRepository(client, "test", new AerospikeEntityMapper(new MyCustomTypes()));

```

#Custom serialization
By default repository uses BinnaryFormatter to serialized complex types. But if you want to add your own serializer like Json or Protobuf then implement interface ```Aerospike.Client.Repository.ISerializer```:
```C#
    public interface ISerializer
    {
        object Deserialize(Type type, byte[] data);
        byte[] Serialize(object o);
    }
```

...and use it like that:
```C#
AerospikeRepository repository = new AerospikeRepository(client, "test", new AerospikeEntityMapper(new MyCustomSerializer()));
```
