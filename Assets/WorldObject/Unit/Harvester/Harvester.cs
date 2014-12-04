using RTS;
using UnityEngine;

public class Harvester : Unit {
    public float Capacity;
    public Building ResourceStore;
    public float collectionAmount;
    public float depositAmount;

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

    protected override void Start() {
        base.Start();
        harvestType = ResourceType.Unknown;
    }

    protected override void Update() {
        base.Update();

        if (!rotating && !moving && (isHarvesting || isEmptying)) {
            Arms[] arms = GetComponentsInChildren<Arms>();
            ShowArms(arms);

            if (isHarvesting) {
                Collect();
                if (currentLoad >= Capacity || resourceDeposit.IsEmpty()) {
                    currentLoad = Mathf.Floor(currentLoad);
                    isHarvesting = false;
                    isEmptying = true;
                    HideArms(arms);

                    StartMove(ResourceStore.transform.position, ResourceStore.gameObject);
                }
            } else {
                Deposit();
                if (currentLoad <= 0) {
                    isEmptying = false;
                    HideArms(arms);

                    if (!resourceDeposit.IsEmpty()) {
                        isHarvesting = true;
                        StartMove(resourceDeposit.transform.position, resourceDeposit.gameObject);
                    }
                }
            }
        }
    }

    private void Collect() {
        // Should really round to int HERE rather than later to make it IMPOSSIBLE to get a float back
        // For now, just following the tutorial
        float collect = collectionAmount * Time.deltaTime;

        // Make sure harvest can't collect more than it can carry
        if (currentLoad + collect > Capacity)
            collect = Capacity - currentLoad;

        resourceDeposit.Remove(collect);
        currentLoad += collect;
    }

    private void Deposit() {
        currentDeposit += depositAmount * Time.deltaTime;
        int deposit = Mathf.FloorToInt(currentDeposit);

        if (deposit < 1)
            return;

        // This should be impossible!
        if (deposit > currentLoad)
            deposit = Mathf.FloorToInt(currentLoad);

        currentDeposit -= deposit;
        currentLoad -= deposit;
        ResourceType depositType = harvestType;
        if (harvestType == ResourceType.Ore)
            depositType = ResourceType.Money;
        player.AddResource(depositType, deposit);
    }

    private void ShowArms(Arms[] arms) {
        foreach (Arms arm in arms)
            arm.renderer.enabled = true;
    }

    private void HideArms(Arms[] arms) {
        foreach (Arms arm in arms)
            arm.renderer.enabled = false;
    }

    private void StartHarvest(Resource resource) {
        resourceDeposit = resource;
        StartMove(resource.transform.position, resource.gameObject);

        // We can only collect one resource at a time. Other resources are lost.
        if (harvestType == ResourceType.Unknown || harvestType != resource.ResourceType) {
            harvestType = resource.ResourceType;
            currentLoad = 0f;
        }

        isHarvesting = true;
        isEmptying = false;
    }

    private void StopHarvest() {
    }

    private bool isHarvesting = false;
    private bool isEmptying = false;
    private float currentLoad = 0f;
    private ResourceType harvestType;
    private Resource resourceDeposit;
    private float currentDeposit = 0f;
}
