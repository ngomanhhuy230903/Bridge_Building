using UnityEngine;
using System.Collections.Generic;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private ObjectPool pillarPool;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject initialPillar;
    [SerializeField] private GameObject nextPillar;
    [SerializeField] private float distance = 0.8f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float minHeight = -2f;
    [SerializeField] private float maxHeight = 2f;
    [SerializeField] private BridgeManager bridgeManager;
    [SerializeField] private float pillarHeightOffset = 2f;
    [SerializeField] private ObjectPool powerUpPool;
    [SerializeField] private GameObject speedPowerUpPrefab;
    [SerializeField] private GameObject invincibilityPowerUpPrefab;
    [SerializeField] private GameObject healthPowerUpPrefab;
    [SerializeField] private int maxPowerUps = 1;
    [SerializeField] private float pillarMoveSpeed = 2f; 
    [SerializeField] private float pillarVerticalSpeed = 1f;
    [SerializeField] private float pillarMoveDistance = 2.5f; 
    [SerializeField] private float pillarVerticalDistance = 0.9f; 

    private Vector3 lastPillarPosition;
    private GameObject currentPillar;
    private GameObject previousPillar;
    private bool hasStartedSpawning;
    private List<GameObject> activePowerUps = new List<GameObject>();
    private Vector3 pillarSpawnPosition; // Vị trí spawn ban đầu của trụ hiện tại
    private bool isPillarMoving = true; // Trụ có đang di chuyển không

    void Start()
    {
        if (player == null || pillarPool == null || initialPillar == null || nextPillar == null ||
            bridgeManager == null || powerUpPool == null || speedPowerUpPrefab == null ||
            invincibilityPowerUpPrefab == null || healthPowerUpPrefab == null)
        {
            Debug.LogError("Missing references in PillarManager!");
            return;
        }
        currentPillar = initialPillar;
        previousPillar = null;
        lastPillarPosition = initialPillar.transform.position;
        pillarSpawnPosition = lastPillarPosition; // Lưu vị trí spawn ban đầu
        hasStartedSpawning = false;
        Debug.Log("PillarManager initialized | PowerUp Pool: " + (powerUpPool != null));
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;
        if (currentPillar == null) return;

        Vector3 pillarTopPosition = currentPillar.transform.position + Vector3.up * pillarHeightOffset;
        float distanceToPillar = Vector3.Distance(player.position, pillarTopPosition);

        if (distanceToPillar < distance)
        {
            if (currentPillar == initialPillar && !hasStartedSpawning)
            {
                Debug.Log("Success! Player reached initial pillar at " + pillarTopPosition);
                previousPillar = currentPillar;
                currentPillar = nextPillar;
                pillarSpawnPosition = currentPillar.transform.position; // Cập nhật vị trí spawn
                SpawnPowerUps(currentPillar);
                Debug.Log("Reached initial pillar, moving to next pillar.");
            }
            else if (hasStartedSpawning)
            {
                Debug.Log("Success! Player reached pillar at " + pillarTopPosition);
                TransitionToNewPillar();
                GameManager.Instance.IncreaseScore();
            }
            else if (currentPillar == nextPillar)
            {
                Debug.Log("Success! Player reached next pillar at " + pillarTopPosition);
                TransitionToNewPillar();
                hasStartedSpawning = true;
                GameManager.Instance.IncreaseScore();
            }
        }

        // Di chuyển trụ nếu đang trong trạng thái di chuyển
        if (isPillarMoving && currentPillar != null)
        {
            MovePillar();
        }
    }

    private void TransitionToNewPillar()
    {
        if (previousPillar != null)
        {
            previousPillar.SetActive(false);
            Debug.Log("Deactivated previous pillar: " + previousPillar.name);
        }
        bridgeManager.DeactivateCurrentBridge();

        SpawnNextPillar();
        previousPillar = currentPillar;
        currentPillar = pillarPool.GetObject(lastPillarPosition, Quaternion.identity);
        pillarSpawnPosition = lastPillarPosition; // Lưu vị trí spawn của trụ mới
        isPillarMoving = true; // Bật lại trạng thái di chuyển
        SpawnPowerUps(currentPillar);
        Debug.Log("Spawned new random pillar at " + lastPillarPosition);
    }

    private void SpawnNextPillar()
    {
        float randomDistance = Random.Range(minDistance, maxDistance);
        float randomHeight = Random.Range(minHeight, maxHeight);

        // Dùng hướng cố định (ví dụ: trục X) thay vì random hướng
        Vector3 direction = Vector3.forward; // Hướng cố định +Z, có thể đổi thành Vector3.right (+X) nếu muốn

        // Tính vị trí mới chỉ dựa trên hướng cố định
        Vector3 newPosition = lastPillarPosition + (direction * randomDistance);
        newPosition.y = lastPillarPosition.y + randomHeight;

        // Cập nhật vị trí trụ
        lastPillarPosition = newPosition;

        // Log để kiểm tra
        Debug.Log("New pillar position: " + newPosition + " | Distance: " + randomDistance);
    }

    private void SpawnPowerUps(GameObject pillar)
    {
        Vector3 pillarTop = pillar.transform.position + Vector3.up * pillarHeightOffset;

        int powerUpCount = Random.Range(1, maxPowerUps + 1);
        Debug.Log("Spawning " + powerUpCount + " PowerUps on pillar: " + pillar.name);

        for (int i = 0; i < powerUpCount; i++)
        {
            float randomValue = Random.value;
            GameObject powerUpPrefab;
            if (randomValue < 0.33f)
                powerUpPrefab = healthPowerUpPrefab;
            else if (randomValue < 0.66f)
                powerUpPrefab = invincibilityPowerUpPrefab;
            else
                powerUpPrefab = speedPowerUpPrefab;

            Vector3 spawnPosition = pillarTop + new Vector3(Random.Range(-0.5f, 0.5f), -1f, Random.Range(-0.5f, 0.5f));
            GameObject powerUp = powerUpPool.GetObject(spawnPosition, Quaternion.identity);
            if (powerUp != null)
            {
                powerUp.transform.parent = pillar.transform;
                activePowerUps.Add(powerUp);
                Debug.Log("Spawned PowerUp " + powerUp.name + " at: " + spawnPosition);
            }
            else
            {
                Debug.LogError("Failed to get PowerUp from pool!");
            }
        }
    }
    private void MovePillar()
    {
        if (currentPillar == null) return;

        float xOffset = Mathf.Sin(Time.time * pillarMoveSpeed) * pillarMoveDistance;
        float yOffset = Mathf.Sin(Time.time * pillarVerticalSpeed) * pillarVerticalDistance;

        Vector3 newPosition = pillarSpawnPosition + new Vector3(xOffset, yOffset, 0f);
        currentPillar.transform.position = newPosition;

        Debug.Log("Pillar moving | Position: " + newPosition);
    }

    // Gọi từ BridgeManager để dừng trụ khi bắt đầu dựng cầu
    public void StopPillarMovement()
    {
        isPillarMoving = false;
        Debug.Log("Pillar movement stopped at: " + currentPillar.transform.position);
    }

    public void DeactivatePowerUps()
    {
        foreach (GameObject powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                powerUp.SetActive(false);
                Debug.Log("Deactivated PowerUp: " + powerUp.name);
            }
        }
        activePowerUps.Clear();
    }

    public GameObject GetNextPillar()
    {
        if (currentPillar == initialPillar)
        {
            return nextPillar;
        }
        foreach (Transform child in pillarPool.transform)
        {
            if (child.gameObject.activeInHierarchy && child.gameObject != currentPillar)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}