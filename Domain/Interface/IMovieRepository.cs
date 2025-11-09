using MediaManager.Domain;

namespace MediaManager.Domain.Interface
{
    public interface IMovieRepository
    {
        Task<bool> MovieExistsByNfoPathAsync(string nfoPath);

        Task InsertMoviesAsync(IEnumerable<Movie> movies);

        Task UpdateMovieAsync(Movie movie);

        Task<Movie?> GetMovieByIdAsync(int id);

        Task<IEnumerable<Movie>> GetAllMoviesAsync(string? search = null);
    }
}