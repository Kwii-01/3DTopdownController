using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StatSystem {
    public class Health {
        public float Max { get; private set; } = 0f;
        private float _value;

        public float Value {
            get {
                return _value;
            }
            set {
                if (value != _value) {
                    this._value = value;
                    this.onHealthChanged?.Invoke(_value);
                }
            }
        }

        public bool IsAlive => this.Value > 0f;
        public event Action<float> onHealthChanged;

        public Health(StatBehaviour statBehaviour) {
            statBehaviour.onStatChanged += this.OnStatChanged;
            this.Max = statBehaviour.Get(StatType.Health).Value;
            this._value = this.Max;
        }

        private void OnStatChanged(StatType statType, Stat stat) {
            if (statType == StatType.Health) {
                this.Max = stat.Value;
                if (this.Max < this._value) {
                    this.Value = this.Max;
                }
            }
        }
    }
}