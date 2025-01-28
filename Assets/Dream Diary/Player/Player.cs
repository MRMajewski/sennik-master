using UnityEngine;

public class Player : MonoBehaviour {
    public Collider enteredTrigger;

    [SerializeField] CharacterController characterController;

    public Animator animator;

    void OnTriggerEnter(Collider collider) {
        enteredTrigger = collider;
    }

    public void Move(float diff) {
        var motion = transform.forward * diff;
        characterController.Move(motion);

        if (animator.GetBool("IsMoving"))
            return;
        animator.SetBool("IsMoving", true);
    }

    public void Stop() {

        if (!animator.GetBool("IsMoving"))
            return;
        animator.SetBool("IsMoving", false);
    }
    public void Rotate(float diff) {
        transform.Rotate(axis: transform.up, angle: diff);
    }
}
