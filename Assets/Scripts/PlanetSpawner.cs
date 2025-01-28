using UnityEngine;
using System.Collections;

public class PlanetSpawner : MonoBehaviour
{
    [Header("Animasyon Ayarlarý")]
    public float startHeight = 10f;      // Baþlangýç yüksekliði
    public float fallDuration = 1.5f;    // Düþme süresi
    public float fallDelay = 0.2f;       // Gezegenler arasý düþme gecikmesi

    void Start()
    {
        // Sahnedeki tüm DraggableObject'leri bul
        DraggableObject[] allPlanets = FindObjectsOfType<DraggableObject>();
        StartCoroutine(DropPlanets(allPlanets));
    }

    IEnumerator DropPlanets(DraggableObject[] planets)
    {
        foreach (DraggableObject planet in planets)
        {
            // Gezegenin orijinal pozisyonunu kaydet
            Vector3 originalPos = planet.transform.position;
            // Baþlangýç pozisyonunu ayarla (ayný x ve z, ama yüksekte)
            Vector3 startPos = new Vector3(originalPos.x, startHeight, originalPos.z);
            planet.transform.position = startPos;

            // Düþme animasyonunu baþlat
            StartCoroutine(FallAnimation(planet.gameObject, originalPos));

            // Sonraki gezegen için bekle
            yield return new WaitForSeconds(fallDelay);
        }
    }

    IEnumerator FallAnimation(GameObject planet, Vector3 targetPos)
    {
        Vector3 startPos = planet.transform.position;
        float elapsed = 0f;

        // Baþlangýçta hafif rastgele dönüþ ekle
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

            // Dönüþ animasyonu
            planet.transform.rotation = Quaternion.Euler(
                Vector3.Lerp(randomRotation, Vector3.zero, curveValue)
            );

            // Hafif zýplama efekti
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