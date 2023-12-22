using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Controller;

using Entities;

using StatSystem;

using UnityEngine;

[RequireComponent(typeof(RBController))]
public class Character : Entity {

    [Header("Controls")]
    [SerializeField] private RBController _controller;
    [SerializeField, Range(0f, 10f)] private float _velocityChangeMultiplier;
    [SerializeField] private float _rotationSpeed;

    protected override void Reset() {
        base.Reset();
        this._controller = this.GetComponent<RBController>();
    }

    public void MoveToward(Vector3 direction) {
        float speed = this.statBehaviour.Get(StatType.MoveSpeed);
        this._controller.Move(direction * speed, speed * this._velocityChangeMultiplier);
    }

    public void Stop() {
        this._controller.Stop();
    }

    public void RotateDirection(Vector3 direction) {
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(direction), this._rotationSpeed * Time.deltaTime);
    }

}
