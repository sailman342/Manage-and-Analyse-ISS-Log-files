using IISLogsManager.AppConfig;
using IISLogsManager.Database;
using static IISLogsManager.AppConfig.AppConfiguration;

namespace IISLogsManager.DataBase
{
    public class IISLogRecords : List<IISLogRecord>
    {
        public IISLogRecords()
        {
            this.Clear();
        }

        public IISLogRecords(string filePath)
        {
            this.Clear();
            if (File.Exists(filePath))
            {
                // Store each line in array of strings
                string[] lines = File.ReadAllLines(filePath);
                int lineNumber = 0;
                string definitions = "";
                foreach (string ln in lines)
                {
                    lineNumber++;
                    if(ln.StartsWith("#Fields:"))
                    {
                        definitions = ln;
                    }

                    if (!ln.StartsWith('#'))
                    {
                        IISLogRecord iisLogRecord = new();

                        string[] splittedLine = ln.Split(" ");

                        // Set file location properties
                        iisLogRecord.LineNumber=lineNumber;
                        iisLogRecord.OriginFilePath = filePath;
                        iisLogRecord.SubFolder = TheAppConfiguration.IISSiteLogSubFolderName;
                        iisLogRecord.DomainName = TheAppConfiguration.IISSiteDomainName;
                        iisLogRecord.DomainID = TheAppConfiguration.IISSiteID;

                        // Parse the log line to set log record properties
                        iisLogRecord.ParseFromLogLine(splittedLine, definitions);

                        this.Add(iisLogRecord);
                    }
                }
            }

        }
    }
}
