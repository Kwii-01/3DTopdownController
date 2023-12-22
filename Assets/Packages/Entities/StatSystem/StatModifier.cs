using System;
using System.Collections.Generic;

using UnityEngine;

namespace StatSystem {

    public enum StatModType {
        Flat = 100,
        PercentAdd = 200,
        PercentMult = 300,
    }

    [Serializable]
    public class StatModifier {
        public float Value;
        public StatModType Type = StatModType.Flat;
        public int Order;
        public object Source;

        public StatModifier() {
        }

        public StatModifier(float value, StatModType type, int order, object source) : this() {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
        public StatModifier(float value, StatModType type, int order) : this(value, type, order, null) { }
        public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }

    }

}