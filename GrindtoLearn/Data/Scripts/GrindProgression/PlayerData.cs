using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using VRage.Game.Entity;
using VRage.Utils;
using Phoera.GringProgression;

namespace Phoera.GringProgression
{
    public class PlayerData
    {
        public HashSet<SerializableDefinitionId> LearnedBocks = new HashSet<SerializableDefinitionId>();
        public int Luck { get; set; } = 0;

        public void SavePlayers()
        {
            MyLog.Default.WriteLine("Saving Players...");
            MyLog.Default.Flush();
            foreach (var player in Core.playersData)
            {
                try
                {
                    using (var sw =
                      MyAPIGateway.Utilities.WriteFileInWorldStorage(string.Format(Settings.playerFile, player.Key), typeof(Core)))
                        sw.Write(MyAPIGateway.Utilities.SerializeToXML(player.Value.Select(s => (SerializableDefinitionId)s)
                          .ToList()));

                    MyLog.Default.WriteLine($"Player {player.Key} Saved!");
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"ERROR SaveData: {e.Message}");
                }
            }
            MyLog.Default.Flush();
        }
    }
}
