using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public int planetId;
    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isPlaced = false;
    private PlacementArea currentPlacementArea;
    private Camera mainCamera;
    private Rigidbody rb;

    // Sürükleme için yeni deðiþkenler
    private float dragSpeed = 15f;
    private float yPosition = 0.5f;
    private Vector3 targetPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        targetPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (!ScoreManager.Instance.IsGameActive) return;

        SkillButton.SetSelectedPlanet(this);
        isDragging = true;

        if (isPlaced && currentPlacementArea != null)
        {
            currentPlacementArea.RemoveObject(this);
            isPlaced = false;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || !ScoreManager.Instance.IsGameActive) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, yPosition, 0));

        if (dragPlane.Raycast(ray, out float distance))
        {
            targetPosition = ray.GetPoint(distance);
            targetPosition.y = yPosition;
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            // Yumuþak hareket
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * dragSpeed
            );
        }
    }

    private void OnMouseUp()
    {
        if (!ScoreManager.Instance.IsGameActive) return;

        isDragging = false;
        if (currentPlacementArea != null)
        {
            bool placed = currentPlacementArea.PlaceObject(this);
            if (!placed)
            {
                StartCoroutine(SmoothReturnToStart());
            }
        }
        else
        {
            StartCoroutine(SmoothReturnToStart());
        }
    }

    private System.Collections.IEnumerator SmoothReturnToStart()
    {
        float elapsedTime = 0;
        float returnTime = 0.3f;
        Vector3 currentPos = transform.position;

        while (elapsedTime < returnTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnTime;
            transform.position = Vector3.Lerp(currentPos, startPosition, t);
            yield return null;
        }

        transform.position = startPosition;
        isPlaced = false;
        Debug.Log($"Gezegen {planetId} baþlangýç pozisyonuna döndü");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlaced && other.gameObject.GetComponent<PlacementArea>() != null)
        {
            currentPlacementArea = other.gameObject.GetComponent<PlacementArea>();
            Debug.Log($"Gezegen {planetId} placement area'ya girdi");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlacementArea>() != null)
        {
            currentPlacementArea = null;
            Debug.Log($"Gezegen {planetId} placement area'dan çýktý");
        }
    }

    public void ReturnToStartPosition()
    {
        StartCoroutine(SmoothReturnToStart());
    }

    public void SetPlaced(bool value)
    {
        isPlaced = value;
        Debug.Log($"Gezegen {planetId} placed durumu: {value}");
    }
}