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
                List<string> lines = new();
                try
                {

                    // Open file in read-only mode, allowing shared read/write access
                    using (FileStream fs = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))

                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Error: File not found.");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error: Access denied. Check file permissions.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }

                int lineNumber = 0;
                string definitions = "";
                foreach (string ln in lines)
                {
                    IISLogRecord iisLogRecord = new();
                    try
                    {
                        lineNumber++;
                        if (ln.StartsWith("#Fields:"))
                        {
                            definitions = ln;
                        }

                        if (!ln.StartsWith('#'))
                        {


                            string[] splittedLine = ln.Split(" ");

                            // Set file location properties
                            iisLogRecord.LineNumber = lineNumber;
                            iisLogRecord.OriginFilePath = filePath;
                            iisLogRecord.SubFolder = TheAppConfiguration.IISSiteLogSubFolderName;
                            iisLogRecord.DomainName = TheAppConfiguration.IISSiteDomainName;
                            iisLogRecord.DomainID = TheAppConfiguration.IISSiteID;

                            // Parse the log line to set log record properties
                            iisLogRecord.ParseFromLogLine(splittedLine, definitions);

                            this.Add(iisLogRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing line {lineNumber} in file {filePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}
