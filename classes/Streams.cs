using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GronkhTV_DL.classes
{
    public class Streams : INotifyPropertyChanged
    {
        public string? title { get; set; }
        public DateTime created_at { get; set; }
        public int episode { get; set; }
        public string? preview_url { get; set; }
        public int video_length { get; set; }
        public Qualities Qualities { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Qualities
    {
        public List<Quality> StreamQualities { get; set; } = [];

        public override string ToString()
        {
            return $"{string.Join(", ",StreamQualities)}";
        }
    }
    public class Quality
    {
        public string quality { get; set; } = "";
        public string url { get; set; } = "";

        public override string ToString()
        {
            return quality;
        }
    }
    public class AssociatedGame
    {
        public int id { get; set; }
        public string? title { get; set; }
        public TwitchDetails? twitch_details { get; set; }
    }
    public class Chapter
    {
        public int id { get; set; }
        public string? title { get; set; }
        public AssociatedGame? associated_game { get; set; }
        public List<Video>? videos { get; set; }
    }
    public class Game
    {
        public int id { get; set; }
        public string? title { get; set; }
        public TwitchDetails? twitch_details { get; set; }
        public List<Video>? videos { get; set; }
    }
    public class Results
    {
        public List<Video>? videos { get; set; }
        public List<Game>? games { get; set; }
        public List<Chapter>? chapters { get; set; }
        public List<object>? users { get; set; }
    }
    public class Root
    {
        public Results? results { get; set; }
    }
    public class Tag
    {
        public int id { get; set; }
        public string? title { get; set; }
    }
    public class TwitchDetails
    {
        public int id { get; set; }
        public string? title { get; set; }
        public string? thumbnail_url { get; set; }
    }
    public class Video
    {
        public int id { get; set; }
        public string? title { get; set; }
        public DateTime created_at { get; set; }
        public int episode { get; set; }
        public string? preview_url { get; set; }
        public int video_length { get; set; }
        public int views { get; set; }
        public List<Tag>? tags { get; set; }

        public override string ToString()
        {
            return $"[{episode}] {title} ({created_at:dd.MMM.yyyy})";
        }
    }
}
