using MediaManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MediaManager
{
    /// <summary>
    /// Logique d'interaction pour SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsRepository _settingsRepo;
        private const string ScanPathsKey = "ScanPaths";
        private const string ApiKeyKey = "ApiKey";

        public SettingsWindow()
        {
            InitializeComponent();

            _settingsRepo = new SettingsRepository();

            ApiKeyTextBox.Text = _settingsRepo.Get(ApiKeyKey) ?? "";

            var paths = _settingsRepo.GetPaths(ScanPathsKey);
            PathsTextBox.Text = string.Join(Environment.NewLine, paths);
        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Sauvegarde clé API
            _settingsRepo.Save(ApiKeyKey, ApiKeyTextBox.Text.Trim());

            // Sauvegarde chemins
            var paths = PathsTextBox.Text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            _settingsRepo.SavePaths(ScanPathsKey, paths);

            MessageBox.Show("Clé API et chemins enregistrés ✅", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}