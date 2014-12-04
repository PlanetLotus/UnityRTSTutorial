using RTS;
using UnityEngine;

public class Harvester : Unit {
    public float Capacity;

    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);

        // Only handle input if owned by human and selected
        if (player && player.Human && currentlySelected && hoverObject.name != "Ground") {
            Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
            if (resource && !resource.IsEmpty())
                player.Hud.SetCursorState(CursorState.Harvest);
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);

        // Only handle input if owned by human
        if (player && player.Human && hitObject.name != "Ground") {
            Resource resource = hitObject.transform.parent.GetComponent<Resource>();
            if (resource && !resource.IsEmpty()) {
                if (player.SelectedObject)
                    player.SelectedObject.SetSelection(false, playingArea);
                SetSelection(true, playingArea);
                player.SelectedObject = this;
                StartHarvest(resource);
            }
        } else {
            StopHarvest();
        }
    }

	protected override void Start () {
        base.Start();
        harvestType = ResourceType.Unknown;
	}
	
	protected override void Update () {
        base.Update();	
	}

    private void StartHarvest(Resource resource) {
    }

    private void StopHarvest() {
    }

    private bool isHarvesting = false;
    private bool isEmptying = false;
    private float currentLoad = 0f;
    private ResourceType harvestType;
}
