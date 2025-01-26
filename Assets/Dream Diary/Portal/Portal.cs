using UnityEngine;

public class Portal : MonoBehaviour {
    [SerializeField] Portal exitPortal;

    public Portal GetExitPortal() => exitPortal;

    public Portal SetExitPortal(Portal value) => exitPortal=value;
}
