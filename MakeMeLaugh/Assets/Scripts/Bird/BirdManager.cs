using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MakeMeLaugh.Assets.Scripts.Bird;

public class BirdManager : MonoBehaviour
{

    private int _birdCount;

    [field: SerializeField]
    public int BirdCount { get; set; }
    
    public int maxBirdCount = 50;
    public float birdSpawnDistance = 4f;
    
    public Transform birdInitialTarget = null;

    public GameObject birdPrefab = null;

    public List<BirdAgent> Birds = new List<BirdAgent>();

    public void AddBird()
    {
        Vector3 defaultLocation = GetBirdSpawnLocation();
        AddBird(defaultLocation);
    }
    public void AddBird(Vector3 location)
    {
        if (BirdCount < maxBirdCount)
        {
            BirdCount += 1;
            GameObject bird = SpawnBird(location);
            BirdAgent birdAgent = bird.GetComponent<BirdAgent>();
            birdAgent.target = birdInitialTarget;
            birdAgent.SetRandomSpeedInRange();
            Birds.Add(birdAgent);
        }
    }

    public GameObject SpawnBird(Vector3 location)
    {
        if (location == null)
        {
            location = Vector3.zero;
        }
        GameObject bird = GameObject.Instantiate(birdPrefab, location, Quaternion.identity);
        bird.transform.parent = transform;
        return bird;
    }
    
    public Vector3 GetBirdSpawnLocation()
    {
        Vector3 spawnLocation = LocationRandomizer.GetLocationInProjectedSphere(birdInitialTarget.position, birdSpawnDistance);
        return spawnLocation;
    }

    public void RemoveAllBirds()
    {
        BirdCount = 0;
        Birds.Clear();
    }
}
