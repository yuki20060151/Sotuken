using UnityEngine;

public class Explosion : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(Vector3.up * 20, ForceMode.Impulse);
        }
    }
}
