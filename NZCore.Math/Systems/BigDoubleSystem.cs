// <copyright project="NZCore" file="BigDoubleSystem.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;

namespace NZCore
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct BigDoubleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            BigDouble.PowersOf10.Init();
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            BigDouble.PowersOf10.Dispose();
        }
    }
}