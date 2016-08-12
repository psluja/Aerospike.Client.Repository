using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;

namespace Aerospike.Client.Repository
{
    public class AerospikeRepository
        : IAerospikeRepository
    {
        private readonly AsyncClient _aerospikeClient;
        private readonly IAerospikeEntityMapper _aerospikeEntityMapper;
        private readonly IIndexNameResolver _indexNameResolver;
        private readonly WritePolicy _addWritePolicy;
        private readonly WritePolicy _updateWritePolicy;
        private readonly string _namespace;
        public CancellationTokenSource CancellationTokenSource { get; private set; }


        public AerospikeRepository(AsyncClient aerospikeClient, string ns) : this(aerospikeClient, ns, new AerospikeEntityMapper(), new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, IAeroTypeSupport aeroTypeSupport, ISerializer serializer, IBinaryPresenter binaryPresenter)
            : this(aerospikeClient, ns, new AerospikeEntityMapper(aeroTypeSupport, serializer, binaryPresenter), new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, IAeroTypeSupport aeroTypeSupport, ISerializer serializer)
            : this(aerospikeClient, ns, new AerospikeEntityMapper(aeroTypeSupport, serializer, null), new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, ISerializer serializer)
            : this(aerospikeClient, ns, new AerospikeEntityMapper(serializer), new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, IAeroTypeSupport aeroTypeSupport)
            : this(aerospikeClient, ns, new AerospikeEntityMapper(aeroTypeSupport), new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, IAerospikeEntityMapper aerospikeEntityMapper) : this(aerospikeClient, ns, aerospikeEntityMapper, new DefaultIndexNameResolver())
        {
        }

        public AerospikeRepository(AsyncClient aerospikeClient, string ns, IAerospikeEntityMapper aerospikeEntityMapper, IIndexNameResolver indexNameResolver)
        {
            _aerospikeClient = aerospikeClient;
            _aerospikeEntityMapper = aerospikeEntityMapper;
            _indexNameResolver = indexNameResolver;
            _namespace = ns;
            CancellationTokenSource = new CancellationTokenSource();
            _addWritePolicy = new WritePolicy
            {
                recordExistsAction = RecordExistsAction.CREATE_ONLY
            };

            _updateWritePolicy = new WritePolicy
            {
                recordExistsAction = RecordExistsAction.REPLACE_ONLY
            };
        }

        private string GetIndexName<TEntity>(string propertyName) where TEntity : IAeroEntity, new()
        {
            return _indexNameResolver.GetIndexName<TEntity>(propertyName);
        }

        public void CreateStringIndex<TEntity>(string propertyName) where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            string indexName = GetIndexName<TEntity>(propertyName);
            var resultTask = _aerospikeClient
                .CreateIndex(null, _namespace, entityType.Name, indexName, propertyName,
                    IndexType.STRING);
            resultTask.Wait();
        }

        public void DropStringIndex<TEntity>(string propertyName) where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            string indexName = GetIndexName<TEntity>(propertyName);
            _aerospikeClient.DropIndex(null, _namespace, entityType.Name, indexName);
        }


        public IEnumerable<TEntity> GetEntitiesEquals<TEntity>(string propertyName, string value) where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            Statement statement = new Statement()
            {
                Namespace = _namespace,
                SetName = entityType.Name,
                BinNames = _aerospikeEntityMapper
                .GetPropertiesNames<TEntity>()
                .ToArray(),
                IndexName = GetIndexName<TEntity>(propertyName)
            };
            statement.SetFilters(Filter.Equal(propertyName, value));

            using (RecordSet records = _aerospikeClient.Query(null, statement))
            {
                while (records.Next())
                {
                    var record = records.Record;
                    var entity = _aerospikeEntityMapper.GetEntity<TEntity>(record);
                    yield return entity;
                }
            }
        }

        public IEnumerable<TEntity> GetEntitiesRange<TEntity>(string propertyName, long begin, long end) where TEntity : IAeroEntity, new()
        {
            Type entityType = typeof(TEntity);
            Statement statement = new Statement()
            {
                Namespace = _namespace,
                SetName = entityType.Name,
                BinNames = _aerospikeEntityMapper
                .GetPropertiesNames<TEntity>()
                .ToArray(),
                IndexName = GetIndexName<TEntity>(propertyName)
            };
            statement.SetFilters(Filter.Range(propertyName, begin, end));

            using (RecordSet records = _aerospikeClient.Query(null, statement))
            {
                while (records.Next())
                {
                    var record = records.Record;
                    var entity = _aerospikeEntityMapper.GetEntity<TEntity>(record);
                    yield return entity;
                }
            }
        }

        public Task<IEnumerable<TEntity>> GetEntities<TEntity>(params string[] keys) where TEntity : IAeroEntity, new()
        {
            Key[] aeroKeys = keys.Select(i => new Key(_namespace, typeof (TEntity).Name, i)).ToArray();
            Task<Record[]> recordsTask = _aerospikeClient.Get(new BatchPolicy(), CancellationTokenSource.Token, aeroKeys);

            return recordsTask.ContinueWith(p =>
            {
                return p.Result.Select(record => _aerospikeEntityMapper.GetEntity<TEntity>(record));
            });
        }

        public Task AddEntity<TEntity>(TEntity entity) where TEntity : IAeroEntity, new()
        {
            Key aeroKey = new Key(_namespace, typeof(TEntity).Name, entity.Key);
            Bin[] bins = _aerospikeEntityMapper.CreateBinsArray(entity);
            return _aerospikeClient.Put(_addWritePolicy, CancellationTokenSource.Token, aeroKey,bins);
        }

        public Task UpdateEntity<TEntity>(TEntity entity) where TEntity : IAeroEntity, new()
        {
            Key aeroKey = new Key(_namespace, typeof(TEntity).Name, entity.Key);
            Bin[] bins = _aerospikeEntityMapper.CreateBinsArray(entity);
            return _aerospikeClient.Put(_updateWritePolicy, CancellationTokenSource.Token, aeroKey, bins);
        }

        public Task DeleteEntity<TEntity>(string key) where TEntity : IAeroEntity, new()
        {
            Key aeroKey = new Key(_namespace, typeof(TEntity).Name, key);
            return _aerospikeClient.Delete(null, CancellationTokenSource.Token, aeroKey);
        }
    }
}
