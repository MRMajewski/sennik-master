using UnityEngine;

public class Player : MonoBehaviour {
    public Collider enteredTrigger;

    [SerializeField] CharacterController characterController;

    void OnTriggerEnter(Collider collider) {
        enteredTrigger = collider;
    }

    public void Move(float diff) {
        var motion = transform.forward * diff;
        characterController.Move(motion);
    }

    public void Rotate(float diff) {
        transform.Rotate(axis: transform.up, angle: diff);
    }
}
