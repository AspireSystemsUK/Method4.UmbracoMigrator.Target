using Method4.UmbracoMigrator.Target.Core.Models.DataModels;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Method4.UmbracoMigrator.Target.Core.Repositories
{
    public class MigrationLookupsRepositoryWithCache : MigrationLookupsRepository
    {
        private readonly IAppPolicyCache _runtimeCache;
        private readonly ILogger<MigrationLookupsRepositoryWithCache> _logger;

        private const string CacheKey = "MigrationRelationLookup";
        private const int CacheTimeoutDays = 7;

        public MigrationLookupsRepositoryWithCache(
            AppCaches appCaches,
            IUmbracoMapper umbracoMapper,
            IScopeProvider scopeProvider,
            ILogger<MigrationLookupsRepository> logger,
            ILogger<MigrationLookupsRepositoryWithCache> logger2)
            : base(umbracoMapper, scopeProvider, logger)
        {
            _runtimeCache = appCaches.RuntimeCache;
            _logger = logger2;
        }

        public override NodeRelation? Get(string columnName, string value)
        {
            var relation = _runtimeCache.GetCacheItem<NodeRelation>($"{CacheKey}_{columnName}_{value}");
            if (relation != null) { return relation; }
            
            relation = base.Get(columnName, value);
            if (relation == null) { return relation; }

            InsertIntoRuntimeCache(relation);
            return relation;
        }

        public override void Insert(string newId, string oldId, string newKey, string oldKey)
        {
            base.Insert(newId, oldId, newKey, oldKey);
            var relationLookup = base.Get("NewId", newId);
            if (relationLookup != null)
            {
                InsertIntoRuntimeCache(relationLookup);
            }
        }

        public override void DeleteAll()
        {
            base.DeleteAll();
            try
            {
                _runtimeCache.ClearOfType<NodeRelation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove Migration Relations from the Umbraco runtime cache");
                throw;
            }
        }

        /// <summary>
        /// Adds the node relation to the runtime cache if it doesn't exist.
        /// </summary>
        /// <param name="relation"></param>
        private void InsertIntoRuntimeCache(NodeRelation relation)
        {
            var newIdCacheKey = $"{CacheKey}_NewId_{relation.NewId}";
            var oldIdCacheKey = $"{CacheKey}_OldId_{relation.OldId}";
            var newKeyCacheKey = $"{CacheKey}_NewKey_{relation.NewKeyAsString}";
            var oldKeyCacheKey = $"{CacheKey}_OldKey_{relation.OldKeyAsString}";

            try
            {
                if (_runtimeCache.GetCacheItem<NodeRelation>(newIdCacheKey) == null)
                {
                    _runtimeCache.GetCacheItem(
                        newIdCacheKey,
                        () => relation,
                        TimeSpan.FromDays(CacheTimeoutDays));
                }

                if (_runtimeCache.GetCacheItem<NodeRelation>(oldIdCacheKey) == null)
                {
                    _runtimeCache.GetCacheItem(
                        oldIdCacheKey,
                        () => relation,
                        TimeSpan.FromDays(CacheTimeoutDays));
                }

                if (_runtimeCache.GetCacheItem<NodeRelation>(newKeyCacheKey) == null)
                {
                    _runtimeCache.GetCacheItem(
                        newKeyCacheKey,
                        () => relation,
                        TimeSpan.FromDays(CacheTimeoutDays));
                }

                if (_runtimeCache.GetCacheItem<NodeRelation>(oldKeyCacheKey) == null)
                {
                    _runtimeCache.GetCacheItem(
                        oldKeyCacheKey,
                        () => relation,
                        TimeSpan.FromDays(CacheTimeoutDays));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert Migration Relation into the Umbraco runtime cache");
                throw;
            }
        }
    }
}