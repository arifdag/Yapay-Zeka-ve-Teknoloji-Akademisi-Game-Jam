using UnityEngine;

public class EnvironmentObjectSettings : MonoBehaviour
{
    [Tooltip("X ekseni offset (Modelin yol kenar�ndan uzakla�mas� i�in)")]
    public float spawnOffsetX = 0f;

    [Tooltip("Y ekseni offset (Y�kseklik ayar�)")]
    public float spawnOffsetY = 0f;

    [Tooltip("Z ekseni offset (�leri-geri kayd�rma)")]
    public float spawnOffsetZ = 0f;
}
