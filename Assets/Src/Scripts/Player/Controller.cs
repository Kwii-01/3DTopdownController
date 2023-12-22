using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Controller {
    public abstract class Controller<T> : MonoBehaviour where T : MonoBehaviour {
        [SerializeField] protected T _context;

        protected virtual void Reset() {
            this._context = this.GetComponent<T>();
        }
    }
}