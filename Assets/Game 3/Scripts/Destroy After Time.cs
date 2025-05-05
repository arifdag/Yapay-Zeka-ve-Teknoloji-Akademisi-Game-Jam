using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 30.0f;  // 10 saniye sonra yok olsun

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
