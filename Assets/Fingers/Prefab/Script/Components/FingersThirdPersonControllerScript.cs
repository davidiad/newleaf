using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    [AddComponentMenu("Fingers Gestures/Component/Third Person Controller", 5)]
    public class FingersThirdPersonControllerScript : MonoBehaviour
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

        [Tooltip("Jump speed/power")]
        [Range(0.0f, 32.0f)]
        public float JumpSpeed = 10.0f;

        [Tooltip("How often the player can jump")]
        [Range(0.0f, 3.0f)]
        public float JumpCooldown = 0.3f;

        [Tooltip("The layers the player may jump off of")]
        public LayerMask JumpMask = -1;

        private float jumpTimer;

        [Header("Camera")]
        [Tooltip("Camera z offset")]
        [Range(1.0f, 100.0f)]
        public float CameraZOffset = 10.0f;

        [Tooltip("Camera y offset")]
        [Range(0.0f, 100.0f)]
        public float CameraYOffset = 5.0f;

        [Tooltip("Zoom dampening. This causes the zoom to stop faster with higher values. Set to 0 for no zoom at all.")]
        [Range(0.0f, 100.0f)]
        public float ZoomDampening = 10.0f;

        [Tooltip("Min/max camera z distance from player")]
        public Vector2 CameraZDistanceRange = new Vector2(5.0f, 30.0f);

        private float scaleVelocity = 1.0f;

        private float? forwardSpeed;
        private float? sideSpeed;
        private readonly Collider[] tempResults = new Collider[8];

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            // face player in same direction as camera
            Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;
            cameraRotation.x = cameraRotation.z = 0.0f; // only rotate player around y axis
            Player.transform.rotation = Quaternion.Euler(cameraRotation);

            // camera zoom velocity
            if (ZoomDampening > 0.0f && Mathf.Abs(1.0f - scaleVelocity) > 0.001f)
            {
                CameraZOffset = Mathf.Clamp(CameraZOffset * scaleVelocity, CameraZDistanceRange.x, CameraZDistanceRange.y);
                scaleVelocity = Mathf.Lerp(scaleVelocity, 1.0f, Time.deltaTime * ZoomDampening);
            }

            // position camera from player
            Vector3 pos = Player.transform.position;
            pos += (Player.transform.forward * -CameraZOffset);
            pos += (Player.transform.up * CameraYOffset);
            Camera.main.transform.position = pos;
            Camera.main.transform.LookAt(Player.transform);

            // calculate new velocity
            Vector3 velRight = Vector3.zero;
            Vector3 velForward = Vector3.zero;
            Vector3 velUp = new Vector3(0.0f, Player.velocity.y, 0.0f);
            if (forwardSpeed != null)
            {
                velForward = Player.transform.forward * forwardSpeed.Value;
            }
            if (sideSpeed != null)
            {
                velRight = Player.transform.right * sideSpeed.Value;
            }
            Vector3 vel = velRight + velForward + velUp;
            Player.velocity = vel;

            // reduce jump timer
            jumpTimer -= Time.deltaTime;

            // Debug.Log("Velocity: " + Player.velocity.x.ToString() + ", " + Player.velocity.y.ToString() + ", " + Player.velocity.z.ToString());
        }

        public void Moved(Vector2 panAmount)
        {
            sideSpeed = panAmount.x * MoveSpeed;
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

        public void Scaled(GestureRecognizer scaleGesture)
        {
            if (scaleGesture.State == GestureRecognizerState.Executing)
            {
                float scaleMultiplier = (scaleGesture as ScaleGestureRecognizer).ScaleMultiplier;
                if (scaleMultiplier != 1.0f)
                {
                    scaleVelocity = scaleMultiplier;
                }
            }
        }
    }
}
