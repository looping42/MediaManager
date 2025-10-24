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
            DataContext = this;

            // Initialisation de la commande du bouton
            ScanMoviesCommand = new RelayCommand(ScanMovies);

            // Exemples de données (tu remplaceras ça par ton scan réel)
            Movies.Add(new Movie
            {
                Title = "Inception",
                Year = "2010",
                Overview = "Un voleur professionnel s'infiltre dans les rêves de ses cibles pour voler leurs secrets.",
                PosterPath = "https://image.tmdb.org/t/p/w500/qmDpIHrmpJINaRKAfWQfftjCdyi.jpg"
            });

            Movies.Add(new Movie
            {
                Title = "Interstellar",
                Year = "2014",
                Overview = "Un groupe d'explorateurs traverse un trou de ver pour sauver l'humanité.",
                PosterPath = "https://image.tmdb.org/t/p/w500/rAiYTfKGqDCRIIqo664sY9XZIvQ.jpg"
            });

            SelectedMovie = Movies[0];
        }

        private void ScanMovies(object parameter)
        {
            MessageBox.Show("🚀 Scan des films en cours...", "Scan", MessageBoxButton.OK, MessageBoxImage.Information);
            // Ici tu mettras ton code de scrapping ou de scan de répertoires
        }

        // --- Binding support (INotifyPropertyChanged) ---
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // --- Classe de base pour les films ---
    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
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