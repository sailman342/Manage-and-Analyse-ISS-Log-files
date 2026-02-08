using IISLogsManager.DataBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Text.Json;

namespace IISLogsManager.AppConfig
{
    /*
     * 
     * 
     * TO BE SERIALISABLE TO JSON EACH PROPERTY MUST HAVE GET AND SET METHODS
     * 
     * 
     */

    public class IISSite
    {
        public string ID { get; set; } = "";
        public string DomainName { get; set; } = "";
        public string LogSubFolder { get; set; } = "";
    }

    public class IISSites
    {
        public List<IISSite> Sites { get; set; } = [];
    }

    public class AppConfiguration
    {
        // edited in the config file app

        public string AdminLogin { get; set; } = "";
        public string AdminPassword { get; set; } = "";
        public string LogsRootDirectory { get; set; } = "C:\\inetpub\\logs\\LogFiles\\";
        public IISSites IISSites { get; set; } = new();

        // used for app controlling, not edited in the configuation app for the config file

        public bool IsLoaded { get; set; } = false;
        public bool IsLoggedIn { get; set; } = false;

        // set up by siteselecion page

        public bool IISSiteSelected { get; set; } = false;
        public string IISSiteID { get; set; } = "";
        public string IISSiteLogSubFolderName { get; set; } = "";
        public string IISSiteLogSubFolderPath  { get; set; } = ""; //TODO replace everywhere
        public string IISSiteDomainName { get; set; } = "";

        public IISLogFile IISSiteSelectedLogFile { get; set; } = new();
        public static async Task<string> CopyAppConfigFileToBaseDirectory(IBrowserFile browserFile)
        {
            using Stream fileStream = browserFile.OpenReadStream(maxAllowedSize: 100000000);
            if (fileStream.Length > 0)
            {
                using StreamReader reader = new(fileStream, encoding: System.Text.Encoding.UTF8);
                try
                {
                    string fileContent = await reader.ReadToEndAsync();
                    try
                    {

                        AppConfiguration TheAppConfiguration = JsonSerializer.Deserialize<AppConfiguration>(fileContent) ?? new();
                        if (TheAppConfiguration.IsLoaded == true)
                        {
                            string baseDirectory = GetBaseDirectory();
                            try
                            {
                                string localFileContent = EncryptConfig(TheAppConfiguration);
                                File.WriteAllText(Path.Combine(baseDirectory, "AppConfig.txt"), localFileContent);

                                fileStream.Dispose();

                            }
                            catch (Exception ex)
                            {
                                return "Cannot write : " + ex.Message;
                            }
                        }
                        else
                        {
                            return "Cannot decode the file !";
                        }
                    }
                    catch
                    {
                        return "Cannot deserialize the text to AppConfiguration type";
                    }
                }
                catch (Exception ecanootread)
                {
                    return "Cannot read the input file : " + ecanootread.Message;
                }

                fileStream.Dispose();
                return "Succes";
            }
            return "File stream is empty !";
        }

        public AppConfiguration()
        {
            AdminLogin = "";
            AdminPassword = "";
            LogsRootDirectory = "C:\\inetpub\\logs\\LogFiles\\";
            IISSites = new();

            IsLoaded = false;
            IsLoggedIn = false;

            IISSiteLogSubFolderName = "";
            IISSiteDomainName = "";
            IISSiteID = "";
        }

        // IsLoaded == false
        public static AppConfiguration TheAppConfiguration  = new(); // { get; set; } make it available for debug

        public static AppConfiguration GetAppConfig()
        {
            // note appConfig.IsLoaded == false;
            // and iiissiteselected == false

            AppConfiguration appConfig;

            // THIS WILL OVERRIDE THE CURRENT CONFIG !
            // should be done once at /home before loggin and no more afterwards !
            try
            {
                string baseDirectory = GetBaseDirectory();
                string fileContent = File.ReadAllText(Path.Combine(baseDirectory, "AppConfig.txt"));

                appConfig = DecryptConfig(fileContent);

                if (appConfig.IsLoaded)
                {
                    // App config is loaded successfully
                }
                else
                {
                    appConfig = new(); // IsLoaded = false !
                }
            }
            catch
            {
                appConfig = new(); // IsLoaded = false !
            }
            return appConfig;
        }

        public static AppConfiguration DecryptConfig(string data)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // get App Dll directory
            return DecryptConfig(data, baseDirectory);
        }

        public static AppConfiguration DecryptConfig(string data, string key)
        {
            try
            {
                string inter = DecryptStr(data, key);
                return DecodeFromBase64(inter);
            }
            catch
            {
                return new();
            }
        }

        public static string DecryptStr(string data)
        {
            string key = "DriveABmwZ4ToBeHappyIfYouhave hairs";
            return DecryptStr(data, key);
        }

        public static string DecryptStr(string data, string key)
        {

            Encoding unicode = Encoding.Unicode;
            try
            {
                return unicode.GetString(Encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
            }
            catch
            {
                return "";
            }
        }
        public static AppConfiguration DecodeFromBase64(string str)
        {
            AppConfiguration? appConfig;
            try
            {
                byte[] decoded = Convert.FromBase64String(str);
                string data = Encoding.ASCII.GetString(decoded);

                if (data != null && data != "")
                {
                    appConfig = JsonSerializer.Deserialize<AppConfiguration>(data);
                    if (appConfig != null)
                    {
                        appConfig.IsLoaded = true;
                        return appConfig;
                    }
                }
                appConfig = new();
                return appConfig;
            }
            catch
            {
                appConfig = new();
                return appConfig;
            }

        }

        // use App Domain Base Directory as key for decryption
        public static string GetBaseDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // get App Dll directory
            return baseDirectory;
        }

        public static string EncryptConfig(AppConfiguration appConfig)
        {
            // Use App Domain Base Directory as key for encryption
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // get App Dll directory
            return EncryptConfig(appConfig, baseDirectory);
        }

        public static string EncryptConfig(AppConfiguration appConfig, string key)
        {

            Encoding unicode = Encoding.Unicode;
            try
            {
                return Convert.ToBase64String(Encrypt(unicode.GetBytes(key), unicode.GetBytes(EncodeToBase64(appConfig))));
            }
            catch
            {
                return "";
            }
        }

        public static string EncodeToBase64(AppConfiguration appConfig)
        {
            string jsonStr = JsonSerializer.Serialize(appConfig);
            return EncodeToBase64Str(jsonStr);
        }

        public static string EncodeToBase64Str(string toEncode)
        {
            byte[] data = Encoding.ASCII.GetBytes(toEncode);
            string base64Data = Convert.ToBase64String(data);
            return base64Data;
        }

        public static string DecodeFromBase64Str(string str)
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(str);
                return Encoding.ASCII.GetString(decoded);
            }
            catch
            {
                return "";
            }

        }
        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            return [.. EncryptOutput(key, data)];
        }

        private static IEnumerable<byte>
        EncryptOutput(byte[] key, IEnumerable<byte>
            data)
        {
            byte[] s = EncryptInitalize(key);

            int i = 0;
            int j = 0;

            return data.Select((b) =>
            {
                i = i + 1 & 255;
                j = j + s[i] & 255;

                Swap(s, i, j);

                return (byte)(b ^ s[s[i] + s[j] & 255]);
            });
        }

        private static byte[] EncryptInitalize(byte[] key)
        {
            byte[] s = [.. Enumerable.Range(0, 256).Select(i => (byte)i)];

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = j + key[i % key.Length] + s[i] & 255;

                Swap(s, i, j);
            }

            return s;
        }

        private static void Swap(byte[] s, int i, int j)
        {
            (s[j], s[i]) = (s[i], s[j]);
        }
    }
}
