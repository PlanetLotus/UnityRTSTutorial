using UnityEngine;

public class Worker : Unit {
    public int BuildSpeed;

    public override void SetBuilding(Building project) {
        base.SetBuilding(project);
        currentProject = project;
        StartMove(currentProject.transform.position, currentProject.gameObject);
        isBuilding = true;
    }

    public override void PerformAction(string action) {
        base.PerformAction(action);
        CreateBuilding(action);
    }

    public override void StartMove(Vector3 destination) {
        base.StartMove(destination);
        amountBuilt = 0f;
        isBuilding = false;
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        bool doBase = true;

        if (player && player.Human && currentlySelected && hitObject && hitObject.name != "Ground") {
            Building building = hitObject.transform.parent.GetComponent<Building>();
            if (building && building.UnderConstruction()) {
                SetBuilding(building);
                doBase = false;
            }
        }

        if (doBase)
            base.MouseClick(hitObject, hitPoint, controller);
    }

    protected override void Start() {
        base.Start();
        actions = new string[] { "Refinery", "WarFactory" };
    }

    protected override void Update() {
        base.Update();

        if (!moving && !rotating && isBuilding && currentProject && currentProject.UnderConstruction()) {
            amountBuilt += BuildSpeed * Time.deltaTime;
            int amount = Mathf.FloorToInt(amountBuilt);

            if (amount > 0) {
                amountBuilt -= amount;
                currentProject.Construct(amount);
                if (!currentProject.UnderConstruction())
                    isBuilding = false;
            }
        }
    }

    private void CreateBuilding(string buildingName) {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        if (player)
            player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    }

    private Building currentProject;
    private bool isBuilding = false;
    private float amountBuilt = 0f;
}
