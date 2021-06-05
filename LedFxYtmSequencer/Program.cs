using System;
using System.Collections.Generic;
using System.IO;

namespace LedFxYtmSequencer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading available Songs...");
            Player.songlist = new Dictionary<string, Song>();
            loadSongs();

            RestListener.StartWebServer();
            Console.Read();
        }

        static void loadSongs()
        {
            string[] files = Directory.GetFiles("songs", "*.sng");
            Console.WriteLine($"found {files.Length} songs...");


            foreach(string file in files)
            {
                addSong(new List<string>(File.ReadAllLines(file)));
            }
        }

        static void addSong(List<string> songFile)
        {
            string songName = songFile[0];
            songFile.RemoveAt(0);

            Song newSong = new Song()
            {
                name = songName
            };

            List<SceneTime> sceneTimes = new List<SceneTime>();
            foreach(string line in songFile)
            {
                string[] data = line.Split(':');
                sceneTimes.Add(new SceneTime()
                {
                    startTime = int.Parse(data[0]),
                    sceneId = data[1]
                });
            }
            newSong.scenes = sceneTimes;
            Player.songlist.Add(songName, newSong);
        }
    }
}
