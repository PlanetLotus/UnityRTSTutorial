using UnityEngine;
using System.Collections;

public class WarFactory : Building {
    public override void PerformAction(string action) {
        base.PerformAction(action);
        CreateUnit(action);
    }

    protected override void Start() {
        base.Start();
        actions = new string[] {
            "Tank"
        };
    }
}
