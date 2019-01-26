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
    public class PlayerFile
    {
        public HashSet<SerializableDefinitionId> LearnedBocks = new HashSet<SerializableDefinitionId>();
        public int Luck { get; set; } = 0;
    }

    public class PlayerData
    {
        public HashSet<MyDefinitionId> LearnedBocks = new HashSet<MyDefinitionId>();
        public int Luck { get; set; } = 0;

        public PlayerData()
        {
            LearnedBocks = new HashSet<MyDefinitionId>();
            Luck = 0;
        }
        public PlayerData(HashSet<MyDefinitionId> _LearnedBocks)
        {
            this.LearnedBocks = _LearnedBocks;
            this.Luck = 0;
        }
        public PlayerData(HashSet<MyDefinitionId> _LearnedBocks, int _Luck = 0)
        {
            this.LearnedBocks = _LearnedBocks;
            this.Luck = _Luck;
        }

        public void SavePlayers()
        {
            

            MyLog.Default.WriteLine("Saving Players...");
            MyLog.Default.Flush();
            foreach (var player in Core.players)
            {
                PlayerFile playerFile = new PlayerFile();
                playerFile.LearnedBocks = new HashSet<SerializableDefinitionId>(player.Value.LearnedBocks.Select(s => (SerializableDefinitionId)s)); //player.Value.LearnedBocks.Select(s => (SerializableDefinitionId)s).ToList()
                try
                {
                    using (var sw =
                      MyAPIGateway.Utilities.WriteFileInWorldStorage(string.Format(Settings.playerFile, player.Key), typeof(Core)))
                        sw.Write(MyAPIGateway.Utilities.SerializeToXML(playerFile));

                    MyLog.Default.WriteLine($"Player {player.Key} Saved!");
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"ERROR SaveData: {e.Message}");
                }
            }
            MyLog.Default.Flush();
        }

        public PlayerData Load(PlayerFile v)
        {
            this.LearnedBocks = new HashSet<MyDefinitionId>(v.LearnedBocks.Select(s => (MyDefinitionId)s), MyDefinitionId.Comparer);
            this.Luck = v.Luck;

            return this;
        }
    }
}
