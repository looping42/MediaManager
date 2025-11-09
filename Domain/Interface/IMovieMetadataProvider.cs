using Domain.Movie.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Domain.Interface
{
    public interface IMovieMetadataProvider
    {
        Task<MovieMetadata> FetchMetadataAsync(string title, string year = null);
    }
}