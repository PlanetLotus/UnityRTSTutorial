using UnityEngine;

public class Refinery : Building {
    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }

    protected override void Start() {
        base.Start();
        actions = new string[] { "Harvester" };
    }
}
