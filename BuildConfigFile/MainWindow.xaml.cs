using IISLogsManager.ILMAppConfigNameSpace;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace BuildConfigFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILMAppConfiguration appConfig = new();

        public MainWindow()
        {
            InitializeComponent();
            ClearForm();
        }

        private void SaveToFile(object sender, RoutedEventArgs e)
        {
            appConfig = new();

            CopyFormToAppConfig();

            string jsonStr = JsonSerializer.Serialize(appConfig, new JsonSerializerOptions { WriteIndented = true });

            SaveFileDialog dialog = new()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*",
                FileName = "AppConfig.Txt"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, jsonStr);
            }
        }

        private void LoadFromFile(object sender, RoutedEventArgs e)
        {

            string jsonStr = "";
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*",
                FileName = "AppConfig.Txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                jsonStr = File.ReadAllText(openFileDialog.FileName);
            }

            if (jsonStr != null)
            {

                try
                {
                    appConfig = JsonSerializer.Deserialize<ILMAppConfiguration>(jsonStr) ?? new();

                    if (appConfig.IsLoaded)
                    {
                        CopyAppConfigToForm();
                    }
                }
                catch
                {
                    ClearForm();
                }
            }
            else
            {
                ClearForm();
            }
        }
        /*

         */
        private void CopyFormToAppConfig()
        {
            appConfig.IsLoaded = true;
            appConfig.AdminLogin = AdminLoginTextBox.Text;
            appConfig.AdminPassword = AdminPasswordTextBox.Text;
            appConfig.LogsRootDirectory = LogsRootDirectoryTextBox.Text;

            string [] LogsRootDirectoryTexts = Regex.Split(SitesListTextBox.Text, "\r\n|\r|\n");
            foreach (string line in LogsRootDirectoryTexts)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    string[] parts = trimmedLine.Split(';');
                    if (parts.Length >= 3)
                    {
                        IISSite site = new IISSite();
                        site.ID = parts[0].Trim();
                        site.LogSubFolder = parts[1].Trim();
                        site.DomainName = parts[2].Trim();
                        appConfig.IISSites.Sites.Add(site);
                    }
                }
            }
        }
        private void CopyAppConfigToForm()
        {
            AdminLoginTextBox.Text = appConfig.AdminLogin;
            AdminPasswordTextBox.Text = appConfig.AdminPassword;
            LogsRootDirectoryTextBox.Text = appConfig.LogsRootDirectory;
            foreach(IISSite site in appConfig.IISSites.Sites)
            {
                SitesListTextBox.Text += $"{site.ID} ; {site.LogSubFolder} ; {site.DomainName}\r\n";
            }
        }

        private void ClearForm()
        {
            AdminLoginTextBox.Text="";
            AdminPasswordTextBox.Text="";
            LogsRootDirectoryTextBox.Text= "C:\\inetpub\\logs\\LogFiles\\";
            SitesListTextBox.Text="";
        }
    }
}