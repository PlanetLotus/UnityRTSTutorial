using UnityEngine;
using RTS;
using System.Collections;

public class Unit : WorldObject {
    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected) {
            if (hoverObject.name != "Ground")
                player.Hud.SetCursorState(CursorState.Move);
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected && hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
            float y = hitPoint.y + player.SelectedObject.transform.position.y;
            Vector3 destination = new Vector3(hitPoint.x, y, hitPoint.z);
            StartMove(destination);
        }
    }

    public void StartMove(Vector3 dest) {
        destination = dest;
        targetRotation = Quaternion.LookRotation(dest - transform.position);
        rotating = true;
        moving = false;
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

        // Sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing. This check fixes that.
        Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
        if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
            rotating = false;
            moving = true;
        }

        CalculateBounds();
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
}
