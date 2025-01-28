using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SkillButton : MonoBehaviour
{
    public enum SkillType
    {
        Time,
        Hint,
        Sweep,
        Magnet
    }

    public SkillType skillType;
    public float cooldownTime = 10f;
    public float hintBlinkDuration = 2f;
    public float blinkRate = 0.5f;
    public float shuffleAnimationDuration = 1f;

    private Image buttonImage;
    private Image cooldownOverlay;
    private Button button;
    private bool isOnCooldown = false;

    private static DraggableObject selectedPlanet;
    public static void SetSelectedPlanet(DraggableObject planet)
    {
        selectedPlanet = planet;
    }

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        CreateCooldownOverlay();
        button = GetComponent<Button>();
    }

    private void CreateCooldownOverlay()
    {
        GameObject overlayObj = new GameObject("CooldownOverlay");
        overlayObj.transform.SetParent(transform, false);

        RectTransform rectTransform = overlayObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;

        cooldownOverlay = overlayObj.AddComponent<Image>();
        cooldownOverlay.color = new Color(0, 0, 0, 0.5f);
        cooldownOverlay.type = Image.Type.Filled;
        cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
        cooldownOverlay.fillClockwise = true;
        cooldownOverlay.fillAmount = 0f;
    }

    public void UseSkill()
    {
        if (isOnCooldown || !ScoreManager.Instance.IsGameActive) return;

        switch (skillType)
        {
            case SkillType.Time:
                UseTimeSkill();
                StartCoroutine(StartCooldown());
                break;
            case SkillType.Hint:
                if (UseHintSkill())
                {
                    StartCoroutine(StartCooldown());
                }
                break;
            case SkillType.Sweep:
                if (UseSweepSkill())
                {
                    StartCoroutine(StartCooldown());
                }
                break;
            case SkillType.Magnet:
                if (UseMagnetSkill())
                {
                    StartCoroutine(StartCooldown());
                }
                break;
        }
    }

    private void UseTimeSkill()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddTime(5f);
            Debug.Log("5 saniye eklendi!");
        }
    }

    private bool UseHintSkill()
    {
        if (selectedPlanet == null)
        {
            Debug.Log("Önce bir gezegen seçmelisiniz!");
            return false;
        }

        var allPlanets = FindObjectsByType<DraggableObject>(FindObjectsSortMode.None);

        DraggableObject matchingPlanet = null;
        foreach (var planet in allPlanets)
        {
            if (planet != selectedPlanet && planet.planetId == selectedPlanet.planetId)
            {
                matchingPlanet = planet;
                break;
            }
        }

        if (matchingPlanet != null)
        {
            StartCoroutine(BlinkPlanet(matchingPlanet));
            return true;
        }
        else
        {
            Debug.Log("Eþleþen gezegen bulunamadý!");
            return false;
        }
    }

    private IEnumerator BlinkPlanet(DraggableObject planet)
    {
        Material planetMaterial = planet.GetComponent<Renderer>().material;
        Color originalColor = planetMaterial.color;
        Color blinkColor = Color.cyan;

        float endTime = Time.time + hintBlinkDuration;
        bool isBlinking = true;

        while (Time.time < endTime)
        {
            planetMaterial.color = isBlinking ? blinkColor : originalColor;
            isBlinking = !isBlinking;
            yield return new WaitForSeconds(blinkRate);
        }

        planetMaterial.color = originalColor;
    }

    private bool UseSweepSkill()
    {
        PlacementArea placementArea = PlacementArea.Instance;
        if (placementArea == null) return false;

        var placedObjects = placementArea.GetPlacedObjects();

        if (placedObjects == null || placedObjects.Count == 0)
        {
            Debug.Log("Süpürülecek gezegen yok!");
            return false;
        }

        var objectsToSweep = new List<DraggableObject>(placedObjects);

        foreach (DraggableObject planet in objectsToSweep)
        {
            if (planet != null)
            {
                placementArea.RemoveObject(planet);
                planet.ReturnToStartPosition();
            }
        }

        Debug.Log($"{objectsToSweep.Count} gezegen süpürüldü!");
        return true;
    }

    private bool UseMagnetSkill()
    {
        var allPlanets = FindObjectsByType<DraggableObject>(FindObjectsSortMode.None)
            .Where(p => p != null && p.gameObject.activeInHierarchy)
            .ToArray();

        if (allPlanets.Length == 0) return false;

        // Tüm gezegenleri PlacementArea'dan çýkar
        var placementArea = PlacementArea.Instance;
        if (placementArea != null)
        {
            foreach (var planet in allPlanets)
            {
                placementArea.RemoveObject(planet);
            }
        }

        StartCoroutine(ShufflePlanets(allPlanets));
        Debug.Log($"{allPlanets.Length} gezegen karýþtýrýlýyor...");
        return true;
    }

    private IEnumerator ShufflePlanets(DraggableObject[] planets)
    {
        Dictionary<DraggableObject, Vector3> startPositions = new Dictionary<DraggableObject, Vector3>();
        Dictionary<DraggableObject, Vector3> targetPositions = new Dictionary<DraggableObject, Vector3>();

        foreach (var planet in planets)
        {
            if (planet != null)
            {
                startPositions[planet] = planet.transform.position;
                targetPositions[planet] = GetRandomSafePosition(planets, planet);
            }
        }

        float elapsedTime = 0;
        while (elapsedTime < shuffleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shuffleAnimationDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            foreach (var planet in planets)
            {
                if (planet != null && startPositions.ContainsKey(planet) && targetPositions.ContainsKey(planet))
                {
                    planet.transform.position = Vector3.Lerp(
                        startPositions[planet],
                        targetPositions[planet],
                        smoothProgress
                    );
                }
            }

            yield return null;
        }

        foreach (var planet in planets)
        {
            if (planet != null && targetPositions.ContainsKey(planet))
            {
                planet.transform.position = targetPositions[planet];
            }
        }

        Debug.Log("Karýþtýrma tamamlandý!");
    }

    private Vector3 GetRandomSafePosition(DraggableObject[] planets, DraggableObject currentPlanet)
    {
        float currentY = currentPlanet.transform.position.y;

        // Oyun alaný sýnýrlarý
        float minX = -7f;
        float maxX = 7f;
        float minZ = -4f;
        float maxZ = 4f;

        const int maxAttempts = 50;
        const float minDistanceBetweenPlanets = 2f;
        const float minDistanceFromPlane = 3f;

        PlacementArea placementArea = PlacementArea.Instance;
        Vector3 planeCenter = Vector3.zero;
        if (placementArea != null && placementArea.placementPlane != null)
        {
            planeCenter = placementArea.placementPlane.position;
        }

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(minX, maxX),
                currentY,
                Random.Range(minZ, maxZ)
            );

            // Plane'den uzaklýk kontrolü
            float distanceFromPlane = Vector3.Distance(
                new Vector3(randomPos.x, 0, randomPos.z),
                new Vector3(planeCenter.x, 0, planeCenter.z)
            );

            if (distanceFromPlane < minDistanceFromPlane)
            {
                continue;
            }

            bool isPositionSafe = true;
            foreach (var otherPlanet in planets)
            {
                if (otherPlanet != null && otherPlanet != currentPlanet)
                {
                    float distance = Vector3.Distance(
                        new Vector3(randomPos.x, currentY, randomPos.z),
                        otherPlanet.transform.position
                    );
                    if (distance < minDistanceBetweenPlanets)
                    {
                        isPositionSafe = false;
                        break;
                    }
                }
            }

            if (isPositionSafe)
            {
                return randomPos;
            }
        }

        // Güvenli pozisyon bulunamazsa, mevcut pozisyondan uzaða taþý
        Vector3 fallbackPosition = currentPlanet.transform.position;
        fallbackPosition.x += Random.Range(2f, 4f) * (Random.value > 0.5f ? 1 : -1);
        fallbackPosition.z += Random.Range(2f, 4f) * (Random.value > 0.5f ? 1 : -1);

        // Sýnýrlar içinde kal
        fallbackPosition.x = Mathf.Clamp(fallbackPosition.x, minX, maxX);
        fallbackPosition.z = Mathf.Clamp(fallbackPosition.z, minZ, maxZ);

        return fallbackPosition;
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        button.interactable = false;
        float remainingTime = cooldownTime;

        while (remainingTime > 0)
        {
            cooldownOverlay.fillAmount = remainingTime / cooldownTime;
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        cooldownOverlay.fillAmount = 0f;
        button.interactable = true;
        isOnCooldown = false;
    }
}