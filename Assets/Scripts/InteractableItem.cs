using UnityEngine;
using System.Collections;

public class InteractableItem : MonoBehaviour {

	public Rigidbody rigidbody;

  private bool currentlyInteracting;

  private float velocityFactor = 30000f;
  private Vector3 posDelta;

  private float rotationFactor = 30000f;
  private Quaternion rotationDelta;
  private float angle;
  private Vector3 axis;

	private float triggerStrength;
	private float triggerAxis;

  private WandController attachedWand;

  private Transform interactionPoint;

	// Use this for initialization
	void Start () {
    rigidbody = GetComponent<Rigidbody>();
    interactionPoint = new GameObject().transform;
    velocityFactor /= rigidbody.mass;
    rotationFactor /= rigidbody.mass;
	}

	// Update is called once per frame
	void FixedUpdate() {
    if (attachedWand && currentlyInteracting) {
        posDelta = attachedWand.transform.position - interactionPoint.position;

				if (triggerAxis - attachedWand.triggerAxis > 0.2) {

				} else {

					triggerStrength = Mathf.Pow(attachedWand.triggerAxis, 3.0f) * 10.0f;
					triggerAxis = attachedWand.triggerAxis;

					this.rigidbody.velocity = posDelta * Mathf.Min(velocityFactor * Time.fixedDeltaTime * triggerStrength, 1.0f / Time.fixedDeltaTime);

					rotationDelta = attachedWand.transform.rotation * Quaternion.Inverse(interactionPoint.rotation);
					rotationDelta.ToAngleAxis(out angle, out axis);

					if (angle > 180) {
						angle -= 360;
					}

					this.rigidbody.angularVelocity = Mathf.Min(rotationFactor * Time.fixedDeltaTime * triggerStrength, 1.0f / Time.fixedDeltaTime) * angle * Mathf.Deg2Rad * axis;
				}
			}
	}

    public void BeginInteraction(WandController wand) {
      attachedWand = wand;
      interactionPoint.position = wand.transform.position;
      interactionPoint.rotation = wand.transform.rotation;
      interactionPoint.SetParent(transform, true);

      currentlyInteracting = true;
    }

    public void EndInteraction(WandController wand) {
      if (wand == attachedWand) {
        attachedWand = null;
        currentlyInteracting = false;
      }
    }

    public bool IsInteracting() {
      return currentlyInteracting;
    }
}
