using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("Distance of probe from the player to check for interactable objects.")]
    [SerializeField] private float probeDistance = 1f;

    [Tooltip("Radius of the probe to check for interactable objects.")]
    [SerializeField] private float probeRadius = 0.3f;

    [SerializeField] private LayerMask interactableLayer;

    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnInteract()
    {
        FindInteractable()?.Interact(gameObject);
    }

    private Vector2 GetProbeCenter(Vector2 facing)
    {
        return (Vector2)transform.position + facing * probeDistance;
    }

    private IInteractable FindInteractable()
    {
        Vector2 probeCenter = GetProbeCenter(player.Facing);
        Collider2D hit = Physics2D.OverlapCircle(probeCenter, probeRadius, interactableLayer);

        if (hit != null && hit.TryGetComponent(out IInteractable interactable))
            return interactable;
        
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 facing = player != null ? player.Facing : Vector2.right;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetProbeCenter(facing), probeRadius);
    }
}
