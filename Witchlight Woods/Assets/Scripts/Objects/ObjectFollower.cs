using System;
using UnityEngine;

namespace WitchlightWoods.Objects
{
    public class ObjectFollower : MonoBehaviour
    {
        public Transform target;
        private bool _useRigidbody;
        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _useRigidbody = TryGetComponent(out _rigidbody2D);
        }

        private void Update()
        {
            if(!_useRigidbody)
                transform.position = target.position;
        }

        private void FixedUpdate()
        {
            if(_useRigidbody)
                _rigidbody2D.MovePosition(target.position);
        }
    }
}