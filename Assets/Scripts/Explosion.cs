using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float force;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        }
    }
}
