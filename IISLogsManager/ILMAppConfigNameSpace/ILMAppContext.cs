using IISLogsManager.DataBase;

namespace IISLogsManager.ILMAppConfigNameSpace;

public class ILMAppContext
{
    // ILMAppConfig.IsLoaded = false
    public ILMAppConfiguration ILMAppConfig { get; set; } = new();

    public bool IsLoaded { get; set; } = false;
    public bool IsLoggedIn { get; set; } = false;
    public string LoggedUser { get; set; } = "";

    public string UrlContextID { get; set; } = "";

    // set up by siteselecion page

    public bool IISSiteSelected { get; set; } = false;
    public string IISSiteID { get; set; } = "";
    public string IISSiteLogSubFolderName { get; set; } = "";
    public string IISSiteLogSubFolderPath { get; set; } = ""; // TODO replace everywhere
    public string IISSiteDomainName { get; set; } = "";

    public IISLogFile IISSiteSelectedLogFile { get; set; } = new();
    public IISLogRecord IISSiteSelectedLogFileRow { get; set; } = new();

    public ILMAppContext(bool loadConfig = true)
    {
        if (loadConfig)
        {
            ILMAppConfig = ILMAppConfiguration.GetAppConfig();
        }
    }
}
