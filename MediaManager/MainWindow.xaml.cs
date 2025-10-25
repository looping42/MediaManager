using MediaManager.Business;
using MediaManager.Data;
using MediaManager.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<Movie> Movies { get; set; } = new();

        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged();
            }
        }

        public ICommand ScanMoviesCommand { get; }

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
            DataContext = this;

            // Initialisation de la commande du bouton
            ScanMoviesCommand = new RelayCommand(ScanMovies);
        }

        private void ScanMovies(object parameter)
        {
            MessageBox.Show("🚀 Scan des films en cours...", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- Binding support (INotifyPropertyChanged) ---
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = "\\\\192.168.1.12\\Share\\424084BC4084B865"; // plus tard, tu pourras ouvrir un dialogue
            _scanner.ScanDirectory(folder);
            Movies.Clear();
            foreach (var m in _db.GetAllMovies())
                Movies.Add(m);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            MoviesList.ItemsSource = string.IsNullOrEmpty(searchText)
                ? _db.GetAllMovies()
                : _db.GetAllMovies(searchText);
        }
    }

    // --- Commande générique pour les boutons / actions ---
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}