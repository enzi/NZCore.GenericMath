// <copyright project="NZCore" file="BigDoubleSystem.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;
using UnityEngine;

namespace NZCore
{
    [UpdateInGroup(typeof(NZCoreInitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct BigDoubleSystem : ISystem
    {
        private BigDouble.PowersOf10 _lookup;

        public void OnCreate(ref SystemState state)
        {
            Debug.Log("Creating BigDouble lookup");
            _lookup = new BigDouble.PowersOf10();
            _lookup.Init();
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            _lookup.Dispose();
        }
    }
}