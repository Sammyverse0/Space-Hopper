using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerTransform; // Assign the player in the Inspector

    [Header("Planet Settings")]
    public GameObject[] planetPrefabs; // An array of your planet prefabs
    public int poolSize = 15; // How many planets to keep in memory
    public float planetYPosition = 0f; // The Y-level to spawn planets at

    [Header("Generation Logic")]
    public int initialPlanets = 10; // Number of planets to spawn at the start
    public float spawnDistanceAhead = 500f; // How far ahead of the player to spawn planets
    public float planetSpawnDistance = 250f; // *** NEW *** Manually set the distance between planets

    // --- Private Variables ---
    private List<GameObject> pooledPlanets;
    private float nextSpawnZ;
    private PlayerSwipeRunner playerScript;

    void Start()
    {
        // Get a reference to the player script to access its properties
        playerScript = playerTransform.GetComponent<PlayerSwipeRunner>();
        if (playerScript == null)
        {
            Debug.LogError("PlayerSwipeRunner script not found on the player!");
            return;
        }

        pooledPlanets = new List<GameObject>();

        // Create the object pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject planet = Instantiate(planetPrefabs[Random.Range(0, planetPrefabs.Length)]);
            planet.SetActive(false);
            pooledPlanets.Add(planet);
        }

        // *** CHANGED *** Start spawning from the first planet's distance, not at 0
        nextSpawnZ = planetSpawnDistance;

        // Spawn the initial set of planets
        for (int i = 0; i < initialPlanets; i++)
        {
            SpawnPlanet();
        }
    }

    void Update()
    {
        // Continuously spawn planets ahead of the player
        if (playerTransform.position.z + spawnDistanceAhead > nextSpawnZ)
        {
            SpawnPlanet();
        }

        // Recycle planets that are behind the player
        RecyclePlanets();
    }

    void SpawnPlanet()
    {
        GameObject planet = GetPooledPlanet();
        if (planet == null) return; // No available planets in the pool

        // Choose a random lane (0, 1, or 2)
        int randomLane = Random.Range(0, playerScript.laneCount);
        float laneX = (randomLane - (playerScript.laneCount / 2)) * playerScript.laneOffset;

        // Position and activate the planet
        planet.transform.position = new Vector3(laneX, planetYPosition, nextSpawnZ);
        planet.SetActive(true);

        // Update the next spawn position using the new manual distance
        nextSpawnZ += planetSpawnDistance;
    }

    void RecyclePlanets()
    {
        foreach (GameObject planet in pooledPlanets)
        {
            // If the planet is active and far behind the player
            if (planet.activeInHierarchy && planet.transform.position.z < playerTransform.position.z - 50f)
            {
                planet.SetActive(false);
            }
        }
    }

    GameObject GetPooledPlanet()
    {
        // Find an inactive planet in the pool to reuse
        foreach (GameObject planet in pooledPlanets)
        {
            if (!planet.activeInHierarchy)
            {
                return planet;
            }
        }
        // If no inactive planets are found, return null (consider increasing poolSize)
        return null;
    }
}

