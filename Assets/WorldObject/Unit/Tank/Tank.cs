﻿using UnityEngine;
using System.Collections;
using RTS;

public class Tank : Unit {
    public override bool CanAttack() {
        return true;
    }

    protected override void UseWeapon() {
        base.UseWeapon();
        Vector3 spawnPoint = transform.position;
        spawnPoint.x += 2.1f * transform.forward.x;
        spawnPoint.y += 1.4f;
        spawnPoint.z += 2.1f * transform.forward.z;

        GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("TankProjectile"), spawnPoint, transform.rotation);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
        projectile.SetRange(0.9f * WeaponRange);
        projectile.SetTarget(target);
    }

    protected override void AimAtTarget() {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    }

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();

        if (isAiming) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, WeaponAimSpeed);
            CalculateBounds();
            Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);

            if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
                isAiming = false;
            }
        }
    }

    private Quaternion aimRotation;
}
