using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace StatSystem {

    [Serializable]
    public class Stat {
        public float BaseValue;
        private bool _isDirty = true;
        private float _value;
        private float _lastBaseValue = float.MinValue;
        public float Value {
            get {
                if (this._isDirty || this._lastBaseValue != this.BaseValue) {
                    this._lastBaseValue = this.BaseValue;
                    this._value = CalculateFinalValue();
                    this._isDirty = false;
                }
                return this._value;
            }
        }

        private readonly List<StatModifier> statModifiers;
        public readonly ReadOnlyCollection<StatModifier> StatModifiers;

        public Stat() {
            this.statModifiers = new List<StatModifier>();
            this.StatModifiers = this.statModifiers.AsReadOnly();
        }

        public Stat(float baseValue) : this() {
            this.BaseValue = baseValue;
        }

        public void AddModifier(StatModifier mod) {
            this._isDirty = true;
            this.statModifiers.Add(mod);
            this.statModifiers.Sort(this.CompareModifierOrder);
        }

        public bool RemoveModifier(StatModifier mod) {
            if (this.statModifiers.Remove(mod)) {
                this._isDirty = true;
                return true;
            }
            return false;
        }


        public bool RemoveAllModifiersFromSource(object source) {
            bool didRemove = false;

            for (int i = this.statModifiers.Count - 1; i >= 0; i--) {
                if (this.statModifiers[i].Source == source) {
                    this._isDirty = true;
                    didRemove = true;
                    this.statModifiers.RemoveAt(i);
                }
            }
            return didRemove;
        }

        private float CalculateFinalValue() {
            float finalValue = this.BaseValue;
            float sumPercentAdd = 0;
            StatModifier mod;

            for (int i = 0; i < this.statModifiers.Count; i++) {
                mod = this.statModifiers[i];
                if (mod.Type == StatModType.Flat) {
                    finalValue += mod.Value;
                } else if (mod.Type == StatModType.PercentAdd) {
                    sumPercentAdd += mod.Value;
                    if (i + 1 >= this.statModifiers.Count || this.statModifiers[i + 1].Type != StatModType.PercentAdd) {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                } else if (mod.Type == StatModType.PercentMult) {
                    finalValue *= 1 + mod.Value;
                }
            }
            return (float)Math.Round(finalValue, 4);
        }


        private int CompareModifierOrder(StatModifier a, StatModifier b) {
            if (a.Order < b.Order)
                return -1;
            else if (a.Order > b.Order)
                return 1;
            return 0;
        }

        public static implicit operator float(Stat stat) {
            return stat.Value;
        }


    }
}