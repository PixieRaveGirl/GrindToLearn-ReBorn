using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Phoera.GringProgression
{
    public class Progression
    {
        public Dictionary<MyDefinitionId, BlockInformation> ComponentsCosts { get; } = new Dictionary<MyDefinitionId, BlockInformation>();
        public Dictionary<String, List<BlockInformation>> typeList = new Dictionary<String, List<BlockInformation>>();
        public Dictionary<String, List<BlockInformation>> TechTree = new Dictionary<String, List<BlockInformation>>();
        Dictionary<MyDefinitionId, HashSet<MyDefinitionId>> variantGroups =
          new Dictionary<MyDefinitionId, HashSet<MyDefinitionId>>(MyDefinitionId.Comparer);

        Dictionary<String, double> LuckValues = new Dictionary<String, double>();

        public double getLuckValueByBlockId(MyDefinitionId blockId)
        {
            String blockType = this.GetbyBlockId(blockId).Type;
            double luck;
            LuckValues.TryGetValue(blockType, out luck);
            if (luck <= 0) { luck = 1; }
            return luck;
        }

        public double getLuckValueByType(String Type)
        {
            double luck;
            LuckValues.TryGetValue(Type, out luck);
            if(luck<=0) { luck = 1; }
            return luck;
        }

        public BlockInformation GetbyBlockId(MyDefinitionId blockId)
        {
            BlockInformation BI = new BlockInformation();
            ComponentsCosts.TryGetValue(blockId, out BI);

            return BI;
        }

        private void CalculateLuck()
        {
            LuckValues.Add("Warhead", 1);
            LuckValues.Add("CubeBlock", -2);
        }

        public BlockInformation GetNextBlock(MyDefinitionId blockId, HashSet<MyDefinitionId> alreadyKnownBlocks, bool forceNext = false)
        {
            if (blockId.TypeId.IsNull) { return (BlockInformation)null; }

            BlockInformation CurrentBI = GetbyBlockId(blockId);

            List<BlockInformation> blockList = new List<BlockInformation>();
            typeList.TryGetValue(CurrentBI.Type, out blockList);

            /* Maybe make this a toggle in settings, This would go with Learning everything under the current value of this block */
            bool Next = true;
            BlockInformation BI = new BlockInformation();
            BlockInformation Lowest = new BlockInformation();
            foreach (BlockInformation selectedBI in blockList)
            {
                if (Next && CurrentBI.Group == selectedBI.Group && !alreadyKnownBlocks.Contains(selectedBI.BlockId) || Next && forceNext && !alreadyKnownBlocks.Contains(selectedBI.BlockId))
                {
                    return selectedBI;
                }
                if (selectedBI.BlockId.ToString() == CurrentBI.BlockId.ToString() && CurrentBI.Group == selectedBI.Group)
                {
                    Next = true;
                }
            }

            return (BlockInformation)null;
        }

        public HashSet<MyDefinitionId> GetBlocksUnderBlockId(MyDefinitionId blockId)
        {
            if (blockId.TypeId.IsNull) { return (HashSet<MyDefinitionId>)null; }
            HashSet<MyDefinitionId> Blocks = new HashSet<MyDefinitionId>();

            BlockInformation CurrentBI = this.GetbyBlockId(blockId);

            List<BlockInformation> blockList = new List<BlockInformation>();
            typeList.TryGetValue(CurrentBI.Type, out blockList);
            if (blockList.Count() > 0)
            {
                foreach (BlockInformation selectedBI in blockList)
                {
                    if (CurrentBI.TechPos > selectedBI.TechPos && CurrentBI.Group == selectedBI.Group)
                    {
                        Blocks.Add(selectedBI.BlockId);
                    }
                }
                /*
                foreach (BlockInformation selectedBI in blockList)
                {
                    //MyLog.Default.WriteLine($"compare {selectedBI.blockId.ToString()} <> {blockId.ToString()}");
                    if (selectedBI.BlockId.ToString() == blockId.ToString() && CurrentBI.Group == selectedBI.Group)
                    {
                        return Blocks;
                    }
                    /* Add Blocks of the same group and size 
                    if (CurrentBI.CubeSize == selectedBI.CubeSize && CurrentBI.Group == selectedBI.Group)
                    {
                        Blocks.Add(selectedBI.BlockId);
                    }
                }*/
            }
            return Blocks;
            //return (HashSet<MyDefinitionId>)null;
        }

        public int CurrentLearningCountByType(MyDefinitionId blockId, HashSet<MyDefinitionId> alreadyKnownBlocks)
        {
            if (blockId.TypeId.IsNull) { return -1;}
                HashSet<MyDefinitionId> Blocks = new HashSet<MyDefinitionId>();

            BlockInformation CurrentBI = this.GetbyBlockId(blockId);

            List<BlockInformation> blockList = new List<BlockInformation>();
            typeList.TryGetValue(CurrentBI.Type, out blockList);
            int pos = 0;
            foreach (BlockInformation selectedBI in blockList)
            {
                if (selectedBI.Group == CurrentBI.Group && alreadyKnownBlocks.Contains(selectedBI.BlockId))
                {
                    pos++;
                    /*if (CurrentBI.TechPos >= pos)
                    {
                        pos++;
                    }*/
                }
            }

            return pos;
        }

        public int GetBlockPosition(MyDefinitionId blockId)
        {
            if (blockId.TypeId.IsNull) { return -1; }

            List<MyDefinitionId> Blocks = new List<MyDefinitionId>();
            BlockInformation CurrentBI = this.GetbyBlockId(blockId);

            return CurrentBI.TechPos;
            /*List<BlockInformation> blockList = new List<BlockInformation>();
            typeList.TryGetValue(CurrentBI.Type, out blockList);
            if (blockList.Count() > 0)
            {
                int pos = 0;
                foreach (BlockInformation selectedBI in blockList)
                {
                    if (CurrentBI.Group == selectedBI.Group)
                    {
                        MyCubeBlockDefinition cb = new MyCubeBlockDefinition();
                        MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(selectedBI.BlockId, out cb);

                        if (!Blocks.Contains(cb.Id))
                        {
                            pos++; Blocks.Add(cb.Id);
                            if (cb.BlockStages != null && cb.BlockStages.Length > 0)
                            {
                                //pos++;
                                var ids = new HashSet<MyDefinitionId>(cb.BlockStages, MyDefinitionId.Comparer);
                                ids.Add(cb.Id);
                                foreach (var id in ids)
                                {
                                    if (!Blocks.Contains(id))
                                    {
                                        if (!this.GetbyBlockId(id).SmallLargeBlockId.IsNull()) { Blocks.Add(this.GetbyBlockId(id).SmallLargeBlockId); };
                                        Blocks.Add(id);
                                    }
                                }
                            }
                        }
                        //pos++;
                    }
                    /* Add Blocks of the same group and size 
                    if (selectedBI.BlockId.ToString() == blockId.ToString() && CurrentBI.Group == selectedBI.Group)
                    {
                        //pos = Blocks.Count() - pos;
                        //pos++;
                        return pos;
                    }
                    
                }
            }*/
            return -1;
        }

        public int GetBlockTechTreeCount(MyDefinitionId blockId)
        {
            if (blockId.TypeId.IsNull) { return -1; }
            HashSet<MyDefinitionId> Blocks = new HashSet<MyDefinitionId>();
            BlockInformation CurrentBI = this.GetbyBlockId(blockId);
            List<BlockInformation> blockList = new List<BlockInformation>();
            typeList.TryGetValue(CurrentBI.Type, out blockList);

            //blockList.AsQueryable();

            //blockList = (List<BlockInformation>)blockList.Where(s => s.Group == CurrentBI.Group);
            int pos = -1;
            if (blockList.Count() > 0)
            {

                foreach (BlockInformation selectedBI in blockList)
                {
                    if(selectedBI.Group == CurrentBI.Group)
                    {
                        if(selectedBI.TechPos > pos)
                        {
                            pos = selectedBI.TechPos;
                        }
                    }
                }
            }

            return pos;//blockList.Last().TechPos;
            /*
            int pos = -1;
            int posGrouped = 0;
            if (blockList.Count() > 0)
            {

                foreach (BlockInformation selectedBI in blockList)
                {
                    /* Add Blocks of the same group and size 
                    if (CurrentBI.Group == selectedBI.Group)
                    {
                        MyCubeBlockDefinition cb = new MyCubeBlockDefinition();
                        MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(selectedBI.BlockId, out cb);

                        if (!Blocks.Contains(cb.Id))
                        {
                            pos++; Blocks.Add(cb.Id);
                            if (cb.BlockStages != null && cb.BlockStages.Length > 0)
                            {
                                //pos++;
                                //pos++;
                                var ids = new HashSet<MyDefinitionId>(cb.BlockStages, MyDefinitionId.Comparer);
                                ids.Add(cb.Id);
                                //pos = pos - ids.Count();
                                foreach (var id in ids)
                                {
                                    if (!Blocks.Contains(id))
                                    {
                                        MyLog.Default.WriteLine($"BlockStages: {(SerializableDefinitionId)id}");
                                        if (!this.GetbyBlockId(id).SmallLargeBlockId.IsNull()) { Blocks.Add(this.GetbyBlockId(id).SmallLargeBlockId); posGrouped++; };
                                        Blocks.Add(id);
                                        posGrouped++;
                                    }
                                }
                            }
                        }
                        //pos++;
                        //Blocks.Add(cb.Id);
                        
                    }
                }
            }
            return pos;

            foreach (BlockInformation selectedBI in blockList)
            {
                /* Add Blocks of the same group and size 
                if (CurrentBI.CubeSize == selectedBI.CubeSize && CurrentBI.Group == selectedBI.Group)
                {
                    MyCubeBlockDefinition cb = new MyCubeBlockDefinition();
                    MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(selectedBI.BlockId, out cb);

                    if (!Blocks.Contains(cb.Id)) { pos++; Blocks.Contains(cb.Id); }
                    if (cb.BlockStages != null && cb.BlockStages.Length > 0)
                    {
                        pos++;
                        //pos++;
                        var ids = new HashSet<MyDefinitionId>(cb.BlockStages, MyDefinitionId.Comparer);
                        ids.Add(cb.Id);
                        //pos = pos - ids.Count();
                        foreach (var id in ids)
                        {
                            MyLog.Default.WriteLine($"BlockStages: {(SerializableDefinitionId)id}");
                            if(!Blocks.Contains(id))
                            {
                                Blocks.Add(id);
                            }
                        }
                    }
                    //pos++;
                    //Blocks.Add(cb.Id);

                }
            }*/
        }

        void SetTechPos()
        {
            foreach (List<BlockInformation> blockList in typeList.Values)
            {
                HashSet<MyDefinitionId> Blocks = new HashSet<MyDefinitionId>();
                HashSet<MyDefinitionId> tempBlocks = new HashSet<MyDefinitionId>();
                HashSet<string> BlockPairNames = new HashSet<string>();

                int pos = 0;
                if (blockList.Count() > 0)
                {

                    foreach (BlockInformation selectedBI in blockList)
                    {
                        /* Add Blocks of the same group and size */

                        MyCubeBlockDefinition cb = new MyCubeBlockDefinition();
                        MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(selectedBI.BlockId, out cb);

                        if (!Blocks.Contains(cb.Id) && !BlockPairNames.Contains(cb.BlockPairName))
                        {
                            tempBlocks = new HashSet<MyDefinitionId>();

                            pos++;
                            Blocks.Add(cb.Id);
                            tempBlocks.Add(cb.Id);
                            BlockPairNames.Add(cb.BlockPairName);
                            selectedBI.TechPos = pos;


                            var dg = MyDefinitionManager.Static.TryGetDefinitionGroup(cb.BlockPairName);
                            if (dg != null)
                            {
                                if (dg.Large != null)
                                {
                                    Blocks.Add(dg.Large.Id);
                                    tempBlocks.Add(dg.Large.Id);
                                    BlockPairNames.Add(dg.Large.BlockPairName);
                                    if (this.GetbyBlockId(dg.Large.Id) != null)
                                    {
                                        this.GetbyBlockId(dg.Large.Id).TechPos = pos;
                                    }
                                }
                                if (dg.Small != null)
                                {
                                    Blocks.Add(dg.Small.Id);
                                    tempBlocks.Add(dg.Small.Id);
                                    BlockPairNames.Add(dg.Small.BlockPairName);
                                    if (this.GetbyBlockId(dg.Small.Id) != null)
                                    {
                                        this.GetbyBlockId(dg.Small.Id).TechPos = pos;
                                    }
                                }
                            }

                            if (!cb.GuiVisible || (cb.BlockStages != null && cb.BlockStages.Length > 0))
                            {
                                foreach (var bid in tempBlocks.ToList())
                                {
                                    HashSet<MyDefinitionId> blocks;
                                    if (variantGroups.TryGetValue(bid, out blocks))
                                    {
                                        if (blocks != null)
                                            foreach (var block in blocks)
                                            {
                                                MyLog.Default.WriteLine($"BlockStages: {(SerializableDefinitionId)block}");
                                                BlockPairNames.Add(this.GetbyBlockId(block).PairName);
                                                Blocks.Add(block);
                                                this.GetbyBlockId(block).TechPos = pos;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public Progression()
        {

            IEnumerable<MyBlueprintDefinitionBase> blues = MyDefinitionManager.Static.GetBlueprintDefinitions(); //System.Collections.Generic.IEnumerable

            foreach (MyBlueprintDefinitionBase blue in blues)
            {
                //BlockInformation blockInfo = new BlockInformation();
                //blockInfo.BlueprintId = blue.Id;
                //MyDefinitionId blockId = new MyDefinitionId();
                //blockInfo.Results = new HashSet<SerializableDefinitionId>();
                MyDefinitionId BlockId = new MyDefinitionId();
                MyDefinitionId.TryParse("MyObjectBuilder_" + blue.Id.SubtypeId.String, out BlockId);

                if (BlockId.TypeId.IsNull) { continue; }
                MyCubeBlockDefinition cb = new MyCubeBlockDefinition();
                MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(BlockId, out cb);
                HashSet<SerializableDefinitionId> results = new HashSet<SerializableDefinitionId>();

                String group = "Default";
                float efficiency = 0;
                String cubeSize = cb.CubeSize.ToString();
                String _Type = blue.Id.SubtypeId.String.Split('/')[0];
                string PairName = cb.BlockPairName;

                MyThrustDefinition thrust = new MyThrustDefinition();
                MyReactorDefinition reactor = new MyReactorDefinition();
                MyCargoContainerDefinition cargo = new MyCargoContainerDefinition();
                float Cost = 0;
                if (MyDefinitionManager.Static.TryGetDefinition<MyThrustDefinition>(BlockId, out thrust))
                {
                    if (thrust.FuelConverter.FuelId.IsNull())
                    {
                        group = thrust.ThrusterType.String;
                    }
                    else
                    {
                        group = thrust.FuelConverter.FuelId.SubtypeName;
                    }
                    //Cost = thrust.ForceMagnitude;
                    efficiency = thrust.ForceMagnitude;//thrust.MaxPowerConsumption;
                }
                else if (MyDefinitionManager.Static.TryGetDefinition<MyReactorDefinition>(BlockId, out reactor))
                {
                    group = reactor.FuelId.SubtypeId.String;
                }
                else if (MyDefinitionManager.Static.TryGetDefinition<MyCargoContainerDefinition>(BlockId, out cargo))
                {
                    efficiency = cargo.InventorySize.Sum;
                }

                string resultCount = blue.DisplayNameText;
                SerializableDefinitionId SerializedBlueprintId = blue.Id;

                List<MyBlueprintDefinitionBase.Item> recipe = new List<MyBlueprintDefinitionBase.Item>();
                foreach (MyBlueprintDefinitionBase.Item item in blue.Results)
                {
                    results.Add(item.Id);
                }

                SerializableDefinitionId smallLargeBlockId = new SerializableDefinitionId();
                var dg = MyDefinitionManager.Static.TryGetDefinitionGroup(cb.BlockPairName);
                if (dg != null)
                {
                    if (dg.Large != null)
                        smallLargeBlockId = dg.Large.Id;
                    if (dg.Small != null)
                        smallLargeBlockId = dg.Small.Id;
                }

                //MyLog.Default.WriteLine($"Id {(SerializableDefinitionId)blue.Results[0].Id}");
                foreach (var item in blue.Prerequisites)
                {
                    Cost += item.Amount.RawValue;
                    recipe.Add(item);
                }
                MyLog.Default.WriteLine($"Cost {Cost}");
                if (BlockId != null)
                {
                    SerializableDefinitionId SerializedBlockId = BlockId;
                    ComponentsCosts.Add(BlockId, new BlockInformation(BlockId, SerializedBlockId, results, recipe, Cost, group, cubeSize, efficiency, _Type, smallLargeBlockId, PairName));

                }
            }

            List<BlockInformation> existingKey = new List<BlockInformation>();
            foreach (BlockInformation BI in ComponentsCosts.Values)
            {

                if (typeList.TryGetValue(BI.Type, out existingKey))
                {
                    existingKey.Add(BI);
                }
                else
                {
                    // Create if not exists in dictionary
                    List<BlockInformation> temp = new List<BlockInformation>();
                    temp.Add(BI);
                    typeList.Add(BI.Type, temp);
                }
            }

            foreach (List<BlockInformation> BIList in typeList.Values)
            {
                BIList.Sort((pair1, pair2) => pair1.BlueprintCost.CompareTo(pair2.BlueprintCost));
            }

            //Set Actual Positon on Each Block, so later we may get the current position out of the techtree
            PrepareCache();
            SetTechPos();
            CalculateLuck();
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
    }

    public class BlockInformation
    {
        public float BlueprintCost { get; set; } = 0;
        public MyDefinitionId BlockId { get; set; }
        public SerializableDefinitionId SerializedBlockId { get; set; }
        public string ResultCount { get; set; }
        public HashSet<SerializableDefinitionId> Results { get; set; }
        public String Group { get; set; }
        public float Efficiency { get; set; }
        public String CubeSize { get; set; }
        public String Type { get; set; }
        public string PairName { get; set; }
        public int TechPos { get; set; }
        public SerializableDefinitionId SmallLargeBlockId { get; set; }

        List<MyBlueprintDefinitionBase.Item> Recipe = new List<MyBlueprintDefinitionBase.Item>();

        public BlockInformation() { }

        public BlockInformation(MyDefinitionId _BlockId, SerializableDefinitionId _SerializedBlockId, HashSet<SerializableDefinitionId> _Results, List<MyBlueprintDefinitionBase.Item> _Recipe, float _BlueprintCost, String _Group, String _CubeSize, float _Efficiency, String _Type, SerializableDefinitionId _SmallLargeBlockId, string _PairName)
        {
            BlueprintCost = _BlueprintCost;
            Recipe = _Recipe;
            BlockId = _BlockId;
            SerializedBlockId = _SerializedBlockId;
            Results = _Results;
            Group = _Group;
            CubeSize = _CubeSize;
            Efficiency = _Efficiency;
            Type = _Type;
            SmallLargeBlockId = _SmallLargeBlockId;
            PairName = _PairName;
        }
    }
}
