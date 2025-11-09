using MediaManager.Domain.Constants;
using MediaManager.Domain.Interface;
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
        private readonly ISettingsRepository _settingsRepo;

        // Constructeur DI
        public SettingsWindow(ISettingsRepository settingsRepo)
        {
            InitializeComponent();

            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));

            ApiKeyTextBox.Text = _settingsRepo.Get(ConstantSettings.ApiKeyKey) ?? "";

            var paths = _settingsRepo.GetPaths(ConstantSettings.ScanPathsKey);
            PathsTextBox.Text = string.Join(Environment.NewLine, paths);
        }

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Sauvegarde clé API
            _settingsRepo.Save(ConstantSettings.ApiKeyKey, ApiKeyTextBox.Text.Trim());

            // Sauvegarde chemins
            var paths = PathsTextBox.Text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            _settingsRepo.SavePaths(ConstantSettings.ScanPathsKey, paths);

            var ignoredPaths = IgnoredPathsTextBox.Text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();
            _settingsRepo.SavePaths(ConstantSettings.IgnoredPath, ignoredPaths);

            MessageBox.Show("Enregistrement ✅", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}