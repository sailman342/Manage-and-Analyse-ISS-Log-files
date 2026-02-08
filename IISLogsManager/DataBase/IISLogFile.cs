using IISLogsManager.Database;
using System.Reflection;

namespace IISLogsManager.DataBase
{
    [DB_TableName("ISSLogFiles", "PRIMARY KEY(ID)")]
    public class IISLogFile
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

        [DB_ElementName("FilePath", "VARCHAR(255) NOT NULL", "Full log file path")]
        public string FilePath { get; set; } = "";

        [DB_ElementName("FileName", "VARCHAR(255) NOT NULL", "Log file name")]
        public string FileName { get; set; } = "";



        // properties extracted from file data

        [DB_ElementName("NumLogLines", "INT NOT NULL", "Number of log lines")]
        public int NumLogLines { get; set; } = 0;

        [DB_ElementName("StartDate", "DATE", "First date in file")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.MaxValue);

        [DB_ElementName("StartTime", "TIME", "First time in start date")]
        public TimeOnly StartTime { get; set; } = TimeOnly.FromDateTime(DateTime.MaxValue);

        [DB_ElementName("EndDate", "DATE", "Last date in file")]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.MinValue);

        [DB_ElementName("EndTime", "TIME", "Last time in end date")]
        public TimeOnly EndTime { get; set; } = TimeOnly.FromDateTime(DateTime.MinValue);

        // Used internally

        public bool IsMarked { get; set; } = false;

        public FileInfo FileInfo { get; set; } = default!;

    }
}
