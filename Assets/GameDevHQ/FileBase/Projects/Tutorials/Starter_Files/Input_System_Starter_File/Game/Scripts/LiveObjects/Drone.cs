using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        private PlayerInputMap _input;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;

            //New Input System below:
            _input = new PlayerInputMap();
            _input.Drone.Enable();
            _input.Drone.Escape.performed += Escape_performed;
        }

        //New Input system
        private void Escape_performed(InputAction.CallbackContext obj)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

                //Old Input system
                //if (Input.GetKeyDown(KeyCode.Escape))
                //{
                //    _inFlightMode = false;
                //    onExitFlightmode?.Invoke();
                //    ExitFlightMode();
                //}
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            //Here is old input system:  
            //  if (Input.GetKey(KeyCode.LeftArrow))
            // {
            //      var tempRot = transform.localRotation.eulerAngles;
            //      tempRot.y -= _speed / 3;
            //      transform.localRotation = Quaternion.Euler(tempRot);
            //  }
            //  if (Input.GetKey(KeyCode.RightArrow))
            //  {
            //      var tempRot = transform.localRotation.eulerAngles;
            //      tempRot.y += _speed / 3;
            //      transform.localRotation = Quaternion.Euler(tempRot);
            //  }

            //Here is new Input system:
            var horizontalInput = _input.Drone.Arrows.ReadValue<float>();
            var tempRot = transform.localRotation.eulerAngles;
            tempRot.y += horizontalInput*_speed / 3;
            transform.localRotation = Quaternion.Euler(tempRot);

        }

        private void CalculateMovementFixedUpdate()
        {
            // Old Input system
            //if (Input.GetKey(KeyCode.Space))
            //{
            //    _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            //}
            //if (Input.GetKey(KeyCode.V))
            //{
            //    _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            //}

            //New Input System:
            var verticalInput = _input.Drone.Thrusts.ReadValue<float>();
            _rigidbody.AddForce(verticalInput * transform.up * _speed, ForceMode.Acceleration);
        }

        private void CalculateTilt()
        {
            //if (Input.GetKey(KeyCode.A)) 
            //    transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            //else if (Input.GetKey(KeyCode.D))
            //    transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            //else if (Input.GetKey(KeyCode.W))
            //    transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            //else if (Input.GetKey(KeyCode.S))
            //    transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            //else 
            //    transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);

            // New input system
            var tilt = _input.Drone.WASD.ReadValue<Vector2>();

            float targetX = 0f;
            float targetZ = 0f;

            // Vertical keys (W/S) → X rotation
            if (tilt.y > 0) targetX = 30f;
            else if (tilt.y < 0) targetX = -30f;

            // Horizontal keys (A/D) → Z rotation
            if (tilt.x > 0) targetZ = -30f;  // D key = tilt.x > 0 → Z = -30
            else if (tilt.x < 0) targetZ = 30f; // A key = tilt.x < 0 → Z = 30

            transform.rotation = Quaternion.Euler(targetX, transform.localRotation.eulerAngles.y, targetZ);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            _input.Drone.Escape.performed -= Escape_performed;
            _input.Drone.Disable();
        }
    }
}
