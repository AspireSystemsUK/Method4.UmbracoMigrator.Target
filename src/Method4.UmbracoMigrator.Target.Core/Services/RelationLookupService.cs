using Method4.UmbracoMigrator.Target.Core.Models.DataModels;
using Method4.UmbracoMigrator.Target.Core.Options;
using Method4.UmbracoMigrator.Target.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Method4.UmbracoMigrator.Target.Core.Services
{
    public class RelationLookupService : IRelationLookupService
    {
        private readonly IMigrationLookupsRepository _repository;
        private readonly ILogger<RelationLookupService> _logger;

        public RelationLookupService(MigrationLookupsRepository repository,
            MigrationLookupsRepositoryWithCache repositoryWithCache,
            ILogger<RelationLookupService> logger,
            IOptions<MigratorTargetSettings> settings)
        {
            _logger = logger;

            if (settings.Value.EnableMigrationLookupTableRuntimeCaching)
            {
                _logger.LogDebug("Using MigrationLookupsRepositoryWithCache");
                _repository = repositoryWithCache;
            }
            else
            {
                _logger.LogDebug("Using MigrationLookupsRepository");
                _repository = repository;
            }
        }

        public NodeRelation? GetRelationByOldId(string oldId)
        {
            return _repository.Get("OldId", oldId);
        }

        public NodeRelation? GetRelationByNewId(string newId)
        {
            return _repository.Get("NewId", newId);
        }

        public NodeRelation? GetRelationByOldKey(Guid oldKey)
        {
            return GetRelationByOldKey(oldKey.ToString());
        }

        public NodeRelation? GetRelationByOldKey(string oldKey)
        {
            return _repository.Get("OldKey", oldKey);
        }

        public NodeRelation? GetRelationByNewKey(Guid newKey)
        {
            return GetRelationByNewKey(newKey.ToString());
        }

        public NodeRelation? GetRelationByNewKey(string newKey)
        {
            return _repository.Get("NewKey", newKey);
        }

        public void StoreNewRelation(string newId, string oldId, Guid newKey, Guid oldKey)
        {
            _repository.Insert(newId, oldId, newKey.ToString(), oldKey.ToString());
        }

        public void StoreNewRelation(string newId, string oldId, string newKey, string oldKey)
        {
            if (Guid.TryParse(newKey, out var parsedNewKey) == false)
            {
                _logger.LogError("Failed to parse '{newKey}'", newKey);
                throw new Exception($"Failed to parse '{newKey}'");
            }

            if (Guid.TryParse(oldKey, out var parsedOldKey) == false)
            {
                _logger.LogError("Failed to parse '{oldKey}'", oldKey);
                throw new Exception($"Failed to parse '{oldKey}'");
            }

            StoreNewRelation(newId, oldId, parsedNewKey, parsedOldKey);
        }

        public int CountRelations()
        {
            return _repository.Count();
        }

        public void DeleteAllRelations()
        {
            _repository.DeleteAll();
        }
    }
}