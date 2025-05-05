using UnityEngine;
using UnityEngine.UI;

public class CircularRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [Tooltip("Týklanabilir alaný geniþletmek için çarpan. 1 varsayýlan, 1'den büyük deðerler alaný geniþletir.")]
    public float clickableRadiusMultiplier = 1f;

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform rectTransform = transform as RectTransform;
        Vector2 localPoint;

        // Týklama noktasýný RectTransform'un lokal koordinatlarýna çeviriyoruz.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPoint);

        // Butonun yuvarlak olduðunu varsayarsak, yarýçapý RectTransform'un en küçük kenarýnýn yarýsý olarak alýyoruz.
        float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2f;

        // Çarpaný uygulayarak týklanabilir alaný geniþletiyoruz.
        radius *= clickableRadiusMultiplier;

        // Týklama noktasýnýn merkezden uzaklýðýný kontrol ediyoruz.
        return localPoint.sqrMagnitude <= radius * radius;
    }
}
