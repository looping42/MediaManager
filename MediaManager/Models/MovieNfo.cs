using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("movie", Namespace = "")]
    public class MovieNfo
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("originaltitle")]
        public string OriginalTitle { get; set; }

        [XmlElement("sorttitle")]
        public string SortTitle { get; set; }

        [XmlElement("epbookmark")]
        public string EpBookmark { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlArray("ratings")]
        [XmlArrayItem("rating")]
        public List<Rating> Ratings { get; set; } = new();

        [XmlElement("userrating")]
        public double UserRating { get; set; }

        [XmlElement("top250")]
        public int Top250 { get; set; }

        [XmlElement("set")]
        public MovieSet Set { get; set; }

        [XmlElement("plot")]
        public string Plot { get; set; }

        [XmlElement("outline")]
        public string Outline { get; set; }

        [XmlElement("tagline")]
        public string Tagline { get; set; }

        [XmlElement("runtime")]
        public int Runtime { get; set; }

        [XmlElement("thumb")]
        public List<Thumb> Thumbs { get; set; } = new();

        [XmlElement("fanart")]
        public Fanart Fanart { get; set; }

        [XmlElement("mpaa")]
        public string Mpaa { get; set; }

        [XmlElement("certification")]
        public string Certification { get; set; }

        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("tmdbid")]
        public string TmdbId { get; set; }

        [XmlElement("uniqueid")]
        public List<UniqueId> UniqueIds { get; set; } = new();

        [XmlElement("country")]
        public List<string> Countries { get; set; } = new();

        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("code")]
        public string Code { get; set; }

        [XmlElement("premiered")]
        public string Premiered { get; set; }

        [XmlElement("watched")]
        public bool Watched { get; set; }

        [XmlElement("playcount")]
        public int PlayCount { get; set; }

        [XmlElement("genre")]
        public List<string> Genres { get; set; } = new();

        [XmlElement("studio")]
        public List<string> Studios { get; set; } = new();

        [XmlElement("credits")]
        public List<Credit> Credits { get; set; } = new();

        [XmlElement("director")]
        public List<Director> Directors { get; set; } = new();

        [XmlElement("tag")]
        public List<string> Tags { get; set; } = new();

        [XmlElement("actor")]
        public List<Actor> Actors { get; set; } = new();
    }

    public class Rating
    {
        [XmlAttribute("default")]
        public bool Default { get; set; }

        [XmlAttribute("max")]
        public int Max { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public double Value { get; set; }

        [XmlElement("votes")]
        public int Votes { get; set; }
    }

    public class MovieSet
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("overview")]
        public string Overview { get; set; }
    }

    public class Thumb
    {
        [XmlAttribute("aspect")]
        public string Aspect { get; set; }

        [XmlText]
        public string Url { get; set; }
    }

    public class Fanart
    {
        [XmlElement("thumb")]
        public List<string> Thumbs { get; set; } = new();
    }

    public class UniqueId
    {
        [XmlAttribute("default")]
        public bool Default { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class Credit
    {
        [XmlAttribute("tmdbid")]
        public string TmdbId { get; set; }

        [XmlText]
        public string Name { get; set; }
    }

    public class Director
    {
        [XmlAttribute("tmdbid")]
        public string TmdbId { get; set; }

        [XmlText]
        public string Name { get; set; }
    }

    public class Actor
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("role")]
        public string Role { get; set; }

        [XmlElement("thumb")]
        public string Thumb { get; set; }

        [XmlElement("profile")]
        public string Profile { get; set; }

        [XmlElement("tmdbid")]
        public string TmdbId { get; set; }
    }
}