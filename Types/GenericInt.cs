// <copyright project="NZCore.Math" file="GenericInt.cs" version="1.2.2">
// Copyright © 2024 Thomas Enzenebner. All rights reserved.
// </copyright>

namespace Types
{
    public struct GenericInt
    {
        public int Value;

        public double ToDouble()
        {
            return Value;
        }
    }
    
    public struct GenericFloat
    {
        public float Value;

        public double ToDouble()
        {
            return Value;
        }
    }
}