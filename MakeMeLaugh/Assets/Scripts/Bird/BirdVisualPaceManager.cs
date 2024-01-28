using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdVisualPaceManager : MonoBehaviour
{
    // NOTE: Float from 0.0 to 1.0 depending on how progressed the game is.
    [SerializeField]
    private int startingBirdCount = 0;

    [SerializeField]
    private int finalMaxBirdCount = 50;
    
    [SerializeField]
    private float currentProgress;

    public BirdManager birdManager;
    
    public List<BirdAgent> Birds = new List<BirdAgent>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < startingBirdCount; i++)
        {
            birdManager.AddBird();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CheckBirdCount()
    {
        int birdCountDelta = finalMaxBirdCount - startingBirdCount; 
        float expectedBirdCount = startingBirdCount + birdCountDelta * currentProgress;
        if (birdManager.)
    }

    public void ResetProgress()
    {
        currentProgress = 0;
    }

    public void UpdateBirdProgression(float progress)
    {
        if (progress > currentProgress)
        {
            currentProgress = progress;
            CheckBirdCount();
        }
    }
}
