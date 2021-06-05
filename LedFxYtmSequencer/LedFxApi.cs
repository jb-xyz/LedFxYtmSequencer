using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LedFxYtmSequencer
{
    static class LedFxApi
    {
        static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:8888")
        };

        public static void setScene(string sceneId)
        {
            SceneUpdate newScene = new SceneUpdate()
            {
                id = sceneId
            };
            var body = JsonConvert.SerializeObject(newScene).ToLower();
            using (HttpContent httpContent = new StringContent(body))
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = client.PutAsync("api/scenes", httpContent).Result;
                Console.WriteLine("");
            }
        }
    }

    class SceneUpdate
    {
        public string action = "activate";
        public string id { get; set; }
    }
}
