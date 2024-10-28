﻿using System;
using Game.Scripts.Enemy;
using Game.Scripts.Framework.Configuration.SO.Weapon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Scripts.Framework.Weapon
{
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        private Action<Projectile> callback { get; set; }
        public float speed = 100f;
        public float damage = 10f;

        private Vector3 _targetPosition;
        private bool _isMoving;
        private bool _isInitialized;

        public void Initialize(WeaponSettings weaponSettings, Action<Projectile> poolCallback)
        {
            Assert.IsNotNull(poolCallback, "ProjectilePool callback is not set");

            SetDamage(weaponSettings.projectileDamage);
            SetSpeed(weaponSettings.projectileSpeed);

            callback = poolCallback;

            _isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!_isInitialized || !_isMoving) return;
            MoveToTarget();
        }

        public void LaunchToTarget(Vector3 from, Vector3 to)
        {
            _targetPosition = to;
            transform.position = from;
            _isMoving = true;
        }

        private void MoveToTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
            transform.LookAt(_targetPosition);

            if (transform.position != _targetPosition) return;
            callback?.Invoke(this);
            _isMoving = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            _isMoving = false;

            var enemy = other.GetComponent<EnemyHolder>();
            enemy.TakeDamage(damage);
            callback.Invoke(this);
        }

        private void SetDamage(float value) => damage = value;
        private void SetSpeed(float value) => speed = value;
    }
}
