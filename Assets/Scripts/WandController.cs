using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour {

	// Public API
	public InteractableItem interactingItem;
	public float triggerAxis = 0.0f;

  private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

  private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
  private SteamVR_TrackedObject trackedObj;

  HashSet<InteractableItem> objectsHoveringOver = new HashSet<InteractableItem>();

  private InteractableItem collidedItem;
  private InteractableItem closestItem;

	private float minDistance;
	private float distance;

	// Use this for initialization
	void Start () {
  	trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

	// Update is called once per frame
	void Update () {
    if (controller == null) {
      Debug.Log("Controller not initialized");
      return;
    }

		triggerAxis = controller.GetAxis(triggerButton).x;

    if (triggerAxis >= 0.01f && interactingItem == null) {
			closestItem = null;
			minDistance = float.MaxValue;

      foreach (InteractableItem item in objectsHoveringOver) {
        distance = (item.transform.position - transform.position).sqrMagnitude;
        if (distance < minDistance) {
          minDistance = distance;
          closestItem = item;
        }
      }

      interactingItem = closestItem;

      if (interactingItem) {
        if (interactingItem.IsInteracting()) {
          interactingItem.EndInteraction(this);
        }
        interactingItem.BeginInteraction(this);
      }
    }

    if (triggerAxis < 0.01f && interactingItem != null) {
      interactingItem.EndInteraction(this);
			interactingItem = null;
    }

}

  private void OnTriggerEnter(Collider collider) {
    collidedItem = collider.GetComponent<InteractableItem>();
    if (collidedItem) {
      objectsHoveringOver.Add(collidedItem);
    }
  }

  private void OnTriggerExit(Collider collider) {
    collidedItem = collider.GetComponent<InteractableItem>();
    if (collidedItem) {
      objectsHoveringOver.Remove(collidedItem);
    }
  }
}
