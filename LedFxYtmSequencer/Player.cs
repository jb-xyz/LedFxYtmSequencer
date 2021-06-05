using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedFxYtmSequencer
{
    static class Player
    {
        private static Song _currentSong = null;
        public static Song currentSong { get { return _currentSong; } set { _currentSong = value; } }

        private static string currentScene ="";

        public static Dictionary<string, Song> songlist { get; set; }

        private static String FindScene(int time)
        {
            string newScene = "";
            for (int i = 0; i < _currentSong.scenes.Count; i++)
            {
                if (_currentSong.scenes[i].startTime <= time)
                {
                    newScene = _currentSong.scenes[i].sceneId;
                }
            }

            return newScene;
        }

        public static void Update(SongUpdate update)
        {
            Console.SetCursorPosition(0, 0);
            int pad = Console.WindowWidth;
            Console.WriteLine(update.name.PadRight(pad, ' '));
            Console.WriteLine(("Time: " + update.time).PadRight(pad, ' '));
            Console.WriteLine("-------------------------".PadRight(pad, ' '));
            Console.WriteLine("Active Scene".PadRight(pad, ' '));
            Console.WriteLine(currentScene.PadRight(pad, ' '));
            
            if (_currentSong != null && _currentSong.name.Equals(update.name))
            {
                string newScene = FindScene(update.time);
                if (!newScene.Equals(currentScene))
                {
                    currentScene = newScene;
                    LedFxApi.setScene(currentScene);
                }
            }
            else
            {
                Song nextSong = null;
                bool found = songlist.TryGetValue(update.name, out nextSong);

                if (found)
                {
                    _currentSong = nextSong;
                    currentScene = _currentSong.scenes[0].sceneId;

                    LedFxApi.setScene(currentScene);
                }
            }
            
        }
    }
}
