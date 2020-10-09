using System;
using ConVar;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Tides", "Regr3tti", "0.0.3")]
    [Description("adds natural tides to rust")]
    public class Tides : RustPlugin
    {
        float tideheight;
        float period;
        float offset;
        float _oceanlevel;
        private bool Changed;
        float refresh;

        public bool Initialized { get; private set; }

        object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                Changed = true;
            }
            object value;
            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
                Changed = true;
            }
            return value;
        }
        void LoadVariables()
        {
            tideheight = Convert.ToInt32(GetConfig("Settings", "Tide Hight. How many meters (1m = 3ft) high do you want High Tide? 2 is natural and default, 5 is high, higher than 10 may have unintended consequences", 2));
            period = Convert.ToInt32(GetConfig("Settings", "Interval. How many in-game hours between high and low tide? 6 is natural and mimics real life, giving a high tide every 30 minutes", 6));
            offset = Convert.ToInt32(GetConfig("Settings", "Offset. Don't change this unless you know what you're doing, 2 makes low tide start at midnight and -2 makes high tide start at midnight", 2));
            refresh = Convert.ToInt32(GetConfig("Settings", "Refresh per second. How often do you want the ocean level to refresh? This number represents every X seconds, so 0.1 happens 10 times per second", 0.1));

            if (!Changed) return;
            SaveConfig();
            Changed = false;
        }
        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            LoadVariables();
        }

        void Loaded()
        {
            LoadVariables();
            Initialized = false;
        }

        void OnServerInitialized()
        {
            timer.Repeat(refresh, 0, () => //This timer starts the loop below. Basically every 0.1 seconds the code below runs, an infinite number of times (or until it breaks!). It runs this often because otherwise the tide would look like it's jumping up and down, this keeps it smooth and makes the changes to ocean level virtually unnoticeable when staring at the ocean. 
            {
                _oceanlevel = (tideheight / 2) * (float)Math.Sin((Math.PI / period) * Env.time - 1f * (Math.PI / offset)) + (tideheight / 2); //By default Ocean Level = 1*sin((pi/6)x-(pi/2))+1, Simply this makes High Tide at 6:00 and 18:00, with low tide at 0:00 and 12:00. Ocean Levels stay between 0 and 2.
                ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), $"env.oceanlevel {_oceanlevel}"); //Executes the command, and blocks output to the console (or you'd get a flood of commands every 0.1 seconds)
                //Puts($"{_oceanlevel}"); //Debugging purposes only. DO NOT enable unless you want a ridiculous amount of console spam.  
            });
        }
    }
}