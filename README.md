# Aerospike.Client.Repository

# Entities
          public class MyCustomEntity:IAeroEntity
          {
                    public string Key { get; set; }
                    public string Name { get; set; }
          }
    
# Basic usage

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
