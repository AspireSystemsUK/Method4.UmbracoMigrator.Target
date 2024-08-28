using Method4.UmbracoMigrator.Target.Core.CustomDbTables.NPoco;
using Method4.UmbracoMigrator.Target.Core.Models.DataModels;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Method4.UmbracoMigrator.Target.Core.Repositories
{
    public partial class MigrationLookupsRepository : IMigrationLookupsRepository
    {
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<MigrationLookupsRepository> _logger;
        private const string TableName = "MigrationLookups";

        public MigrationLookupsRepository(IUmbracoMapper umbracoMapper, IScopeProvider scopeProvider, ILogger<MigrationLookupsRepository> logger)
        {
            _umbracoMapper = umbracoMapper;
            _scopeProvider = scopeProvider;
            _logger = logger;
        }

        public virtual NodeRelation? Get(string columnName, string value)
        {
            using var scope = _scopeProvider.CreateScope();
            var queryResult = scope.Database.Fetch<NodeRelationLookupPoco>($"SELECT * From {TableName} WHERE {columnName} = @0", value);
            scope.Complete();

            var foundLookup = queryResult.FirstOrDefault(); // There should only ever be 1
            if (foundLookup == null)
            {
                _logger.LogDebug("Lookup Not found for {lookupType}: {lookupValue}", columnName, value);
                return null;
            }

            var relation = _umbracoMapper.Map<NodeRelation>(foundLookup);
            if (relation == null)
            {
                _logger.LogError("Unable to map NodeRelationLookupPoco to NodeRelation for {lookupType}: {value}", columnName, value);
                throw new Exception($"Lookup for {columnName}: '{value}' could not be mapped, result from the Umbraco Mapper was null");
            }

            return relation;
        }

        public virtual void Insert(string newId, string oldId, string newKey, string oldKey)
        {
            var relationLookup = new NodeRelationLookupPoco()
            {
                NewId = newId,
                OldId = oldId,
                NewKey = newKey,
                OldKey = oldKey
            };

            try
            {
                using var scope = _scopeProvider.CreateScope();
                scope.Database.Insert<NodeRelationLookupPoco>(relationLookup);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store new migration node relation");
                throw;
            }
        }

        public virtual int Count()
        {
            using var scope = _scopeProvider.CreateScope();
            var queryResult = scope.Database.Fetch<NodeRelationLookupPoco>($"SELECT * From {TableName}");
            scope.Complete();

            return queryResult.Count;
        }

        public virtual void DeleteAll()
        {
            _logger.LogInformation("Deleting all Key relations");
            try
            {
                using var scope = _scopeProvider.CreateScope();
                var queryResult = scope.Database.Execute($"DELETE FROM {TableName}");
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all relations from the {tableName} table", TableName);
                throw;
            }
        }
    }
}
