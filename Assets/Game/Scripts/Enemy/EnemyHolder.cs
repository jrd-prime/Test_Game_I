﻿using System.Collections;
using Game.Scripts.Framework.Constants;
using UnityEngine;

namespace Game.Scripts.Enemy
{
    public class EnemyHolder : EnemyBase
    {
        private float _lastAttackTime;
        private bool _isAttacking;
        private float _fixedDT;

        private void FixedUpdate()
        {
            if (IsDead) return;

            _fixedDT = Time.fixedDeltaTime;
            TargetPosition = SettingsDto.Target.Position.CurrentValue;
            RbPosition = Rb.position;

            RotateToTarget();

            if (CloseEnoughToAttack())
            {
                if (CanAttack()) Attack();
            }
            else
            {
                EnemyAnimator.SetToRunning();
                _isAttacking = false;
                MoveToTarget();
            }
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0) return;

            var crit = Random.value > 0.5f;

            if (crit)
            {
                damage *= Random.Range(1.8f, 2f);
                damage = Mathf.RoundToInt(damage);
            }


            CurrentHealth -= damage;


            PopUpTextManager.ShowPopUpText($"{damage}", transform.position, 1f, crit);

            if (CurrentHealth > 0)
                OnTakeDamage(damage);
            else
                OnDie();
        }

        protected override void OnTakeDamage(float damage)
        {
            var hpPercent = CurrentHealth / SettingsDto.Health;

            enemyHUD.SetHp(hpPercent);
        }

        private void Attack()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            EnemyAnimator.SetToAttacking();

            OnAttack();

            if (isActiveAndEnabled) StartCoroutine(AttackAnimationDelay());
        }

        private IEnumerator AttackAnimationDelay()
        {
            yield return new WaitForSeconds(AnimConst.AttackAnimationLengthMs / 1000f);
            _isAttacking = false;
            EnemyAnimator.SetToIdle();
        }

        protected override void OnDie()
        {
            if (IsDead) return;

            IsDead = true;
            Rb.isKinematic = true;
            EnemyAnimator.SetToDeath();
            EnemiesManager.OnEnemyDiedAsync(SettingsDto.ID);
        }


        #region Movement

        private void RotateToTarget()
        {
            var directionToTarget = (TargetPosition - Rb.position).normalized;
            var lookRotation = Quaternion.LookRotation(directionToTarget);
            Rb.rotation = Quaternion.Slerp(Rb.rotation, lookRotation, SettingsDto.RotationSpeed * _fixedDT);
        }

        private void MoveToTarget() =>
            Rb.position = Vector3.MoveTowards(RbPosition, TargetPosition, SettingsDto.Speed * _fixedDT);

        #endregion

        #region Conditions

        private bool CanAttack() => !_isAttacking && Time.time - _lastAttackTime > SettingsDto.AttackDelayInSec;

        private bool CloseEnoughToAttack() =>
            Vector3.Distance(RbPosition, TargetPosition) <= SettingsDto.AttackDistance;

        #endregion
    }
}
