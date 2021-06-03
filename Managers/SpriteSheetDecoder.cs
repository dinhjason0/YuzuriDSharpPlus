using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Yuzuri.Managers
{
    public class SpriteSheetDecoder
    {
        //Finds the sprite's coordinates in the .json
        public int[] JsonDecoder(string target)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {
                JObject o2 = (JObject)JToken.ReadFrom(fileContent);
                string stringedJObject = JsonConvert.ToString(o2);
                //dynamic results = JsonConvert.DeserializeObject<dynamic>(stringedJObject);
                JArray jsonObject = JArray.Parse(stringedJObject);

                int[] sendingCoordinates = jsonObject[0][target].Values<int>().ToArray();
                return sendingCoordinates;
            }
        }
    }
}
