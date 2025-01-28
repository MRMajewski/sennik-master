using UnityEngine;

public class Portal : MonoBehaviour {

    [SerializeField] 
    private Portal exitPortal;
    [SerializeField] 
    private Transform exitPosTransform;

    public Portal GetExitPortal() => exitPortal;
    public Portal SetExitPortal(Portal value) => exitPortal=value;
    public Transform GetExitPortalPosition() => exitPosTransform;

}
