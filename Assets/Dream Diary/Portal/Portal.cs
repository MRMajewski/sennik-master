using UnityEngine;

public class Portal : MonoBehaviour {
    [SerializeField] Portal exitPortal;

    public Portal GetExitPortal() => exitPortal;
}
