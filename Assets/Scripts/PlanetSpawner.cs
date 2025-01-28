using UnityEngine;
using System.Collections;

public class PlanetSpawner : MonoBehaviour
{
    [Header("Animasyon Ayarlar�")]
    public float startHeight = 10f;      // Ba�lang�� y�ksekli�i
    public float fallDuration = 1.5f;    // D��me s�resi
    public float fallDelay = 0.2f;       // Gezegenler aras� d��me gecikmesi

    void Start()
    {
        // Sahnedeki t�m DraggableObject'leri bul
        DraggableObject[] allPlanets = FindObjectsOfType<DraggableObject>();
        StartCoroutine(DropPlanets(allPlanets));
    }

    IEnumerator DropPlanets(DraggableObject[] planets)
    {
        foreach (DraggableObject planet in planets)
        {
            // Gezegenin orijinal pozisyonunu kaydet
            Vector3 originalPos = planet.transform.position;
            // Ba�lang�� pozisyonunu ayarla (ayn� x ve z, ama y�ksekte)
            Vector3 startPos = new Vector3(originalPos.x, startHeight, originalPos.z);
            planet.transform.position = startPos;

            // D��me animasyonunu ba�lat
            StartCoroutine(FallAnimation(planet.gameObject, originalPos));

            // Sonraki gezegen i�in bekle
            yield return new WaitForSeconds(fallDelay);
        }
    }

    IEnumerator FallAnimation(GameObject planet, Vector3 targetPos)
    {
        Vector3 startPos = planet.transform.position;
        float elapsed = 0f;

        // Ba�lang��ta hafif rastgele d�n�� ekle
        Vector3 randomRotation = new Vector3(
            Random.Range(-360f, 360f),
            Random.Range(-360f, 360f),
            Random.Range(-360f, 360f)
        );

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fallDuration;

            // Pozisyon animasyonu
            float curveValue = Mathf.SmoothStep(0, 1, normalizedTime);
            planet.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);

            // D�n�� animasyonu
            planet.transform.rotation = Quaternion.Euler(
                Vector3.Lerp(randomRotation, Vector3.zero, curveValue)
            );

            // Hafif z�plama efekti
            if (normalizedTime > 0.9f)
            {
                float bounce = Mathf.Sin((normalizedTime - 0.9f) * 20f) * 0.2f * (1f - normalizedTime);
                planet.transform.position += Vector3.up * bounce;
            }

            yield return null;
        }

        // Son pozisyonu kesin olarak ayarla
        planet.transform.position = targetPos;
        planet.transform.rotation = Quaternion.identity;
    }
}