using UnityEngine;
using System.Collections.Generic;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private ObjectPool pillarPool;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject initialPillar;
    [SerializeField] private GameObject nextPillar;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float minHeight = -2f;
    [SerializeField] private float maxHeight = 2f;
    [SerializeField] private BridgeManager bridgeManager;
    [SerializeField] private float pillarHeightOffset = 2f;
    [SerializeField] private ObjectPool powerUpPool;
    [SerializeField] private GameObject speedPowerUpPrefab;
    [SerializeField] private GameObject invincibilityPowerUpPrefab;
    [SerializeField] private int maxPowerUps = 1;

    private Vector3 lastPillarPosition;
    private GameObject currentPillar;
    private GameObject previousPillar;
    private bool hasStartedSpawning;
    private List<GameObject> activePowerUps = new List<GameObject>();

    void Start()
    {
        if (player == null || pillarPool == null || initialPillar == null || nextPillar == null || bridgeManager == null || powerUpPool == null || speedPowerUpPrefab == null || invincibilityPowerUpPrefab == null)
        {
            Debug.LogError("Missing references in PillarManager!");
            return;
        }
        currentPillar = initialPillar;
        previousPillar = null;
        lastPillarPosition = initialPillar.transform.position;
        hasStartedSpawning = false;
        Debug.Log("PillarManager initialized | PowerUp Pool: " + (powerUpPool != null));
    }

    void Update()
    {
        if (currentPillar == null) return;

        Vector3 pillarTopPosition = currentPillar.transform.position + Vector3.up * pillarHeightOffset;
        float distanceToPillar = Vector3.Distance(player.position, pillarTopPosition);

        if (distanceToPillar < 1f)
        {
            if (currentPillar == initialPillar && !hasStartedSpawning)
            {
                Debug.Log("Success! Player reached initial pillar at " + pillarTopPosition);
                previousPillar = currentPillar;
                currentPillar = nextPillar;
                SpawnPowerUps(currentPillar); // Spawn PowerUp ngay từ nextPillar
                Debug.Log("Reached initial pillar, moving to next pillar.");
            }
            else if (hasStartedSpawning)
            {
                Debug.Log("Success! Player reached pillar at " + pillarTopPosition);
                TransitionToNewPillar();
            }
            else if (currentPillar == nextPillar)
            {
                Debug.Log("Success! Player reached next pillar at " + pillarTopPosition);
                TransitionToNewPillar();
                hasStartedSpawning = true;
            }
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
        SpawnPowerUps(currentPillar);
        Debug.Log("Spawned new random pillar at " + lastPillarPosition);
    }

    private void SpawnNextPillar()
    {
        float randomDistance = Random.Range(minDistance, maxDistance);
        float randomHeight = Random.Range(minHeight, maxHeight);
        Vector3 direction = Random.onUnitSphere;
        direction.y = 0;
        direction.Normalize();

        Vector3 newPosition = lastPillarPosition + (direction * randomDistance);
        newPosition.y = lastPillarPosition.y + randomHeight;
        lastPillarPosition = newPosition;
    }

    private void SpawnPowerUps(GameObject pillar)
    {
        Vector3 pillarTop = pillar.transform.position + Vector3.up * pillarHeightOffset;

        int powerUpCount = Random.Range(1, maxPowerUps + 1);
        Debug.Log("Spawning " + powerUpCount + " PowerUps on pillar: " + pillar.name);

        for (int i = 0; i < powerUpCount; i++)
        {
            bool isSpeedPowerUp = Random.value > 0.5f;
            GameObject powerUpPrefab = isSpeedPowerUp ? speedPowerUpPrefab : invincibilityPowerUpPrefab;

            Vector3 spawnPosition = pillarTop + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
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