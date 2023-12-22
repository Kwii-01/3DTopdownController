using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Controller {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class RBController : MonoBehaviour {
        public Transform inputSpace = default;
        [SerializeField] private float _maxSlope;
        [SerializeField, Range(0f, 100f)] private float _maxSnapSpeed = 100f;
        [SerializeField, Min(0f)] private float probeDistance = 1f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Rigidbody _rigidbody;

        public Rigidbody Rigidbody => this._rigidbody;
        public bool IsOnGround => this._groundContactCount > 0;
        public bool IsOnSteep => this._steepContactCount > 0;
        public event Action OnGround;

        private bool _desiredJump;
        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Vector3 _connectionVelocity;
        private Vector3 _contactNormal;
        private Vector3 _steepNormal;
        private Vector3 _connectionWorldPosition;
        private Vector3 _connectionLocalPosition;
        private Vector3 _rightAxis;
        private Vector3 _forwardAxis;
        private float _maxSpeed;
        private float _jumpHeight;
        private float _minGroundDotProduct;
        private int _groundContactCount;
        private int _stepsSinceLastGrounded;
        private int _stepsSinceLastJump;
        private int _steepContactCount;

        private Rigidbody _connectedBody;
        private Rigidbody _previousConnectedBody;

        private void Awake() {
            this.OnValidate();
        }

        private void Reset() {
            this._rigidbody = this.GetComponent<Rigidbody>();
        }

        private void OnValidate() {
            this._minGroundDotProduct = Mathf.Cos(this._maxSlope * Mathf.Deg2Rad);
        }

        private void FixedUpdate() {
            this._velocity = this._rigidbody.velocity;
            this.UpdateState();
            this.AdjustVelocity();
            if (this._desiredJump) {
                this.Jump(Physics.gravity);
            }
            this._rigidbody.velocity = this._velocity;
            this.ClearState();
        }

        private void OnCollisionEnter(Collision other) => this.EvaluateCollision(other);
        private void OnCollisionStay(Collision other) => this.EvaluateCollision(other);

        public void Move(Vector3 motion, float maxSpeed) {
            if (this.inputSpace) {
                this._rightAxis = this.ProjectDirectionOnPlane(this.inputSpace.right, Vector3.up);
                this._forwardAxis = this.ProjectDirectionOnPlane(this.inputSpace.forward, Vector3.up);
                this._desiredVelocity = this._forwardAxis * motion.z + this._rightAxis * motion.x;
            } else {
                this._rightAxis = this.ProjectDirectionOnPlane(Vector3.right, Vector3.up);
                this._forwardAxis = this.ProjectDirectionOnPlane(Vector3.forward, Vector3.up);
                this._desiredVelocity = motion;
            }
            this._maxSpeed = maxSpeed;
        }

        public void Stop() {
            this._desiredVelocity = Vector3.zero;
            this._velocity = this._rigidbody.velocity;
            this._velocity.x = 0;
            this._velocity.z = 0;
            this._velocity.y = this._rigidbody.velocity.y;
            this._rigidbody.velocity = this._velocity;
        }

        public void Jump(float jumpHeight) {
            this._desiredJump = true;
            this._jumpHeight = jumpHeight;
        }

        private void UpdateState() {
            if (this._stepsSinceLastGrounded < 500) {
                this._stepsSinceLastGrounded += 1;
            }
            if (this._stepsSinceLastJump < 500) {
                this._stepsSinceLastJump += 1;
            }
            if (this.IsOnGround || this.SnapToGround() || this.CheckSteepContacts()) {
                if (this._stepsSinceLastGrounded > 1) {
                    this.OnGround?.Invoke();
                }
                this._stepsSinceLastGrounded = 0;
                if (this._groundContactCount > 1) {
                    this._contactNormal.Normalize();
                }
            } else {
                this._contactNormal = Vector3.up;
            }
            if (this._connectedBody) {
                if (this._connectedBody.isKinematic || this._connectedBody.mass >= this._rigidbody.mass) {
                    this.UpdateConnectionState();
                }
            }
        }

        private void ClearState() {
            this._groundContactCount = 0;
            this._steepContactCount = 0;
            this._connectionVelocity = Vector3.zero;
            this._contactNormal = Vector3.zero;
            this._steepNormal = Vector3.zero;
            this._previousConnectedBody = this._connectedBody;
            this._connectedBody = null;
        }

        private void Jump(Vector3 gravity) {
            Vector3 jumpDirection = this._contactNormal;
            jumpDirection = (jumpDirection + Vector3.up).normalized;
            this._desiredJump = false;
            this._stepsSinceLastJump = 0;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * this._jumpHeight);
            float alignedSpeed = Vector3.Dot(this._velocity, jumpDirection);
            if (alignedSpeed > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            this._velocity += jumpDirection * jumpSpeed;
        }

        private void EvaluateCollision(Collision collision) {
            Vector3 normal;
            for (int i = 0; i < collision.contactCount; i++) {
                normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(Vector3.up, normal);
                if (upDot >= this._minGroundDotProduct) {
                    this._contactNormal += normal;
                    this._groundContactCount += 1;
                    this._connectedBody = collision.rigidbody;
                } else {
                    if (upDot > -0.01f) {
                        this._steepContactCount += 1;
                        this._steepNormal += normal;
                        if (this._groundContactCount == 0) {
                            this._connectedBody = collision.rigidbody;
                        }
                    }
                }
            }
        }

        private void AdjustVelocity() {
            Vector3 xAxis = ProjectDirectionOnPlane(this._rightAxis, this._contactNormal);
            Vector3 zAxis = ProjectDirectionOnPlane(this._forwardAxis, this._contactNormal);
            Vector3 relativeVelocity = this._velocity - this._connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);
            float maxSpeedChange = this._maxSpeed * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, this._desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, this._desiredVelocity.z, maxSpeedChange);
            this._velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
            return (direction - normal * Vector3.Dot(direction, normal)).normalized;
        }

        private bool SnapToGround() {
            if (this._stepsSinceLastGrounded > 1 || this._stepsSinceLastJump <= 2) {
                return false;
            }
            float speed = this._velocity.magnitude;
            if (speed > _maxSnapSpeed
                || !Physics.Raycast(this._rigidbody.position, -Vector3.up, out RaycastHit hit, this.probeDistance, this._groundMask)) {
                return false;
            }
            float upDot = Vector3.Dot(Vector3.up, hit.normal);
            if (upDot < this._minGroundDotProduct) {
                return false;
            }
            this._groundContactCount = 1;
            this._contactNormal = hit.normal;
            float dot = Vector3.Dot(this._velocity, hit.normal);
            if (dot > 0f) {
                this._velocity = (this._velocity - hit.normal * dot).normalized * speed;
            }
            this._connectedBody = hit.rigidbody;
            return true;
        }

        private bool CheckSteepContacts() {
            if (this._steepContactCount > 1) {
                this._steepNormal.Normalize();
                float upDot = Vector3.Dot(Vector3.up, this._steepNormal);
                if (upDot >= this._minGroundDotProduct) {
                    this._groundContactCount = 1;
                    this._contactNormal = this._steepNormal;
                    return true;
                }
            }
            return false;
        }

        private void UpdateConnectionState() {
            if (this._connectedBody == this._previousConnectedBody) {
                Vector3 connectionMovement = this._connectedBody.transform.TransformPoint(this._connectionLocalPosition) - this._connectionWorldPosition;
                this._connectionVelocity = connectionMovement / Time.deltaTime;
            }
            this._connectionWorldPosition = this._rigidbody.position;
            this._connectionLocalPosition = this._connectedBody.transform.InverseTransformPoint(this._connectionWorldPosition);
        }
    }
}
