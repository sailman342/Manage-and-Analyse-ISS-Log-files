using IISLogsManager.Database;
using IISLogsManager.AppConfig;
using static IISLogsManager.AppConfig.AppConfiguration;

namespace IISLogsManager.DataBase
{
    public class IISLogFiles : List<IISLogFile>
    {
        public IISLogFiles()
        {
            this.Clear();
        }

        public IISLogFiles(string logsFolderPath)
        {
            this.Clear();
            if (Directory.Exists(logsFolderPath))
            {
                List<string> txtFiles = [.. Directory.EnumerateFiles(logsFolderPath, "*.log")];
                txtFiles.Sort();
                foreach (string filePath in txtFiles)
                {
                    List<string> lines = new();

                    // Create a new IISLogFile object and set its properties based on the file information
                    IISLogFile iisLogFile = new();

                    iisLogFile.DomainName = TheAppConfiguration.IISSiteDomainName;
                    iisLogFile.DomainID = TheAppConfiguration.IISSiteID;
                    iisLogFile.SubFolder = TheAppConfiguration.IISSiteLogSubFolderName;
                    iisLogFile.FilePath = filePath;
                    iisLogFile.FileName = Path.GetFileName(filePath);

                    try
                    {
                        //Access violation can occur when workking with the files of the test site where this app is located (used by other process)!
                        // should be available same if used by another process !
                        iisLogFile.FileInfo = new FileInfo(filePath);

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

                    // Read the log file and parse its content to set the StartDate, StartTime, EndDate, EndTime, and NumLogLines properties

                    // TODO Verify if reading first and last line would suffice instead of reading all lines and parsing them all to get the first and last date and time of the log file ! (if log files are sorted by name with date and time, it should be ok !)

                    // Store each line in array of strings

                    int lineNumber = 0;
                    string[] defParts = [];

                    foreach (string ln in lines)
                    {
                        try
                        {

                            if (ln.StartsWith("#Fields:"))
                            {
                                defParts = ln.Replace("#Fields: ", "").Split(" ");
                            }

                            if (!ln.StartsWith('#'))
                            {
                                lineNumber++;
                                // Parse the log line to set log record properties
                                (DateOnly Date, TimeOnly Time) = ParseFromLogLine(ln.Split(' '), defParts);
                                // IISLogFile properties date and time are set to min and max values !
                                if (iisLogFile.StartDate > Date || (iisLogFile.StartDate == Date && iisLogFile.StartTime > Time))
                                {
                                    iisLogFile.StartDate = Date;
                                    iisLogFile.StartTime = Time;
                                }
                                if (iisLogFile.EndDate < Date || (iisLogFile.EndDate == Date && iisLogFile.EndTime < Time))
                                {
                                    iisLogFile.EndDate = Date;
                                    iisLogFile.EndTime = Time;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing line {ln} in file {filePath}: {ex.Message}");
                        }
                        iisLogFile.NumLogLines = lineNumber;
                    }
                    this.Add(iisLogFile);
                }
            }
        }


        public IISLogFiles GetMarkedFiles()
        {
            IISLogFiles markedFiles = new();
            foreach (IISLogFile logFile in this)
            {
                if (logFile.IsMarked)
                {
                    markedFiles.Add(logFile);
                }
            }
            return markedFiles;
        }
        private static (DateOnly Date, TimeOnly Time) ParseFromLogLine(string[] splittedLine, string[] defParts)
        {
            DateOnly Date = DateOnly.FromDateTime(DateTime.MinValue);
            TimeOnly Time = TimeOnly.FromDateTime(DateTime.MinValue);

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
                }
            }
            return (Date, Time);
        }
    }
}
