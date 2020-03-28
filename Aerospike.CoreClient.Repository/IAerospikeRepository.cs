using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Aerospike.CoreClient.Repository
{
    public interface IAerospikeRepository
    {
        IEnumerable<TEntity> GetEntitiesEquals<TEntity>(string propertyName, string value) where TEntity : IAeroEntity, new();
        IEnumerable<TEntity> GetEntitiesRange<TEntity>(string propertyName, long begin, long end) where TEntity : IAeroEntity, new();
        Task<IEnumerable<TEntity>> GetEntities<TEntity>(params string[] keys) where TEntity : IAeroEntity, new();
        Task AddEntity<TEntity>(TEntity entity) where TEntity : IAeroEntity, new();
        Task UpdateEntity<TEntity>(TEntity entity) where TEntity : IAeroEntity, new();
        Task DeleteEntity<TEntity>(string key) where TEntity : IAeroEntity, new();
    }

    public static class AerospikeRepositoryExtension
    {
        public static IEnumerable<TEntity> GetEntitiesRange<TEntity>(this IAerospikeRepository repo, Expression<Func<TEntity, string>> expression, long begin, long end)
            where TEntity : IAeroEntity, new()
        {
            string name = ((MemberExpression)expression.Body).Member.Name;
            return repo.GetEntitiesRange<TEntity>(name, begin, end);
        }

        public static IEnumerable<TEntity> GetEntitiesEquals<TEntity>(this IAerospikeRepository repo, Expression<Func<TEntity, string>> expression, string value)
            where TEntity : IAeroEntity, new()
        {
            string name = ((MemberExpression)expression.Body).Member.Name;
            return repo.GetEntitiesEquals<TEntity>(name, value);
        }

        public static void CreateStringIndex<TEntity>(this AerospikeRepository repo, Expression<Func<TEntity, string>> expression)
            where TEntity : IAeroEntity, new()
        {
            string name = ((MemberExpression)expression.Body).Member.Name;
            repo.CreateStringIndex<TEntity>(name);
        }

        public static void DropStringIndex<TEntity>(this AerospikeRepository repo, Expression<Func<TEntity, string>> expression)
            where TEntity : IAeroEntity, new()
        {
            string name = ((MemberExpression)expression.Body).Member.Name;
            repo.DropStringIndex<TEntity>(name);
        }
    }
}