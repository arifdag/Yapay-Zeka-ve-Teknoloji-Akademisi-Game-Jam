using UnityEngine;
using UnityEngine.UI;

public class CircularRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [Tooltip("T�klanabilir alan� geni�letmek i�in �arpan. 1 varsay�lan, 1'den b�y�k de�erler alan� geni�letir.")]
    public float clickableRadiusMultiplier = 1f;

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform rectTransform = transform as RectTransform;
        Vector2 localPoint;

        // T�klama noktas�n� RectTransform'un lokal koordinatlar�na �eviriyoruz.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPoint);

        // Butonun yuvarlak oldu�unu varsayarsak, yar��ap� RectTransform'un en k���k kenar�n�n yar�s� olarak al�yoruz.
        float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2f;

        // �arpan� uygulayarak t�klanabilir alan� geni�letiyoruz.
        radius *= clickableRadiusMultiplier;

        // T�klama noktas�n�n merkezden uzakl���n� kontrol ediyoruz.
        return localPoint.sqrMagnitude <= radius * radius;
    }
}
