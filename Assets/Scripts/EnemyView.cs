using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] float lookDis=10f;
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, lookDis, layerMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Target"))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log("Player seen!");
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * lookDis, Color.white);
        }
    }

}
