using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TelegramBot
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public string ReleaseDate { get; set; }
        public string SteamUrl { get; set; }
    }
}

