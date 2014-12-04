using RTS;
using UnityEngine;

public class Resource : WorldObject {
    public float Capacity;

    public void Remove(float amount) {
        amountLeft -= amount;
        if (amountLeft < 0)
            amountLeft = 0;
    }

    public bool IsEmpty() {
        return amountLeft <= 0;
    }

    protected override void Start() {
        base.Start();
        amountLeft = Capacity;
        resourceType = ResourceType.Unknown;
    }

    public ResourceType ResourceType { get { return resourceType; } }

    protected float amountLeft;
    protected ResourceType resourceType;
}
