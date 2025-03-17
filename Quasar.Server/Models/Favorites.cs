using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Quasar.Server.Models
{
    public class Favorites
    {
        private static readonly string FavoritesPath = Path.Combine(Application.StartupPath, "favorites.xml");
        private static List<string> _favoriteClients = new List<string>();

        public static void LoadFavorites()
        {
            try
            {
                if (File.Exists(FavoritesPath))
                {
                    using (var stream = File.OpenRead(FavoritesPath))
                    {
                        var serializer = new XmlSerializer(typeof(List<string>));
                        _favoriteClients = (List<string>)serializer.Deserialize(stream);
                    }
                }
            }
            catch
            {
                _favoriteClients = new List<string>();
            }
        }

        public static void SaveFavorites()
        {
            try
            {
                using (var stream = File.Create(FavoritesPath))
                {
                    var serializer = new XmlSerializer(typeof(List<string>));
                    serializer.Serialize(stream, _favoriteClients);
                }
            }
            catch
            {

            }
        }

        public static bool IsFavorite(string clientId)
        {
            return _favoriteClients.Contains(clientId);
        }

        public static void ToggleFavorite(string clientId)
        {
            if (_favoriteClients.Contains(clientId))
            {
                _favoriteClients.Remove(clientId);
            }
            else
            {
                _favoriteClients.Add(clientId);
            }
            SaveFavorites();
        }
    }
} 