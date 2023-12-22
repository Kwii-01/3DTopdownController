using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StatSystem {
    public class ModifierBehaviour : MonoBehaviour {
        [Serializable]
        public class Modifier {
            public StatType statType;
            public StatModifier data;
        }

        [SerializeField] private Modifier[] _modifiers;
        private StatBehaviour _target;

        private void OnDisable() {
            this.Remove();
        }

        public void Init(StatBehaviour statBehaviour) {
            this._target = statBehaviour;
            this.Add(this._target);
        }

        public void Add(StatBehaviour statBehaviour) {
            foreach (Modifier modifier in this._modifiers) {
                modifier.data.Source = this;
                statBehaviour.AddModifier(modifier.statType, modifier.data);
            }
        }

        public void Remove() {
            if (this._target != null) {
                foreach (Modifier modifier in this._modifiers) {
                    this._target.RemoveModifier(modifier.statType, modifier.data);
                }
            }
        }
    }
}