using Method4.UmbracoMigrator.Target.Core.Models.DataModels;

namespace Method4.UmbracoMigrator.Target.Core.Repositories;

public interface IMigrationLookupsRepository
{
    NodeRelation? Get(string columnName, string value);
    void Insert(string newId, string oldId, string newKey, string oldKey);
    int Count();
    void DeleteAll();
}