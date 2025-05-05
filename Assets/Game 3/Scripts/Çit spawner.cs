using System.Collections.Generic;
using UnityEngine;

public class FenceSpawnerDynamic : MonoBehaviour
{
    [Header("Çit Ayarları")]
    [Tooltip("Spawn edilecek çit prefab'ı")]
    public GameObject fencePrefab;          // Çit prefab'ınız
    [Tooltip("Toplam çit sayısı")]
    public int fenceCount = 10;               // Kaç adet çit olacak
    [Tooltip("Çitler arasındaki mesafe (Z yönünde)")]
    public float spacingZ = 5f;               // Çitlerin arasındaki mesafe
    [Tooltip("Spawn konumunun X koordinatı")]
    public float spawnX = 0f;                 // Çitlerin X pozisyonu
    [Tooltip("Spawn konumunun Y koordinatı")]
    public float spawnY = 0f;                 // Çitlerin Y pozisyonu
    [Tooltip("İlk çitin Z pozisyonu")]
    public float startZ = 0f;                 // İlk çitin Z pozisyonu

    [Header("Yenileme Ayarları")]
    [Tooltip("Karakterin Transform'u")]
    public Transform player;                // Karakterin transform'u (Inspector’dan atayın)
    [Tooltip("Karakterin arkasında kalması gereken mesafe (bu mesafeden daha geride olan çitler öne taşınır)")]
    public float repositionThreshold = 10f;   // Karakterin arkasında kalan mesafe

    // Spawn edilen çitleri saklamak için liste
    private List<GameObject> fences = new List<GameObject>();

    private void Start()
    {
        // İlk çitleri sırayla spawn ediyoruz.
        for (int i = 0; i < fenceCount; i++)
        {
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, startZ + i * spacingZ);
            // Instantiate ederken çit prefab'ını 180 derece döndürmek için Quaternion.Euler(0,180,0) kullanıyoruz.
            GameObject fence = Instantiate(fencePrefab, spawnPosition, Quaternion.Euler(0, 180, 0), transform);
            fences.Add(fence);
        }
    }

    private void Update()
    {
        // Her karede, her bir çitin pozisyonunu kontrol ediyoruz.
        foreach (GameObject fence in fences)
        {
            // Eğer çit, karakterin z pozisyonundan repositionThreshold kadar geride kalmışsa
            if (fence.transform.position.z < player.position.z - repositionThreshold)
            {
                // En önde yer alan çitin Z pozisyonunu buluyoruz.
                float maxZ = float.MinValue;
                foreach (GameObject f in fences)
                {
                    if (f.transform.position.z > maxZ)
                    {
                        maxZ = f.transform.position.z;
                    }
                }
                // Bu çiti, en öndeki çitin arkasına, spacingZ mesafede yerleştiriyoruz.
                Vector3 newPos = new Vector3(spawnX, spawnY, maxZ + spacingZ);
                fence.transform.position = newPos;
            }
        }
    }
}
