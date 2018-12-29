using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    [AddComponentMenu("Fingers Gestures/Component/First Person Controller", 5)]
    public class FingersFirstPersonControllerScript : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Player main rigid body")]
        public Rigidbody Player;

        [Tooltip("Player feet collider. Used to determine if jump is possible.")]
        public BoxCollider PlayerFeet;

        [Header("Control")]
        [Tooltip("Move speed")]
        [Range(0.1f, 100.0f)]
        public float MoveSpeed = 5.0f;

        [Tooltip("Higher values reduce move speed faster as pan vertical approaches 0.")]
        [Range(0.0f, 1.0f)]
        public float MovePower = 0.5f;

        [Tooltip("Rotation speed for sideways pan")]
        [Range(0.0f, 10.0f)]
        public float RotateSpeed = 3.0f;

        [Tooltip("Higher values reduce rotation faster as pan horizontal approaches 0.")]
        [Range(0.0f, 16.0f)]
        public float RotationPower = 2.0f;

        [Tooltip("Jump speed/power")]
        [Range(0.0f, 32.0f)]
        public float JumpSpeed = 10.0f;

        [Tooltip("How often the player can jump")]
        [Range(0.0f, 3.0f)]
        public float JumpCooldown = 0.3f;

        [Tooltip("The layers the player may jump off of")]
        public LayerMask JumpMask = -1;

        private float jumpTimer;

        private float? forwardSpeed;
        private float? sideSpeed;
        private readonly Collider[] tempResults = new Collider[8];

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            // calculate new velocity
            Vector3 velForward = Vector3.zero;
            Vector3 velUp = new Vector3(0.0f, Player.velocity.y, 0.0f);
            if (forwardSpeed != null)
            {
                velForward = Player.transform.forward * forwardSpeed.Value;
            }
            Vector3 vel = velForward + velUp;
            Player.velocity = vel;

            // reduce jump timer
            jumpTimer -= Time.deltaTime;

            // rotate if side speed is set
            if (sideSpeed != null)
            {
                float rotateSpeed = Mathf.Sign(sideSpeed.Value) * RotateSpeed * Mathf.Pow(Mathf.Abs(sideSpeed.Value), RotationPower);
                Player.angularVelocity = new Vector3(0.0f, rotateSpeed, 0.0f);
            }

            // Debug.Log("Velocity: " + Player.velocity.x.ToString() + ", " + Player.velocity.y.ToString() + ", " + Player.velocity.z.ToString());
        }

        public void Moved(Vector2 panAmount)
        {
            sideSpeed = panAmount.x;
            forwardSpeed = Mathf.Sign(panAmount.y) * Mathf.Pow(Mathf.Abs(panAmount.y), MovePower) * MoveSpeed;
        }

        public void Jumped()
        {
            int resultCount;
            if (jumpTimer <= 0.0f &&
                PlayerFeet != null &&
                (resultCount = Physics.OverlapBoxNonAlloc(PlayerFeet.center + PlayerFeet.transform.position, PlayerFeet.size * 0.5f, tempResults, PlayerFeet.transform.rotation, JumpMask)) > 0)
            {
                bool foundNonPlayer = false;
                for (int i = 0; i < resultCount; i++)
                {
                    if (tempResults[i].transform != Player.transform && tempResults[i].transform != PlayerFeet.transform)
                    {
                        foundNonPlayer = true;
                        break;
                    }
                }
                if (foundNonPlayer)
                {
                    jumpTimer = JumpCooldown;
                    Player.AddForce(0.0f, JumpSpeed * 100.0f, 0.0f, ForceMode.Acceleration);
                }
            }
        }
    }
}
