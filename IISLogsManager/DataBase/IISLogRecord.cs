using IISLogsManager.Database;
using System.Reflection;

namespace IISLogsManager.DataBase
{
    [DB_TableName("IISLogRecords", "PRIMARY KEY(ID)")]
    public class IISLogRecord
    {
        [DB_ElementName("ID", "INT  NOT NULL AUTO_INCREMENT", "ID Identificateur unique géré par le système")]
        public int ID { get; set; } = 0; // 0 means not in database !

        // Properties corresponding to file location

        [DB_ElementName("DomainName", "VARCHAR(255) NOT NULL", "Config-Subdomain/domain name")]
        public string DomainName { get; set; } = "";

        [DB_ElementName("DomainID", "VARCHAR(255) NOT NULL", "Config-Domain unique ID int-tostring")]
        public string DomainID { get; set; } = "";

        [DB_ElementName("SubFolder", "VARCHAR(255) NOT NULL", "Config-Subfolder as created by IIS")]
        public string SubFolder { get; set; } = "";

        [DB_ElementName("OriginFile", "VARCHAR(255) NOT NULL", "origin log file path")]
        public string OriginFilePath { get; set; } = "";

        [DB_ElementName("LineNumber", "INT NOT NULL","Line number in the log file")]
        public int LineNumber { get; set; } = 0;

        // Properties corresponding to log record from file
        // https://docs.trellix.com/fr-FR/bundle/enterprise-security-manager-data-source-configuration-guide/page/UUID-55cefe27-1586-5c9f-cadf-1eea7e6f98c0.html
        //# Fields: date time


        [DB_ElementName("Date", "DATE", "Date of event")]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [DB_ElementName("Time", "TIME","Time of event")]
        public TimeOnly Time { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

        // We use the IIS nomenclature for the following fields
        //s-sitename s-computername s-ip cs-method cs-uri-stem cs-uri-query

        [DB_ElementName("s-sitename", "VARCHAR(255) NOT NULL", "Site name")]
        public string SiteName { get; set; } = "";

        [DB_ElementName("s-computername", "VARCHAR(255) NOT NULL", "Server name")]
        public string ServerName { get; set; } = "";

        [DB_ElementName("s-ip", "VARCHAR(255) NOT NULL", "Destination IP")]
        public string DestinationIP { get; set; } = "";

        [DB_ElementName("cs-method", "VARCHAR(255) NOT NULL", "Command")]
        public string Command { get; set; } = "";

        [DB_ElementName("cs-uri-stem", "VARCHAR(255) NOT NULL", "Object")]
        public string Object { get; set; } = "";

        [DB_ElementName("cs-uri-query", "VARCHAR(255) NOT NULL", "Query String")]
        public string QueryString { get; set; } = "";

        //s-port cs-username c-ip cs-version cs(User-Agent) cs(Cookie) cs(Referer) cs-host

        [DB_ElementName("s-port", "VARCHAR(255) NOT NULL", "Destination Port")]
        public string DestinationPort { get; set; } = "";

        [DB_ElementName("s-username", "VARCHAR(255) NOT NULL", "Domain")]
        public string Domain { get; set; } = "";

        [DB_ElementName("c-ip", "VARCHAR(255) NOT NULL", "Source IP")]
        public string SourceIP { get; set; } = "";

        [DB_ElementName("cs-version", "VARCHAR(255) NOT NULL", "HTTP Version")]
        public string HTTPVersion { get; set; } = "";

        [DB_ElementName("cs-User-Agent", "VARCHAR(255) NOT NULL", "User Agent")]
        public string UserAgent { get; set; } = "";

        [DB_ElementName("cs-Cookie", "VARCHAR(255) NOT NULL", "Cookie")]
        public string Cookie { get; set; } = "";

        [DB_ElementName("cs-Referer", "VARCHAR(255) NOT NULL", "Referer")]
        public string Referer { get; set; } = "";

        [DB_ElementName("cs-hostt", "VARCHAR(255) NOT NULL", "Hostname")]
        public string Hostname { get; set; } = "";

        //sc-status sc-substatus sc-win32-status sc-bytes cs-bytes time-taken

        [DB_ElementName("sc-status", "VARCHAR(255) NOT NULL", "Status")]
        public string Status { get; set; } = "";

        [DB_ElementName("sc-substatus", "VARCHAR(255) NOT NULL", "SubStatus/Action")]
        public string SubStatus { get; set; } = "";

        [DB_ElementName("sc-win32-status", "VARCHAR(255) NOT NULL", "Win32 Status/Error Code")]
        public string Win32Status { get; set; } = "";

        [DB_ElementName("sc-bytes", "VARCHAR(255) NOT NULL", "Bytes Sent")]
        public string BytesSent { get; set; } = "";

        [DB_ElementName("cs-bytes", "VARCHAR(255) NOT NULL", "Bytes Received")]
        public string BytesReceived { get; set; } = "";

        [DB_ElementName("time-taken", "VARCHAR(255) NOT NULL", "Time Taken (ms)")]
        public string TimeTaken { get; set; } = "";

        // Used internally for programmatic purposes

        public bool IsMarked { get; set; } = false; // Used to mark/unmark records for selection

        public string GetLogRecordHeaderCSVString()
        {
            string outStr = "";
            foreach (PropertyInfo property in typeof(IISLogRecord).GetProperties())
            {
                // We only want properties with the DB_ElementName attribute, to avoid including properties that are not meant to be stored in the database
                if (property.GetCustomAttribute<DB_ElementName>() != null)
                {
                    DB_ElementName? elementNameAttribute = property.GetCustomAttribute<DB_ElementName>();
                    outStr += (elementNameAttribute?.ElementName ?? string.Empty).Replace(';','_') + " ;";
                }
            }
            return outStr +"\r\n";
        }
        public string GetLogRecordValuesCSVString()
        {
            string outStr = "";
            foreach (PropertyInfo property in typeof(IISLogRecord).GetProperties())
            {
                // We only want properties with the DB_ElementName attribute, to avoid including properties that are not meant to be stored in the database
                if (property.GetCustomAttribute<DB_ElementName>() != null)
                {
                    outStr += (property.GetValue(this)?.ToString() ?? string.Empty).Replace(';', '_') + " ;";
                }
            }
            return outStr;
        }
        internal void ParseFromLogLine(string[] splittedLine, string definitions)
        {
            string[] defParts = definitions.Replace("#Fields: ", "").Split(" ");
            for (int i = 0; i < defParts.Length; i++)
            {
                string defPart = defParts[i];
                string value = "";
                if (i < splittedLine.Length)
                {
                    value = splittedLine[i];
                }
                switch (defPart)
                {
                    case "date":
                        Date = DateOnly.Parse(value);
                        break;
                    case "time":
                        Time = TimeOnly.Parse(value);
                        break;
                    case "s-sitename":
                        SiteName = value;
                        break;
                    case "s-computername":
                        ServerName = value;
                        break;
                    case "s-ip":
                        DestinationIP = value;
                        break;
                    case "cs-method":
                        Command = value;
                        break;
                    case "cs-uri-stem":
                        Object = value;
                        break;
                    case "cs-uri-query":
                        QueryString = value;
                        break;
                    case "s-port":
                        DestinationPort = value;
                        break;
                    case "csusername":
                        Domain = value;
                        break;
                    case "c-ip":
                        SourceIP = value;
                        break;
                    case "cs-version":
                        HTTPVersion = value;
                        break;
                    case "cs(User-Agent)":
                        UserAgent = value;
                        break;
                    case "cs(Cookie)":
                        Cookie = value;
                        break;
                    case "cs(Referer)":
                        Referer = value;
                        break;
                    case "cs-host":
                        Hostname = value;
                        break;
                    case "sc-status":
                        Status = value;
                        break;
                    case "sc-substatus":
                        SubStatus = value;
                        break;
                    case "sc-win32-status":
                        Win32Status = value;
                        break;
                    case "sc-bytes":
                        BytesSent = value;
                        break;
                    case "cs-bytes":
                        BytesReceived = value;
                        break;
                    case "time-taken":
                        TimeTaken = value;
                        break;
                }
            }
        }
    }
}
