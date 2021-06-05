using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LedFxYtmSequencer
{
    internal static class RestListener
    {
        private const int Port = 9999;

        /// <summary>
        /// This is the heart of the web server
        /// </summary>
        private static readonly HttpListener Listener = new HttpListener { Prefixes = { $"http://localhost:{Port}/" } };

        /// <summary>
        /// A flag to specify when we need to stop
        /// </summary>
        private static volatile bool _keepGoing = true;

        /// <summary>
        /// Keep the task in a static variable to keep it alive
        /// </summary>
        private static Task _mainLoop;

        /// <summary>
        /// Call this to start the web server
        /// </summary>
        public static void StartWebServer()
        {
            if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
            _mainLoop = MainLoop();
        }

        /// <summary>
        /// Call this to stop the web server. It will not kill any requests currently being processed.
        /// </summary>
        public static void StopWebServer()
        {
            _keepGoing = false;
            lock (Listener)
            {
                //Use a lock so we don't kill a request that's currently being processed
                Listener.Stop();
            }
            try
            {
                _mainLoop.Wait();
            }
            catch { /* je ne care pas */ }
        }

        /// <summary>
        /// The main loop to handle requests into the HttpListener
        /// </summary>
        /// <returns></returns>
        private static async Task MainLoop()
        {
            Listener.Start();
            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await Listener.GetContextAsync();
                    lock (Listener)
                    {
                        if (_keepGoing) ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpListenerException) return; //this gets thrown when the listener is stopped
                    //TODO: Log the exception
                }
            }
        }

        /// <summary>
        /// Handle an incoming request
        /// </summary>
        /// <param name="context">The context of the incoming request</param>
        private static void ProcessRequest(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                try
                {
                    var handled = false;
                    switch (context.Request.Url.AbsolutePath)
                    {
                        //This is where we do different things depending on the URL
                        //TODO: Add cases for each URL we want to respond to
                        case "/update":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    //Get the current settings
                                    response.ContentType = "application/json";

                                    //This is what we want to send back
                                    var responseBody = JsonConvert.SerializeObject(Player.currentSong);

                                    //Write it to the response stream
                                    var buffer = Encoding.UTF8.GetBytes(responseBody);
                                    response.Headers.Add("Access-Control-Allow-Origin", "https://music.youtube.com");
                                    response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
                                    response.ContentLength64 = buffer.Length;
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                    handled = true;
                                    break;

                                case "OPTIONS":
                                    //Get the current settings
                                    response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                                    response.Headers.Add("Access-Control-Allow-Origin", "https://music.youtube.com");
                                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                                    handled = true;
                                    break;


                                case "POST":
                                    //Update the settings
                                    using (var body = context.Request.InputStream)
                                    using (var reader = new StreamReader(body, context.Request.ContentEncoding))
                                    {
                                        //Get the data that was sent to us
                                        var json = reader.ReadToEnd();

                                        //Use it to update our settings
                                        Player.Update(JsonConvert.DeserializeObject<SongUpdate>(json));

                                        //Return 204 No Content to say we did it successfully
                                        response.Headers.Add("Access-Control-Allow-Origin", "https://music.youtube.com");
                                        response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
                                        response.StatusCode = 204;
                                        handled = true;
                                    }
                                    break;
                            }
                            break;
                    }
                    if (!handled)
                    {
                        response.StatusCode = 404;
                    }
                }
                catch (Exception e)
                {
                    //Return the exception details the client - you may or may not want to do this
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    //TODO: Log the exception
                }
            }
        }
    }
}
