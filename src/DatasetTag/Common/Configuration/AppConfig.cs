#region ========================================================================= USING =====================================================================================
using System.IO;
using Newtonsoft.Json;
#endregion

namespace DatasetTag.Common.Configuration;

public class AppConfig
{
    #region ==================================================================== PROPERTIES =================================================================================
    public ApplicationConfigDto? Application { get; set; }
    public DefaultCategoriesConfigDto? DefaultCategories { get; set; }
    [JsonIgnore]
    public string? ConfigurationFilePath { get; set; }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Saves the application's configuration settings
    /// </summary>
    public void UpdateConfiguration()
    {
        if (!string.IsNullOrEmpty(ConfigurationFilePath) && File.Exists(ConfigurationFilePath))
            File.WriteAllText(ConfigurationFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        else
            throw new FileNotFoundException("Configuration file does not exist!");
    }
    #endregion
}
