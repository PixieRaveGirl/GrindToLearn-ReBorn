using System;
using System.Collections.Generic;
using System.Linq;
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
using VRage;
using System.Collections;
using System.Dynamic;
using System.Reflection;

/*  
  Welcome to Modding API. This is second of two sample scripts that you can modify for your needs,
  in this case simple script is prepared that will alter behaviour of sensor block
  This type of scripts will be executed automatically  when sensor (or your defined) block is added to world
 */
namespace Phoera.GringProgression
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Core : CoreBase
    {
        NetworkHandlerSystem nhs = new NetworkHandlerSystem(3);
        Settings settings = new Settings();

        Dictionary<MyDefinitionId, HashSet<MyDefinitionId>> variantGroups =
          new Dictionary<MyDefinitionId, HashSet<MyDefinitionId>>(MyDefinitionId.Comparer);

        Dictionary<long, ulong> userIds = new Dictionary<long, ulong>();
        /*  Unlock All Players <SteamID, TRUE>  */
        Dictionary<ulong, bool> unlockAllPlayers = new Dictionary<ulong, bool>();

        public static Dictionary<long, HashSet<MyDefinitionId>> playersData = new Dictionary<long, HashSet<MyDefinitionId>>();

        private MessageEventCaller<long> PlayerInit;
        private bool isInitialized = false;

        public MessageEventCaller<MyDefinitionId,bool> SendUnlockNotification { get; private set; }

        public override void Deinitialize()
        {
            try
            {
                MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= Controller_ControlledEntityChanged;
                nhs.Unload();
            }
            catch (Exception e)
            {

            }

        }

        public override bool Initialize(out MyUpdateOrder order)
        {
            order = MyUpdateOrder.NoUpdate;
            if (NetworkHandlerSystem.IsClient)
            {
                if (MyAPIGateway.Session.Player == null)
                {
                    return false;
                }
            }
            /* Handle Server side */
            if (NetworkHandlerSystem.IsServer)
            {
                /* LOAD World Settings */
                try
                {
                    using (var sw = MyAPIGateway.Utilities.ReadFileInWorldStorage(settings.configFile, typeof(Core)))
                        settings.Load(MyAPIGateway.Utilities.SerializeFromXML<SettingsFile>(sw.ReadToEnd()));
                }
                catch (Exception e)
                {
                    if(e.HResult == -2147024894) { // Unable to find the specified file. 
                        settings = settings.CreateDefaults();
                    } else {
                        MyLog.Default.WriteLine($"Possible Old Settings: {e.Message}");
                        MyLog.Default.WriteLine($"Possible Old SettingsStack: {e.StackTrace}");
                        MyLog.Default.Flush();

                        SettingsFile possibleOldSettings = new SettingsFile();
                        try
                        {
                            using (var sw = MyAPIGateway.Utilities.ReadFileInWorldStorage(settings.configFile, typeof(Core)))
                                possibleOldSettings = (SettingsFile)MyAPIGateway.Utilities.SerializeFromXML<Settings>(sw.ReadToEnd());

                            using (var sw = MyAPIGateway.Utilities.WriteFileInWorldStorage("config.xml.old", typeof(Core)))
                                sw.Write(MyAPIGateway.Utilities.SerializeToXML(possibleOldSettings));
                            /* Create Defaults then Load and Save Old Settings file */
                            settings = settings.CreateDefaults();
                            settings.Load(possibleOldSettings);
                            settings.Save(true);
                            MyLog.Default.WriteLine("Successfully Loaded Old World Settings:");

                        }
                        catch (Exception e1)
                        {
                            MyLog.Default.WriteLine($"ERROR Get World Settings: {e1.Message}");
                            MyLog.Default.WriteLine($"ERROR Get World SettingsStack: {e1.StackTrace}");
                            MyLog.Default.Flush();
                            settings = settings.CreateDefaults();
                        }
                        
                    }
                }

                /* Save after loading to add new default settings just incase we added more */
                settings.Save();

                try
                {
                    var cfg = MyAPIGateway.Utilities.ConfigDedicated;
                    cfg.Load();
                    ulong steamId;
                    foreach (string id in cfg.Administrators)
                    {
                        if (ulong.TryParse(id, out steamId))
                        {

                            unlockAllPlayers.Add(steamId, true);
                            MyLog.Default.WriteLine($"Admin Found {steamId}");
                        }
                    }
                    if (unlockAllPlayers.Count() == 0)
                    {
                        MyLog.Default.WriteLine($"No Admins");
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"Failed to load Server cfg file: {e.Message}");
                    MyLog.Default.Flush();
                }
                if (settings.AlwaysUnlocked == null)
                    settings.AlwaysUnlocked = new HashSet<SerializableDefinitionId>();
                MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, DestroyHandler);
                MyVisualScriptLogicProvider.PlayerResearchClearAll();
                PrepareCache();
            }
            /* Setup Client and Server wide communication*/
            try
            {
                PlayerInit = nhs.Create<long>(null, PlayerJoined, EventOptions.OnlyToServer);
                SendUnlockNotification = nhs.Create<MyDefinitionId, bool>(LearnedById, null, EventOptions.OnlyToTarget);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Network Notifications ERROR: {e.StackTrace}");
                MyLog.Default.Flush();
            }
            if (NetworkHandlerSystem.IsClient)
            {
                try
                {
                    MyVisualScriptLogicProvider.ResearchListWhitelist(true);
                    MyVisualScriptLogicProvider.PlayerResearchClear();
                    MyLog.Default.WriteLine($"Check ControlledEntity");
                    if (MyAPIGateway.Session.Player.Controller.ControlledEntity != null &&
                        MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity is IMyCharacter)
                    {
                        MyLog.Default.WriteLine($"InvokeOnGameThread Start");
                        MyLog.Default.Flush();
                        MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                        {
                            try
                            {
                                MyLog.Default.WriteLine($"Start InitPlayer");
                                MyLog.Default.Flush();
                                PlayerInit(MyAPIGateway.Session.Player.PlayerID);
                            }
                            catch (Exception e)
                            {
                                MyLog.Default.WriteLine($"PlayerInit: {e.Message}");
                                MyLog.Default.Flush();
                            }
                        });
                    }
                    else
                    {
                        MyLog.Default.WriteLine($"ControlledEntityChanged");
                        MyLog.Default.Flush();
                        MyAPIGateway.Session.Player.Controller.ControlledEntityChanged += Controller_ControlledEntityChanged;
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"NetworkHandlerSystem INIT: {e.Message}");
                    MyLog.Default.Flush();
                }
            }
            GetAllBlocks();
            MyLog.Default.WriteLine($"SUCCESSFUL INIT!");
            MyLog.Default.Flush();
            return true;
        }

        void LearnedById(MyDefinitionId id, bool success, ulong sender)
        {
            try
            {
                if (success)
                {
                    //MyAPIGateway.Utilities.ShowMessage("SpaceMaster", $"You can now build {MyDefinitionManager.Static.GetCubeBlockDefinition(id).DisplayNameText}.");
                    MyAPIGateway.Utilities.ShowNotification(
                      $"You can now build {MyDefinitionManager.Static.GetCubeBlockDefinition(id).DisplayNameText}.");
                } else
                {
                    MyAPIGateway.Utilities.ShowNotification(
                      $"{MyDefinitionManager.Static.GetCubeBlockDefinition(id).DisplayNameText} technology is out of your reach.",6000,"Red");
                }
            }
            catch (Exception e)
            {
                //just in case
                MyLog.Default.WriteLine($"ERROR LearnedById: {e.Message}");
            }
        }


        private void Controller_ControlledEntityChanged(VRage.Game.ModAPI.Interfaces.IMyControllableEntity arg1,
          VRage.Game.ModAPI.Interfaces.IMyControllableEntity arg2)
        {
            if (arg2 != null && arg2.Entity is IMyCharacter)
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    try
                    {
                        if (MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null)
                            PlayerInit(MyAPIGateway.Session.Player.PlayerID);
                    }
                    catch (Exception e)
                    {
                        MyLog.Default.WriteLine($"ERROR PlayerInit Entity Change: {e.Message}");
                        MyLog.Default.Flush();
                    }
                });
                MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= Controller_ControlledEntityChanged;
            }
        }

        void DestroyHandler(object target, MyDamageInformation damage)
        {
            if (damage.Type == MyDamageType.Grind)
            {

                if (target is IMySlimBlock)
                {
                    var slim = target as IMySlimBlock;
                    CalculatePlayers(slim.BlockDefinition.Id, slim.CubeGrid.GridIntegerToWorld(slim.Position), damage.AttackerId);
                }
                else if (target is IMyCubeBlock) //just in cause
                {
                    var fat = target as IMyCubeBlock;
                    CalculatePlayers(fat.BlockDefinition, fat.GetPosition(), damage.AttackerId);
                }
            }
        }

        void CalculatePlayers(MyDefinitionId blockId, Vector3D pos, long attackerId)
        {
            IMyEntity ent;
            if (settings.UseLearnFaction)
            {
                if (MyAPIGateway.Entities.TryGetEntityById(attackerId, out ent))
                {
                    var hand = ent as IMyHandheldGunObject<MyToolBase>;
                    if (hand != null && hand.DefinitionId.TypeId == typeof(MyObjectBuilder_AngleGrinder))
                    {
                        var players = new List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(players);
                        var pl = players.FirstOrDefault(p => p.IdentityId.Equals(hand.OwnerIdentityId));
                        var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(pl.PlayerID);
                        if (faction != null)
                        {
                            foreach (var player in players)
                            {
                                var f = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.PlayerID);
                                if (f != null && f.FactionId == faction.FactionId)
                                {
                                    UnlockById(blockId, player.PlayerID);
                                }
                            }
                        }
                    }
                }
            }
            else if (settings.UseLearnRadius && settings.LearnRadius > 0)
            {
                if (MyAPIGateway.Entities.TryGetEntityById(attackerId, out ent))
                {
                    var hand = ent as IMyHandheldGunObject<MyToolBase>;
                    if (hand != null && hand.DefinitionId.TypeId == typeof(MyObjectBuilder_AngleGrinder))
                    {
                        var sphere = new BoundingSphereD(ent.GetPosition(), settings.LearnRadius);
                        var players = new List<IMyPlayer>();
                        MyAPIGateway.Players.GetPlayers(players, p =>
                        {
                            return sphere.Contains(p.GetPosition()) != ContainmentType.Disjoint;
                        });
                        foreach (var player in players)
                        {
                            UnlockById(blockId, player.PlayerID);
                        }
                    }
                }
            }
            if (MyAPIGateway.Entities.TryGetEntityById(attackerId, out ent))
            {
                var hand = ent as IMyHandheldGunObject<MyToolBase>;
                if (hand != null && hand.DefinitionId.TypeId == typeof(MyObjectBuilder_AngleGrinder))
                {
                    var players = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(players);
                    var pl = players.FirstOrDefault(p => p.IdentityId.Equals(hand.OwnerIdentityId));
                    if (pl != null)
                    {
                        UnlockById(blockId, pl.PlayerID);
                    }
                }
            }
        }

        private void PrepareCache()
        {
            foreach (var cube in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>())
            {
                if (cube.BlockStages != null && cube.BlockStages.Length > 0)
                {
                    var ids = new HashSet<MyDefinitionId>(cube.BlockStages, MyDefinitionId.Comparer);
                    ids.Add(cube.Id);
                    foreach (var id in ids)
                    {
                        variantGroups[id] = ids;
                    }
                }
            }
        }

        private void GetAllBlocks()
        {
            IEnumerable<MyCubeBlockDefinition> cubes = MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>(); //System.Collections.Generic.IEnumerable

            try
            {
                using (var sw =
                  MyAPIGateway.Utilities.WriteFileInWorldStorage("blocks.xml", typeof(Core)))
                    sw.Write(MyAPIGateway.Utilities.SerializeToXML(cubes.Select(s => (SerializableDefinitionId)s.Id) //sw.Write(MyAPIGateway.Utilities.SerializeToXML(player.Value.Select(s => (SerializableDefinitionId)s)
                      .ToList()));
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Failed to Getting All Blocks: {e.Message}");
            }
        }

        public override void SaveData()
        {
            if (Initialized)
            {
                settings.Save();
            }
        }

        void PlayerJoined(long playerID, ulong sender)
        {
            MyLog.Default.WriteLine($"PlayerJoined The game! {playerID}");
            MyLog.Default.Flush();
            try
            {
                if (sender == 0)
                    sender = MyAPIGateway.Session?.Player?.SteamUserId ?? 0;
                userIds[playerID] = sender;

                if (settings.AdminUnlockAll)
                {
                    bool check;
                    if (unlockAllPlayers.TryGetValue(sender, out check))
                    {
                        if (check)
                        {
                            UnlockAllByID(playerID);
                            return;
                        }
                    }
                }
                var playerIds = (HashSet<MyDefinitionId>)null;
                try
                {
                    using (var sw =
                      MyAPIGateway.Utilities.ReadFileInWorldStorage(string.Format(settings.playerFile, playerID), typeof(Core)))
                    {
                        var ids = MyAPIGateway.Utilities.SerializeFromXML<List<SerializableDefinitionId>>(sw.ReadToEnd());
                        playerIds = new HashSet<MyDefinitionId>(ids.Select(s => (MyDefinitionId)s), MyDefinitionId.Comparer);
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"PlayerJoinedRead: {e.Message}");
                    MyLog.Default.WriteLine($"PlayerJoinedRead: {e.HResult}");

                    MyLog.Default.Flush();
                }

                if (playerIds == null)
                    playerIds = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
                if (playerIds.Count == 0)
                {
                    MyVisualScriptLogicProvider.ClearAllToolbarSlots(playerID);
                }

                foreach (var id in settings.AlwaysUnlocked)
                {
                    playerIds.Add(id);
                }

                playersData[playerID] = playerIds;
                foreach (var id in playerIds.ToList())
                {
                    UnlockById(id, playerID, true);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"PlayerJoined: {e.Message}");
                MyLog.Default.WriteLine($"PlayerJoinedStack: {e.StackTrace}");
                MyLog.Default.Flush();

            }
            MyLog.Default.Flush();
        }

        void UnlockById(MyDefinitionId blockId, long player, bool force = false)
        {
            try
            {
                var playerData = playersData[player];
                if (!force && playerData.Contains(blockId))
                {
                    return;
                }

                var ids = new HashSet<MyDefinitionId>();
                ids.Add(blockId);
                var cb = MyDefinitionManager.Static.GetCubeBlockDefinition(blockId);
                if (!cb.Public)
                    return;
                ulong steamId;

                try
                {
                    if (!force && userIds.TryGetValue(player, out steamId))
                    {
                        var lockedBlock = new SerializableDefinitionId();
                        if (settings.AlwaysLocked.TryGetValue(blockId, out lockedBlock))
                        {
                            MyLog.Default.WriteLine($"Learning Denied: {lockedBlock.ToString()}");
                            SendUnlockNotification(blockId, false, steamId);
                            return;
                        }
                        else
                        {
                            SendUnlockNotification(blockId, true, steamId);
                        }
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"ERROR Sending Unlock notification: {e.StackTrace}");
                    MyLog.Default.Flush();
                }
                var dg = MyDefinitionManager.Static.TryGetDefinitionGroup(cb.BlockPairName);
                if (dg != null)
                {
                    if (dg.Large != null)
                        ids.Add(dg.Large.Id);
                    if (dg.Small != null)
                        ids.Add(dg.Small.Id);
                }

                if (!cb.GuiVisible || (cb.BlockStages != null && cb.BlockStages.Length > 0))
                    foreach (var bid in ids.ToList())
                    {
                        HashSet<MyDefinitionId> blocks;
                        if (variantGroups.TryGetValue(bid, out blocks))
                        {
                            if (blocks != null)
                                foreach (var block in blocks)
                                {
                                    ids.Add(block);
                                }
                        }
                    }

                foreach (var id in ids)
                {
                    playerData.Add(id);
                    MyVisualScriptLogicProvider.PlayerResearchUnlock(player, id);
                }

            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"ERROR: {e.StackTrace}");
                MyLog.Default.Flush();
            }
        }

        void UnlockAllByID(long player)
        {
            if (Initialized)
            {
                foreach (var cube in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    if (cube != null && cube.Public)
                    {
                        MyVisualScriptLogicProvider.PlayerResearchUnlock(player, cube.Id);
                    }
                }
            }
        }
    }

    public class SettingsFile
    {
        public bool AdminUnlockAll { get; set; } = false;
        public bool UseLearnFaction { get; set; } = false;
        public float LearnRadius { get; set; } = 5;
        public bool UseLearnRadius { get; set; } = false;
        public HashSet<SerializableDefinitionId> AlwaysUnlocked { get; set; } = new HashSet<SerializableDefinitionId>();
        public HashSet<SerializableDefinitionId> AlwaysLocked { get; set; } = new HashSet<SerializableDefinitionId>();

        public static explicit operator SettingsFile(Settings v)
        {
            SettingsFile copySettings = new SettingsFile();
            copySettings.AdminUnlockAll = v.AdminUnlockAll;
            copySettings.UseLearnFaction = v.UseLearnFaction;
            copySettings.UseLearnRadius = v.UseLearnRadius;
            copySettings.LearnRadius = v.LearnRadius;
            copySettings.AlwaysUnlocked = v.AlwaysUnlocked;
            copySettings.AlwaysLocked = v.AlwaysLocked;
            return copySettings;
        }
    }

    public class Settings
    {
        /* Default Settings */
        private bool _isDirty = false;
        public bool AdminUnlockAll { get; set; } = false;
        public bool UseLearnFaction { get; set; } = false;
        public float LearnRadius { get; set; } = 5;
        public bool UseLearnRadius { get; set; } = false;
        public HashSet<SerializableDefinitionId> AlwaysUnlocked { get; set; } = new HashSet<SerializableDefinitionId>();
        public HashSet<SerializableDefinitionId> AlwaysLocked { get; set; } = new HashSet<SerializableDefinitionId>();

        public string configFile = "config.xml";
        public string playerFile = "{0}.xml";

        public bool isDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
            }
        }

        public Settings CreateDefaults()
        {
            try
            {
                this.AlwaysUnlocked = new HashSet<SerializableDefinitionId>(new SerializableDefinitionId[] {
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorSlope2Tip"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorSlope2Tip"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorSlope2Base"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorSlope2Base"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/Window1x2Flat"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/Window1x2FlatInv"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorSlope"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorCorner"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeBlockArmorCornerInv"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeHalfArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/LargeHalfSlopeArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorSlope"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorCorner"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/SmallBlockArmorCornerInv"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/HalfArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_CubeBlock/HalfSlopeArmorBlock"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/SmallWheel1x1"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/Wheel1x1"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/SmallWheel5x5"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/SmallWheel3x3"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/Wheel5x5"),
                              MyDefinitionId.Parse("MyObjectBuilder_Wheel/Wheel3x3"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTubeCurved"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTubeCurvedMedium"),
                              MyDefinitionId.Parse("MyObjectBuilder_Conveyor/LargeBlockConveyor"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTube"),
                              MyDefinitionId.Parse("MyObjectBuilder_Conveyor/SmallShipConveyorHub"),
                              MyDefinitionId.Parse("MyObjectBuilder_Conveyor/SmallBlockConveyor"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTubeSmall"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTubeSmallCurved"),
                              MyDefinitionId.Parse("MyObjectBuilder_ConveyorConnector/ConveyorTubeMedium"),
                              MyDefinitionId.Parse("MyObjectBuilder_InteriorLight/SmallBlockLight_1corner"),
                              MyDefinitionId.Parse("MyObjectBuilder_InteriorLight/LargeBlockLight_1corner"),
                              MyDefinitionId.Parse("MyObjectBuilder_InteriorLight/SmallBlockLight_2corner"),
                              MyDefinitionId.Parse("MyObjectBuilder_InteriorLight/LargeBlockLight_2corner"),
                              MyDefinitionId.Parse("MyObjectBuilder_Reactor/SmallBlockSmallGenerator"),
                              MyDefinitionId.Parse("MyObjectBuilder_Reactor/LargeBlockSmallGenerator")
                    });

                this.AlwaysLocked = new HashSet<SerializableDefinitionId>(
                    new SerializableDefinitionId[] {
                            MyDefinitionId.Parse("MyObjectBuilder_Projector/SmallProjector"),
                            MyDefinitionId.Parse("MyObjectBuilder_Projector/LargeProjector")
                    });

                MyLog.Default.WriteLine("Setting Defaults Loaded");
                this.isDirty = true;
                return this;
            }
            catch (Exception e)
            {
                //just in case
                MyLog.Default.WriteLine($"ERROR Settings: {e.Message}");
                MyLog.Default.WriteLine($"ERROR SettingsStack: {e.StackTrace}");
                return null;
            }
        }
        public void Save(bool force = false)
        {
            if (this._isDirty || force)
            {
                this.SaveSettings();
            }

            this.SavePlayers();

        }
        public void SaveSettings()
        {
            SettingsFile cloneSettings = new SettingsFile();

            cloneSettings.AdminUnlockAll = this.AdminUnlockAll;
            cloneSettings.UseLearnFaction = this.UseLearnFaction;
            cloneSettings.UseLearnRadius = this.UseLearnRadius;
            cloneSettings.LearnRadius = this.LearnRadius;
            cloneSettings.AlwaysUnlocked = this.AlwaysUnlocked;
            cloneSettings.AlwaysLocked = this.AlwaysLocked;

            MyLog.Default.WriteLine("Saving Settings...");
            MyLog.Default.Flush();

            using (var sw = MyAPIGateway.Utilities.WriteFileInWorldStorage(configFile, typeof(Core)))
                sw.Write(MyAPIGateway.Utilities.SerializeToXML((SettingsFile)cloneSettings));
            this._isDirty = false;

            MyLog.Default.WriteLine("Settings Saved!");
            MyLog.Default.Flush();
        }

        public void SavePlayers()
        {
            MyLog.Default.WriteLine("Saving Players...");
            MyLog.Default.Flush();
            foreach (var player in Core.playersData)
            {
                try
                {
                    using (var sw =
                      MyAPIGateway.Utilities.WriteFileInWorldStorage(string.Format(playerFile, player.Key), typeof(Core)))
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

        public static explicit operator Settings(SettingsFile v)
        {
            Settings copySettings = new Settings();
            copySettings.AdminUnlockAll = v.AdminUnlockAll;
            copySettings.UseLearnFaction = v.UseLearnFaction;
            copySettings.UseLearnRadius = v.UseLearnRadius;
            copySettings.LearnRadius = v.LearnRadius;
            copySettings.AlwaysUnlocked = v.AlwaysUnlocked ?? copySettings.AlwaysUnlocked;
            copySettings.AlwaysLocked = v.AlwaysLocked ?? copySettings.AlwaysLocked;
            return copySettings;
        }

        public void Load(SettingsFile v)
        {
            this.AdminUnlockAll = v.AdminUnlockAll;
            this.UseLearnFaction = v.UseLearnFaction;
            this.UseLearnRadius = v.UseLearnRadius;
            this.LearnRadius = v.LearnRadius;
            this.AlwaysUnlocked = v.AlwaysUnlocked ?? this.AlwaysUnlocked;
            this.AlwaysLocked = v.AlwaysLocked ?? this.AlwaysLocked;
        }
    }
}