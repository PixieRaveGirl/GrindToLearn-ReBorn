using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;
using System.Reflection;
using System.Linq;

namespace Phoera
{
    public abstract class CoreBase : MySessionComponentBase
    {

        bool _initialized = false;
        bool _haveHandler = false;

        public bool InInitialization { get; private set; } = false;

        public abstract void Deinitialize();

        public new bool Initialized { get { return _initialized; } }

        public abstract bool Initialize(out MyUpdateOrder order);

        public override string ToString()
        {
            return "Phoera's Core Base class";
        }
        public virtual void UpdateBeforeSim()
        {

        }
        public virtual void UpdateAfterSim()
        {

        }
        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!_initialized)
            {
                InInitialization = true;
                MyUpdateOrder order;
                _initialized = Initialize(out order);
                InInitialization = false;
                if (!_initialized)
                {
                    return;
                }
                ChangeUpdateOrder(order);
                if (order.HasFlag(MyUpdateOrder.BeforeSimulation))
                    UpdateBeforeSim();
                return;
            }
            UpdateBeforeSim();
        }
        protected void ChangeUpdateOrder(MyUpdateOrder value)
        {
            MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(value));
        }

        public override void UpdateAfterSimulation()
        {
            UpdateAfterSim();
        }
        protected override void UnloadData()
        {
            if (_initialized)
            {
                Deinitialize();
            }
            _initialized = false;
        }
    }
}