

namespace IISLogsManager.ILMAppConfigNameSpace;

public class ILMAppContextRecord
{
    public string urlContextID = "";
    public DateTime lastAccessDateTime = DateTime.MinValue;
    public DateTime validityDateTime = DateTime.MinValue;

    public ILMAppContext ILMAppContext = new();
}

public interface IILMAppContextServices
{
    void SetILMAppContext(ILMAppContext appContext, string urlContextID);

    ILMAppContext GetILMAppContext(string urlContextID);

    void DeleteILMAppContextRecord(string urlContextID);

    void DeleteAllILMAppContextRecords();

    List<ILMAppContextRecord> GetILMAppContextRecordsList();

}
public class ILMAppContextServices : IILMAppContextServices
{
    public List<ILMAppContextRecord> ILMAppContextRecordsList = [];

    public List<ILMAppContextRecord> GetILMAppContextRecordsList()
    {
        DeleteValidityExhaustedRecords();
        return ILMAppContextRecordsList;
    }


    public ILMAppContext GetILMAppContext(string urlContextID)
    {
        // empty string as arg will return a new one !
        ILMAppContextRecord appContextRecord = GetILMAppContextRecordFromList(urlContextID);
        if (appContextRecord.urlContextID != "")
        {
            appContextRecord.lastAccessDateTime = DateTime.Now;
            appContextRecord.validityDateTime = DateTime.Now.AddMinutes(60); // Not used for now
            return appContextRecord.ILMAppContext;
        }
        else
        {
            // return default
            ILMAppContext appContext = new();
            return appContext;
        }
    }

    private ILMAppContextRecord GetILMAppContextRecordFromList(string urlContextID)
    {
        // empty string as arg will return a new one !
        DeleteValidityExhaustedRecords();
        foreach (ILMAppContextRecord appContextRecord in ILMAppContextRecordsList)
        {
            if (appContextRecord.urlContextID == urlContextID)
            {
                return appContextRecord;
            }
        }
        return new();
    }

    public void SetILMAppContext(ILMAppContext appContextArg, string urlContextID)
    {
        DeleteValidityExhaustedRecords();
        DeleteILMAppContextRecord(urlContextID);
        // Always allow setting an appcontext !
        if (appContextArg != null && urlContextID != "")
        {
            appContextArg.UrlContextID = urlContextID;
            ILMAppContextRecord appContextRecord = new()
            {
                urlContextID = urlContextID,
                lastAccessDateTime = DateTime.Now,
                validityDateTime = DateTime.Now.AddMinutes(60), // Not used for now
                ILMAppContext = appContextArg
            };
            if(!appContextArg.ILMAppConfig.IsLoaded)
            {
                appContextArg.ILMAppConfig = ILMAppConfiguration.GetAppConfig();
            }
            ILMAppContextRecordsList.Add(appContextRecord);
        }
    }

    public void DeleteILMAppContextRecord(string urlContextID)
    {
        DeleteValidityExhaustedRecords();
        foreach (ILMAppContextRecord appContextRecord in ILMAppContextRecordsList)
        {
            if (appContextRecord.urlContextID == urlContextID)
            {
                ILMAppContextRecordsList.Remove(appContextRecord);
                return;
            }
        }
    }

    public void DeleteAllILMAppContextRecords()
    {
        ILMAppContextRecordsList.Clear();
    }

    public void DeleteValidityExhaustedRecords()
    {
        foreach (ILMAppContextRecord appContextRecord in ILMAppContextRecordsList)
        {
            if (appContextRecord.validityDateTime < DateTime.Now)
            {
                ILMAppContextRecordsList.Remove(appContextRecord);
                DeleteValidityExhaustedRecords();
                break;
            }
        }
    }
}
