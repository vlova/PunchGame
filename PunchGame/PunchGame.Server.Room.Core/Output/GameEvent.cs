using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class GameEvent
    {
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Converters =
                {
                    new StringEnumConverter()
                }
            });
        }
    }
}
