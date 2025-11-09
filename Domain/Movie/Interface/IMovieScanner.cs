using System;
using System.Threading.Tasks;

namespace MediaManager.Domain.Movies.Interface
{
    public interface IMovieScanner
    {
        /// <summary>
        /// Scanne uniquement les fichiers NFO dans un dossier
        /// </summary>
        Task ScanDirectoryNfoOnly(string directoryPath);

        /// <summary>
        /// Scanne les films non identifiés dans un dossier
        /// </summary>
        Task ScanUnidentifiedMovies(string directoryPath);

        /// <summary>
        /// Evénement déclenché après qu'un scan est terminé
        /// </summary>
        event Action? ScanCompleted;

        /// <summary>
        /// Evénement déclenché pour chaque film scanné
        /// </summary>
        event Action<Movie>? MovieScanned;
    }
}