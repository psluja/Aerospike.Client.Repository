# Aerospike.Client.Repository
This is simple repository pattern implemention on top of Aerospike Client.

#Why?
For simplicity


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
