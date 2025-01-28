using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlacementArea : MonoBehaviour
{
    public static PlacementArea Instance { get; private set; }
    public Transform placementPlane;
    private List<DraggableObject> placedObjects = new List<DraggableObject>();
    private readonly int maxObjects = 2;
    private float objectHeight = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (placementPlane == null)
        {
            placementPlane = transform;
        }
    }

    public bool PlaceObject(DraggableObject obj)
    {
        if (!ScoreManager.Instance.IsGameActive) return false;

        if (placedObjects.Count >= maxObjects && !placedObjects.Contains(obj))
        {
            Debug.Log($"Maksimum obje say�s�na ula��ld� ({maxObjects}). Obje ba�lang�� pozisyonuna d�n�yor.");
            obj.ReturnToStartPosition();
            return false;
        }

        Vector3 planeCenter = placementPlane.position;
        BoxCollider planeCollider = placementPlane.GetComponent<BoxCollider>();
        float planeWidth = planeCollider.bounds.size.x;

        Vector3 placePosition;
        if (placedObjects.Count == 0)
        {
            placePosition = new Vector3(
                planeCenter.x - (planeWidth * 0.2f),
                planeCenter.y + objectHeight,
                planeCenter.z
            );
        }
        else
        {
            placePosition = new Vector3(
                planeCenter.x + (planeWidth * 0.2f),
                planeCenter.y + objectHeight,
                planeCenter.z
            );
        }

        obj.transform.position = placePosition;

        if (!placedObjects.Contains(obj))
        {
            placedObjects.Add(obj);
            obj.SetPlaced(true);
            Debug.Log($"Gezegen {obj.planetId} yerle�tirildi. Toplam: {placedObjects.Count}");

            if (placedObjects.Count == 2)
            {
                CheckForMatch();
            }
        }

        return true;
    }

    private void CheckForMatch()
    {
        if (placedObjects.Count != 2) return;

        DraggableObject obj1 = placedObjects[0];
        DraggableObject obj2 = placedObjects[1];

        Debug.Log($"E�le�me kontrol�: Gezegen1 ID: {obj1.planetId}, Gezegen2 ID: {obj2.planetId}");

        if (obj1.planetId == obj2.planetId)
        {
            Debug.Log($"E�le�me bulundu! ID: {obj1.planetId}");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(10);
                Debug.Log("10 puan eklendi!");
            }

            placedObjects.Clear();
            Destroy(obj1.gameObject);
            Destroy(obj2.gameObject);

            // E�le�me sonras� oyun biti�ini kontrol et
            ScoreManager.Instance.CheckGameCompletion();
        }
        else
        {
            Debug.Log("E�le�me bulunamad� - Gezegenler yerlerinde kal�yor");
        }
    }

    public void RemoveObject(DraggableObject obj)
    {
        if (placedObjects.Contains(obj))
        {
            placedObjects.Remove(obj);
            obj.SetPlaced(false);
            Debug.Log($"Gezegen {obj.planetId} kald�r�ld�. Kalan: {placedObjects.Count}");
        }
    }

    public List<DraggableObject> GetPlacedObjects()
    {
        return placedObjects.Where(obj => obj != null).ToList();
    }

    public bool IsOverPlacementArea(Vector3 position)
    {
        BoxCollider planeCollider = placementPlane.GetComponent<BoxCollider>();
        Bounds planeBounds = planeCollider.bounds;
        Vector3 checkPosition = new Vector3(position.x, planeBounds.center.y, position.z);
        return planeBounds.Contains(checkPosition);
    }
}