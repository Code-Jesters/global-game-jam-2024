using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using Random = UnityEngine.Random;

public class GameObserver : NetworkBehaviour
{
    // Id imagine a few of these variables need to be server side, not client side
        // I'll start burning the bridge pieces when that bridge arrives 
    // Let alone some of them need to be managed by the server
    private TimeSpan timeRemaining; // locally tracked countdown timer till loss
    public int spotsTickled;
    private List<HairManuiplation> spotsToTickle;

    private Coroutine coroutine;

    private bool tickling;
    private HashSet<string> seenNames;
    private List<ClimbingSpot> climableSpots;

    // Does not need to be network-synchronized
    public int matchTimer; // public variable to set for time until loss
    public TextMeshProUGUI timerText;
    public Color startColor;
    public Color targetColor;
    public int[] amountOfSpotsToTicklePerPhase;

    // Must be network-synchronized
    public NetworkVariable<int> phasesCompleted = new NetworkVariable<int>();

    void Start()
    {
        timeRemaining = TimeSpan.FromSeconds(matchTimer * 60);
        timerText.text = $"{timeRemaining.TotalMinutes}:00";

        spotsToTickle = new List<HairManuiplation>();
        seenNames = new HashSet<string>();
        climableSpots = new List<ClimbingSpot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) { return; } // relatively first time using networkBehaviour :| 
        
        timerText.gameObject.SetActive(true);
        if (timeRemaining.TotalSeconds > 0)
        {
            timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(Time.deltaTime));
            UpdateTimer(timeRemaining.ToString(@"mm\:ss"));
        }
        else if(timeRemaining.TotalSeconds <= 0)
        {
            Lose();
        }
        
        // debug for loss cond
        if (Input.GetKeyDown(KeyCode.F1))
        {
            timeRemaining = TimeSpan.FromSeconds(0);
            UpdateTimer(timeRemaining.ToString(@"mm\:ss"));
        }
    }

    IEnumerator LerpTickleSpotColors()
    {
        Debug.Log("starting coroutine for color lerp");
        float t = 0;
        Color minColor = startColor;
        Color maxColor = targetColor;
        
        while (true)
        {
            foreach (var t1 in spotsToTickle)
                t1.shellColor = Color.Lerp(minColor, maxColor, t);

            t += 0.3f * Time.deltaTime;

            if (t > 1)
            {
                (maxColor, minColor) = (minColor, maxColor);

                t = 0f;
            }

            yield return null;
        }
    }

    public void PickTickleSpots(List<ClimbingSpot> spots)
    {
        Debug.LogWarning("Changing Tickle Spots");
        if (phasesCompleted.Value >= amountOfSpotsToTicklePerPhase.Length)
        {
            Win();
            return;
        }

        // climableSpots.Clear();
        climableSpots.AddRange(spots); // really not needed but done for the sake of it - lazy dylan
        
        spotsToTickle.Clear();
        seenNames.Clear();
        if(coroutine != null)
            StopCoroutine(coroutine);
        
        for (int i = 0; i < amountOfSpotsToTicklePerPhase[phasesCompleted.Value]; i++)
        {
            int k = GetRandomIndex(0, spots.Count);
            if(seenNames.Add(spots[k].GetComponentInParent<HairManuiplation>().name)) // add non-duplicates
                spotsToTickle.Add(spots[k].GetComponentInParent<HairManuiplation>());
            else // iterate through duplicates until we find a unique climbing spot to tickle
            {
                k = GetRandomIndex(0, spots.Count); 
                while (!seenNames.Add(spots[k].transform.parent.parent.name))
                {
                    if (seenNames.Count == amountOfSpotsToTicklePerPhase[phasesCompleted.Value]) // chosen all the spots we are able to, so stop searching
                        break;
                    k = GetRandomIndex(0, spots.Count);
                }
                // Debug.Log(spots[k].GetComponentInParent<HairManuiplation>());
                spotsToTickle.Add(spots[k].GetComponentInParent<HairManuiplation>());
            }
        }

        coroutine = StartCoroutine(LerpTickleSpotColors());
    }

    public void Tickle(string objName)
    {
        if (tickling) return;
        tickling = true;

        foreach (var v in seenNames)
        {
            Debug.Log(v);
        }

        if (seenNames.Contains(objName))
        {
            // Debug.Log($"I tickled {objName}");
            seenNames.Remove(objName);
            for (int i = 0; i < spotsToTickle.Count; i++)
            {
                if (spotsToTickle[i].transform.name == objName)
                {
                    StartCoroutine(JoltHairColor(spotsToTickle[i]));
                    spotsToTickle.RemoveAt(i);
                    break;
                }
            }
            
            spotsTickled++;

            if (spotsTickled == amountOfSpotsToTicklePerPhase[phasesCompleted.Value])
            {
                spotsTickled = 0;
                phasesCompleted.Value++;
                PickTickleSpots(climableSpots);
            }
        }

        tickling = false;
    }

    IEnumerator JoltHairColor(HairManuiplation hair)
    {
        // jolt the hair with some damage-related color to show that you tickled the giant
        float t = 0;
        Color hairColor = hair.shellColor;
        while (t < 1)
        {
            hair.shellColor = Color.Lerp(hairColor, Color.red, Mathf.SmoothStep(0, 1, t));

            t += Time.deltaTime;
        }

        yield return new WaitForSeconds(0.5f);
        t = 0;
        hairColor = hair.shellColor;
        
        while (t < 1)
        {
            hair.shellColor = Color.Lerp(hairColor, startColor, Mathf.SmoothStep(0, 1, t));

            t += Time.deltaTime;
        }
    }

    int GetRandomIndex(int min, int max)
    {
        return Random.Range(min, max);
    }

    void UpdateTimer(string newTime)
    {
        timerText.text = newTime;
    }

    void Win()
    {
        // show you won
        // swap to win scene/UI
        
        Debug.Log("you win");
        Debug.Break();
    }

    void Lose()
    {
        // show timer has ran out
        // swap to lost scene/UI
        
        Debug.Log("you lose");
        Debug.Break();
    }
}
