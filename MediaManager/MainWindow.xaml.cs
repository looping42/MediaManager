using Domain.Movie.Dto;
using MediaManager.Divers;
using MediaManager.Domain;
using MediaManager.Domain.Interface;
using MediaManager.Domain.Movies.Interface;
using MediaManager.Logger;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml.Serialization;

namespace MediaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IMovieRepository _movieRepo;
        private readonly ISettingsRepository _settingsRepo;
        private readonly IMovieScanner _movieScanner;
        private readonly IUiLogger _logger;

        public ICommand LoadNfoCommand { get; set; }
        public ICommand ScanMoviesCommand { get; set; }

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

        public MainWindow(IMovieRepository movieRepo, ISettingsRepository settingsRepo, IMovieScanner movieScanner, IUiLogger logger)
        {
            InitializeComponent();

            _movieRepo = movieRepo;
            _settingsRepo = settingsRepo;
            _movieScanner = movieScanner;

            _logger = new TextBoxLogger(ScanLogTextBox);

            Loaded += async (s, e) => await InitializeAsync();

            //Loaded += async (s, e) =>
            //{
            //    this.Width = 1400;
            //    this.Height = 900;
            //    Movies = new ObservableCollection<Movie>(await _movieRepo.GetAllMoviesAsync());
            //};

            //LoadNfoCommand = new RelayCommand(param => SelectedMovieNfo = await LoadSelectedMovieNfo());

            //ScanMoviesCommand = new RelayCommand(ScanMovies);

            //DataContext = this;

            //// Initialisation de la commande du bouton
            //if (Movies.Count > 0)
            //    SelectedMovie = Movies[0];
        }

        private async Task InitializeAsync()
        {
            this.Width = 1400;
            this.Height = 900;

            // Charge les films depuis la DB
            var movies = await _movieRepo.GetAllMoviesAsync();
            Movies = new ObservableCollection<Movie>(movies);

            // Initialisation des commandes async
            LoadNfoCommand = new RelayCommand(async _ => SelectedMovieNfo = await LoadSelectedMovieNfo());

            //var test = await ScanMovies();
            //ScanMoviesCommand = new RelayCommand(_ => await ScanMovies());

            ScanMoviesCommand = new RelayCommand(async param => await ScanMovies(param));

            DataContext = this;

            if (Movies.Count > 0)
                SelectedMovie = Movies[0];
        }

        private async Task ScanMovies(object parameter)
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
                foreach (var m in await _movieRepo.GetAllMoviesAsync())
                    Movies.Add(m);
            }
            finally
            {
                ScanProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async Task<MovieNfo> LoadSelectedMovieNfo()
        {
            if (SelectedMovie == null) return null;

            var movieFromDb = await _movieRepo.GetMovieByIdAsync(SelectedMovie.Id);
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

        private async Task SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            MoviesList.ItemsSource = string.IsNullOrEmpty(searchText)
                ? await _movieRepo.GetAllMoviesAsync()
                : await _movieRepo.GetAllMoviesAsync(searchText);

            if (Movies.Count > 0)
                SelectedMovie = Movies[0];
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = App.ServiceProvider.GetRequiredService<SettingsWindow>();
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

        //private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //}
    }
}