using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Misc
{
    public class HoverPanToCinemachine : MonoBehaviour
    {
        public float acceleration = 4f;
        public float maxSpeed = 10f;
        public float initialSpeed = 0f;

        private float _currentSpeed;
        private Vector2 _dir;

        [SerializeField] private bool active;
        public BoxCollider2D confiner;

        private void Update()
        {
            if (!active)
            {
                _dir = Vector2.zero;
                _currentSpeed = 0;
            }
            else
            {
                UpdateDirection();
                UpdateSpeed();
            }

            transform.Translate(_dir * (_currentSpeed * Time.deltaTime), Space.World);
        }

        private void UpdateDirection()
        {
            var mouse = Mouse.current.position.ReadValue();
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            _dir = (mouse - center).normalized;

            if (_dir.sqrMagnitude < 0.01f)
                _dir = Vector2.zero;
        }

        private void UpdateSpeed()
        {
            if (_dir == Vector2.zero)
            {
                _currentSpeed = 0;
                return;
            }

            _currentSpeed += acceleration * Time.deltaTime;
            _currentSpeed = Mathf.Clamp(_currentSpeed, initialSpeed, maxSpeed);
        }

        public void SetActive(bool value)
        {
            active = value;
        }

        private void LateUpdate()
        {
            var p = transform.position;
            var min = confiner.bounds.min;
            var max = confiner.bounds.max;
            p.x = Mathf.Clamp(p.x, min.x, max.x);
            p.y = Mathf.Clamp(p.y, min.y, max.y);
            transform.position = p;
        }
    }
}