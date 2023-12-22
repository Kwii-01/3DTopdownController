using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private Character _character;
    public Character Character => this._character;
}
