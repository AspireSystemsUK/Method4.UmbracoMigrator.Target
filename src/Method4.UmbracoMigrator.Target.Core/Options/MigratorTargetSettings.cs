using System.ComponentModel;

namespace Method4.UmbracoMigrator.Target.Core.Options
{
    public class MigratorTargetSettings
    {
        /// <summary>
        /// For use with the Skybrud Redirects package.
        /// Enables the UniqueMediaPathScheme IMediaPathScheme.
        /// This will automatically generate a re-direct if a migrated media item's file is changed.
        /// Please see the documentation for more information.
        /// </summary>
        [DefaultValue(false)]
        public bool EnableMediaRedirectGeneration { get; set; } = false;

        /// <summary>
        /// The property alias of the Media File Upload property.
        /// </summary>
        [DefaultValue("umbracoFile")]
        public string? MediaFileUploadPropertyAlias { get; set; }

        /// <summary>
        /// Enable the Runtime Caching on the MigrationLookups repository
        /// </summary>
        [DefaultValue(true)]
        public bool EnableMigrationLookupTableRuntimeCaching { get; set; } = true;
    }
}