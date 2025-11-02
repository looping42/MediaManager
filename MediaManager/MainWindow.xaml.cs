using MediaManager.Business;
using MediaManager.Data;
using MediaManager.Models;
using MediaManager.NewFolder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace MediaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Movie _selectedMovie;
        private readonly MovieRepository _movieRepo;
        private readonly SettingsRepository _settingsRepo;
        private readonly MovieScanner _movieScanner;
        public ICommand LoadNfoCommand { get; }
        public ICommand ScanMoviesCommand { get; }

        public ObservableCollection<Movie> Movies { get; set; } = new();
        private MovieNfo _selectedMovieNfo;

        public event PropertyChangedEventHandler PropertyChanged;

        // Callback pour log
        public Action<string> LogCallback { get; set; }

        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged();
                LoadNfoCommand.Execute(null);
            }
        }

        public MovieNfo SelectedMovieNfo
        {
            get => _selectedMovieNfo;
            set
            {
                _selectedMovieNfo = value;
                OnPropertyChanged();
            }
        }

        private void Log(string message)
        {
            LogCallback?.Invoke(message);
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                this.Width = 1400;
                this.Height = 900;
            };
            DatabaseInitializer.Initialize("movies.db");
            _movieRepo = new MovieRepository();
            _settingsRepo = new SettingsRepository();
            LogCallback = LogMessage;
            _movieScanner = new MovieScanner(_movieRepo, LogCallback);
            Movies = new ObservableCollection<Movie>(_movieRepo.GetAllMovies());

            LoadNfoCommand = new RelayCommand(param => SelectedMovieNfo = LoadSelectedMovieNfo());
            DataContext = this;

            // Initialisation de la commande du bouton
            ScanMoviesCommand = new RelayCommand(ScanMovies);
        }

        private async void ScanMovies(object parameter)
        {
            try
            {
                Log($"Start Scan");

                ScanProgressBar.Visibility = Visibility.Visible;
                var paths = _settingsRepo.GetPaths("ScanPaths");
                if (paths.Count == 0)
                {
                    MessageBox.Show("Aucun chemin de scan configuré.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Lancer le scan en parallèle
                await Task.Run(() =>
                {
                    Parallel.ForEach(paths, folder =>
                    {
                        try
                        {
                            _movieScanner.ScanDirectoryNfoOnly(folder);
                        }
                        catch (Exception ex)
                        {
                            Log(ex.ToString());
                        }
                    });
                });
                Log($"End Scan");

                Movies.Clear();
                foreach (var m in _movieRepo.GetAllMovies())
                    Movies.Add(m);
            }
            finally
            {
                ScanProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private MovieNfo LoadSelectedMovieNfo()
        {
            if (SelectedMovie == null) return null;

            var movieFromDb = _movieRepo.GetMovieById(SelectedMovie.Id);
            if (movieFromDb == null) return null;

            var serializer = new XmlSerializer(typeof(MovieNfo));
            try
            {
                using var reader = new StreamReader(movieFromDb.NfoUrl);
                MovieNfo movie = (MovieNfo)serializer.Deserialize(reader);
                return movie;
            }
            catch
            {
                // If deserialization fails, just return null and do not render
                return null;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            MoviesList.ItemsSource = string.IsNullOrEmpty(searchText)
                ? _movieRepo.GetAllMovies()
                : _movieRepo.GetAllMovies(searchText);
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void LogMessage(string message)
        {
            // On s'assure que le code s'exécute dans le thread UI
            Dispatcher.Invoke(() =>
            {
                ScanLogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                ScanLogTextBox.ScrollToEnd();
            });
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // Ouvre le lien dans le navigateur par défaut
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible d'ouvrir le lien : {ex.Message}");
            }

            e.Handled = true;
        }
    }
}