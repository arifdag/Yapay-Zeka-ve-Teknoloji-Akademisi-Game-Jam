using UnityEngine;

public class EnvironmentObjectSettings : MonoBehaviour
{
    [Tooltip("X ekseni offset (Modelin yol kenarýndan uzaklaþmasý için)")]
    public float spawnOffsetX = 0f;

    [Tooltip("Y ekseni offset (Yükseklik ayarý)")]
    public float spawnOffsetY = 0f;

    [Tooltip("Z ekseni offset (Ýleri-geri kaydýrma)")]
    public float spawnOffsetZ = 0f;
}
