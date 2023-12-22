
using StatSystem;

using UnityEngine;

namespace Entities {
    [RequireComponent(typeof(StatBehaviour))]
    public class Entity : MonoBehaviour {
        [SerializeField] protected StatBehaviour statBehaviour;
        public Health health { get; private set; } = default;

        protected virtual void Awake() {
            this.health = new Health(this.statBehaviour);
        }

        protected virtual void Reset() {
            this.statBehaviour = this.GetComponent<StatBehaviour>();
        }

        protected virtual void Takedamages(Entity source, float damages) {
            this.health.Value -= damages;
            if (this.health.IsAlive == false) {
                this.OnDeath(source);
            }
        }

        protected virtual void OnDeath(Entity killer) {

        }
    }
}