using MediaManager.Business;
using MediaManager.Data;
using MediaManager.Models;
using MediaManager.NewFolder;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly DatabaseService _db;
        private readonly MovieScanner _scanner;
        public ICommand LoadNfoCommand { get; }
        public ICommand ScanMoviesCommand { get; }

        public ObservableCollection<Movie> Movies { get; set; } = new();
        private MovieNfo _selectedMovieNfo;

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

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                this.Width = 1400;
                this.Height = 900;
            };
            _db = new DatabaseService();
            _scanner = new MovieScanner(_db);
            Movies = new ObservableCollection<Movie>(_db.GetAllMovies());
            LoadNfoCommand = new RelayCommand(param => SelectedMovieNfo = LoadSelectedMovieNfo());
            DataContext = this;

            // Initialisation de la commande du bouton
            ScanMoviesCommand = new RelayCommand(ScanMovies);
        }

        private async void ScanMovies(object parameter)
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                //MessageBox.Show("🚀 Scan des films en cours...", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);

                var folder = "\\\\192.168.1.12\\Share\\424084BC4084B865"; // plus tard, tu pourras ouvrir un dialogue
                await Task.Run(() => _scanner.ScanDirectory(folder));
                Movies.Clear();
                foreach (var m in _db.GetAllMovies())
                    Movies.Add(m);
            }
            finally
            {
                // Cache l'overlay même en cas d'erreur
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // --- Binding support (INotifyPropertyChanged) ---
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private MovieNfo LoadSelectedMovieNfo()
        {
            if (SelectedMovie == null) return null;

            var movieFromDb = _db.GetMovieById(SelectedMovie.Id);
            if (movieFromDb == null) return null;

            var serializer = new XmlSerializer(typeof(MovieNfo));
            using var reader = new StreamReader(movieFromDb.NfoPath);
            MovieNfo movie = (MovieNfo)serializer.Deserialize(reader);

            return movie;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            MoviesList.ItemsSource = string.IsNullOrEmpty(searchText)
                ? _db.GetAllMovies()
                : _db.GetAllMovies(searchText);
        }
    }
}