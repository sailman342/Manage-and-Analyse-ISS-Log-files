
using IISLogsManager.ILMAppConfigNameSpace;

namespace IISLogsManager.DataBase
{
    public class IISLogRecords : List<IISLogRecord>
    {
        public IISLogRecords()
        {
            this.Clear();
        }

        public IISLogRecords(ILMAppContext TheILMAppContext)
        {
            this.Clear();
            string filePath = TheILMAppContext.IISSiteSelectedLogFile.FilePath;
            if (File.Exists(filePath))
            {
                // Store each line in array of strings
                List<string> lines = new();
                try
                {

                    // Open file in read-only mode, allowing shared read/write access
                    using (FileStream fs = new(
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
                catch (FileNotFoundException ex)
                {
                    throw new Exception ($"Error: File not found : {filePath} {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new Exception ($"Error: Access denied. Check file permissions : {filePath} {ex.Message}");
                }
                catch (IOException ex)
                {
                    throw new Exception ($"I/O Error: {filePath} {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error: {filePath} {ex.Message}");
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
                            iisLogRecord.SubFolder = TheILMAppContext.IISSiteLogSubFolderName;
                            iisLogRecord.DomainName = TheILMAppContext.IISSiteDomainName;
                            iisLogRecord.DomainID = TheILMAppContext.IISSiteID;

                            // Parse the log line to set log record properties
                            iisLogRecord.ParseFromLogLine(splittedLine, definitions);

                            this.Add(iisLogRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception ($"Error parsing line {lineNumber} in file {filePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}
