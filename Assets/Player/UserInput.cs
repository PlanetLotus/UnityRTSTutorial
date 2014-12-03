using UnityEngine;
using RTS;
using System.Collections;

public class UserInput : MonoBehaviour {
	// Use this for initialization
	void Start() {
		player = transform.root.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update() {
		if (player.Human) {
			MoveCamera();
			RotateCamera();
            HandleMouseActivity();
		}
	}

	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3 (0, 0, 0);
        bool mouseScroll = false;

		// Horizontal camera movement
		if (xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
            player.Hud.SetCursorState(CursorState.PanLeft);
            mouseScroll = true;
		} else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
            player.Hud.SetCursorState(CursorState.PanRight);
            mouseScroll = true;
		}

		// Vertical camera movement
		if (ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
            player.Hud.SetCursorState(CursorState.PanDown);
            mouseScroll = true;
		} else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
            player.Hud.SetCursorState(CursorState.PanUp);
            mouseScroll = true;
		}

		// Make sure movement is in the direction the camera is pointing
		//  but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		// Away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

        // Calculate desired camera position based on received input
        Vector3 origin = Camera.main.transform.position;
        Vector3 destination = origin;
        destination.x += movement.x;
        destination.y += movement.y;
        destination.z += movement.z;

        // Limit away from ground movement to be between a minimum and maximum distance
        if (destination.y > ResourceManager.MaxCameraHeight) {
            destination.y = ResourceManager.MaxCameraHeight;
        } else if (destination.y < ResourceManager.MinCameraHeight) {
            destination.y = ResourceManager.MinCameraHeight;
        }

        // If position change is detected, update
        if (destination != origin) {
            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
        }

        if (!mouseScroll) {
            player.Hud.SetCursorState(CursorState.Select);
        }
	}

	private void RotateCamera() {
        Vector3 origin = Camera.main.transform.eulerAngles;
        Vector3 destination = origin;

        // Detect rotation amount if alt is being held and right mouse button is down
        if ((Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) && Input.GetMouseButton (1)) {
            destination.x -= Input.GetAxis ("Mouse Y") * ResourceManager.RotateAmount;
            destination.y += Input.GetAxis ("Mouse X") * ResourceManager.RotateAmount;
        }

        if (destination != origin) {
            Camera.main.transform.eulerAngles = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
        }
	}

    private void HandleMouseActivity() {
        // Is the else if intentional here? Only processes one click at a time, giving left click the priority
        if (Input.GetMouseButtonDown(0))
            LeftMouseClick();
        else if (Input.GetMouseButtonDown(1))
            RightMouseClick();

        MouseHover();
    }

    private void LeftMouseClick() {
        if (player.Hud.MouseInBounds()) {
            GameObject hitObject = FindHitObject();
            Vector3 hitPoint = FindHitPoint();

            if (hitObject && hitPoint != ResourceManager.InvalidPosition) {
                if (player.SelectedObject) {
                    player.SelectedObject.MouseClick(hitObject, hitPoint, player);
                } else if (hitObject.name != "Ground") {
                    WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
                    if (worldObject) {
                        // We already know the player has no selected object
                        player.SelectedObject = worldObject;
                        worldObject.SetSelection(true, player.Hud.GetPlayingArea());
                    }
                }
            }
        }
    }

    private void RightMouseClick() {
        if (player.Hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject) {
            player.SelectedObject.SetSelection(false, player.Hud.GetPlayingArea());
            player.SelectedObject = null;
        }
    }

    private GameObject FindHitObject() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return hit.collider.gameObject;
        return null;
    }

    private Vector3 FindHitPoint() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return hit.point;
        return ResourceManager.InvalidPosition;
    }

    private void MouseHover() {
        if (player.Hud.MouseInBounds()) {
            GameObject hoverObject = FindHitObject();

            if (hoverObject) {
                if (player.SelectedObject) {
                    player.SelectedObject.SetHoverState(hoverObject);
                } else if (hoverObject.name != "Ground") {
                    Player owner = hoverObject.transform.root.GetComponent<Player>();
                    if (owner) {
                        Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
                        Building building = hoverObject.transform.parent.GetComponent<Building>();
                        if (owner.Username == player.Username && (unit || building)) {
                            player.Hud.SetCursorState(CursorState.Select);
                        }
                    }
                }
            }
        }
    }

	private Player player;
}
