using UnityEngine;
using RTS;
using System.Collections;

public class Unit : WorldObject {
    public virtual void Init(Building creator) {
    }

    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected) {
            bool moveHover = false;
            if (hoverObject.name == "Ground") {
                moveHover = true;
            } else {
                Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
                if (resource && resource.IsEmpty())
                    moveHover = true;
            }

            if (moveHover)
                player.Hud.SetCursorState(CursorState.Move);
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected) {
            bool clickedOnEmptyResource = false;
            if (hitObject.transform.parent) {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();
                if (resource && resource.IsEmpty())
                    clickedOnEmptyResource = true;
            }

            if ((hitObject.name == "Ground" || clickedOnEmptyResource) && hitPoint != ResourceManager.InvalidPosition) {
                float y = hitPoint.y + player.SelectedObject.transform.position.y;
                Vector3 destination = new Vector3(hitPoint.x, y, hitPoint.z);
                StartMove(destination);
            }
        }
    }

    public void StartMove(Vector3 dest) {
        destination = dest;
        targetRotation = Quaternion.LookRotation(dest - transform.position);
        rotating = true;
        moving = false;
        destTarget = null;
    }

    public void StartMove(Vector3 dest, GameObject destTarget) {
        StartMove(dest);
        this.destTarget = destTarget;
    }

    protected override void Awake() {
        base.Awake();
    }
    
    protected override void Start () {
        base.Start();
    }
    
    protected override void Update () {
        base.Update();

        if (rotating)
            TurnToTarget();
        else if (moving)
            MakeMove();
    }
    
    protected override void OnGUI() {
        base.OnGUI();
    }

    private void TurnToTarget() {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
        CalculateBounds();

        // Sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing. This check fixes that.
        Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
        if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
            rotating = false;
            moving = true;

            if (destTarget)
                CalculateTargetDestination();
        }
    }

    private void CalculateTargetDestination() {
        // Calculate number of unit vectors from unit center to unit edge of bounds
        Vector3 originalExtents = selectionBounds.extents;
        Vector3 normalExtents = originalExtents;
        normalExtents.Normalize();
        float numExtents = originalExtents.x / normalExtents.x;
        int unitShift = Mathf.FloorToInt(numExtents);

        // Calculate number of unit vectors from target center to target edge of bounds
        WorldObject worldObject = destTarget.GetComponent<WorldObject>();
        if (worldObject)
            originalExtents = worldObject.SelectionBounds.extents;
        else
            originalExtents = new Vector3(0f, 0f, 0f);
        normalExtents = originalExtents;
        normalExtents.Normalize();
        numExtents = originalExtents.x / normalExtents.x;
        int targetShift = Mathf.FloorToInt(numExtents);

        // Calculate number of unit vectors between unit center and dest center with bounds just touching
        int shiftAmount = targetShift + unitShift;

        // Calculate direction unit needs to travel to reach dest in straight line and normalize to unit vector
        Vector3 origin = transform.position;
        Vector3 direction = new Vector3(destination.x - origin.x, 0f, destination.z - origin.z);
        direction.Normalize();

        // Destination = center of dest - num unit vectors calculated above
        // This gives us a dest where unit will not collide with target giving the illusion of moving to edge of target and stopping
        destination -= direction * shiftAmount;
        destination.y = destTarget.transform.position.y;
    }

    private void MakeMove() {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);

        if (transform.position == destination)
            moving = false;

        CalculateBounds();
    }

    public float moveSpeed;
    public float rotateSpeed;
    protected bool moving;
    protected bool rotating;
    private Vector3 destination;
    private Quaternion targetRotation;
    private GameObject destTarget;
}
