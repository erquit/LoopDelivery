using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionRange = 0.5f;
    [SerializeField] private LayerMask interactableLayer;
    
    private void OnInteract()
    {
        Vector2 origin = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, interactionRange, interactableLayer);

        IInteractable nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent(out IInteractable candidate)) continue;

            float sqrDistance = (origin - hit.ClosestPoint(origin)).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = candidate;
            }
        }

        nearest?.Interact(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
