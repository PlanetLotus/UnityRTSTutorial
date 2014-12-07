using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
    public float Velocity = 1f;
    public int Damage = 1;

    public void SetRange(float range) {
        this.range = range;
    }

    public void SetTarget(WorldObject target) {
        this.target = target;
    }

    void Update() {
        if (HitSomething()) {
            InflictDamage();
            Destroy(gameObject);
        }

        if (range > 0) {
            float positionChange = Time.deltaTime * Velocity;
            range -= positionChange;
            transform.position += positionChange * transform.forward;
        } else {
            Destroy(gameObject);
        }
    }

    private bool HitSomething() {
        if (target && target.SelectionBounds.Contains(transform.position))
            return true;
        return false;
    }

    private void InflictDamage() {
        if (target)
            target.TakeDamage(Damage);
    }

    private float range = 1f;
    private WorldObject target;
}
