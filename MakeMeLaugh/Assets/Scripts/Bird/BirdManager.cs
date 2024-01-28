using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdManager : MonoBehaviour
{
    [SerializeField]
    private int maxBirdCount = 50;

    private int birdCount;

    [field: SerializeField]
    public int BirdCount
    {
        get { return _birdCount};
        
        set { _birdCount = value};
    }
    



    public decimal Price
    {
        get => _cost;
        set => _cost = value;
    }

    public List<BirdAgent> Birds = new List<BirdAgent>();

    public birdPrefab = null;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: Reference all birds in scene.
        // TODO: Add reference each time a new bird is spawned/added
    }

    public void AddBird(Vector3 location = null)
    {
        if (BirdCount < maxBirdCount)
        {
            BirdCount += 1;
            SpawnBird(location);
        }
    }

    public void SpawnBird()
    {
        if (location == null)
        {
            location = Vector3.zero;
        }
        GameObject.Instantiate(birdPrefab, location, Quaternion.identity);
    }

    public void RemoveAllBirds()
    {
        BirdCount = 0;
        Birds.Clear();
    }
}
