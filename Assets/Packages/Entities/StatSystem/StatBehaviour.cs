using System;
using System.Collections;
using System.Collections.Generic;

using AYellowpaper.SerializedCollections;

using UnityEngine;

namespace StatSystem {
    public enum StatType {
        Health,
        Damage,
        Range,
        MoveSpeed,
        AttackSpeed
    }

    public class StatBehaviour : MonoBehaviour {
        [SerializeField, SerializedDictionary("Type", "Base value")] private SerializedDictionary<StatType, Stat> _stats;
        public event Action<StatType, Stat> onStatChanged;

        public Stat Get(StatType type) => this._stats[type];

        public void AddModifier(StatType type, StatModifier mod) {
            this._stats[type].AddModifier(mod);
            this.onStatChanged?.Invoke(type, this._stats[type]);
        }

        public bool RemoveModifier(StatType type, StatModifier mod) {
            bool value = this._stats[type].RemoveModifier(mod);
            this.onStatChanged?.Invoke(type, this._stats[type]);
            return value;
        }

        public bool RemoveAllModifiersFromSource(StatType type, object source) {
            bool value = this._stats[type].RemoveAllModifiersFromSource(source);
            this.onStatChanged?.Invoke(type, this._stats[type]);
            return value;
        }
    }
}