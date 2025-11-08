using MediaManager.Business;
using MediaManager.Data;
using MediaManager.Divers;
using MediaManager.Logger;
using MediaManager.Models;
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
        private readonly MovieRepository _movieRepo;
        private readonly SettingsRepository _settingsRepo;
        private readonly MovieScanner _movieScanner;
        private readonly IUiLogger _logger;

        public ICommand LoadNfoCommand { get; }
        public ICommand ScanMoviesCommand { get; }

        private Movie _selectedMovie;
        private MovieNfo _selectedMovieNfo;

        public ObservableCollection<Movie> Movies { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

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

        public MainWindow(MovieRepository movieRepo, SettingsRepository settingsRepo, MovieScanner movieScanner, IUiLogger logger)
        {
            InitializeComponent();

            _movieRepo = movieRepo;
            _settingsRepo = settingsRepo;
            _movieScanner = movieScanner;

            _logger = new TextBoxLogger(ScanLogTextBox);

            Loaded += (s, e) =>
            {
                this.Width = 1400;
                this.Height = 900;
            };

            Movies = new ObservableCollection<Movie>(_movieRepo.GetAllMovies());

            LoadNfoCommand = new RelayCommand(param => SelectedMovieNfo = LoadSelectedMovieNfo());
            ScanMoviesCommand = new RelayCommand(ScanMovies);

            DataContext = this;

            // Initialisation de la commande du bouton
            if (Movies.Count > 0)
                SelectedMovie = Movies[0];
        }

        private async void ScanMovies(object parameter)
        {
            try
            {
                _logger.Log($"Start Scan");

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
                            _logger.Log(ex.ToString());
                        }
                    });
                });
                _logger.Log($"End Scan");

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

            if (Movies.Count > 0)
                SelectedMovie = Movies[0];
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