using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class GameObserver : NetworkBehaviour
{
    public enum GameState
    {
        kNotStarted = 0,
        kStarted = 1,
        kWon = 2,
        kLost = 3
    }

    public static GameObserver Instance;

    // Id imagine a few of these variables need to be server side, not client side
        // I'll start burning the bridge pieces when that bridge arrives 
    // Let alone some of them need to be managed by the server
    private TimeSpan timeRemaining; // locally tracked countdown timer till loss
    
    public int spotsTickled;
    private List<HairManuiplation> spotsToTickle = new List<HairManuiplation>();

    private Coroutine coroutine;

    private HashSet<string> spotsStillToTickle = new HashSet<string>();
    private List<int> climableSpotIndices = new List<int>(); // this one is complicated to deal with

    // Does not need to be network-synchronized
    public int matchTimer; // public variable to set for time until loss
    public TextMeshProUGUI timerText;
    public Color startColor;
    public Color targetColor;
    public int[] amountOfSpotsToTicklePerPhase;
    private bool tickling; // only used server-side
    public TextMeshProUGUI win_loss_message;
    int lastObservedGameState = (int)GameState.kNotStarted;
    public List<ClimbingSpot> climbingSpots = new List<ClimbingSpot>(); // canonical ordering of all climbing spots
    private List<HairManuiplation> allHairManipsOrdered = new List<HairManuiplation>(); // canonical ordering of all hair manipulations
    List<int> lastObservedHairManipIndicesToTickle = new List<int>();

    // Must be network-synchronized
    public NetworkVariable<int> phasesCompleted = new NetworkVariable<int>();
    public NetworkVariable<int> currentGameState = new NetworkVariable<int>();
    private NetworkList<int> hairManipIndicesToTickle = new NetworkList<int>(); // index into allHairManipsOrdered

    // code from here to the next section is logic we're still picking apart network-wise //////////

    void PickTickleSpots(List<int> spotIndices)
    {
        Debug.Log("GameObserver.OnLocalGameBegin()");
        if (!IsServer) { return; }

        Debug.LogWarning("Changing Tickle Spots");
        if (phasesCompleted.Value >= amountOfSpotsToTicklePerPhase.Length)
        {
            ServerActivateWin();
            return;
        }

        // climableSpotIndices.Clear();
        climableSpotIndices.AddRange(spotIndices); // really not needed but done for the sake of it - lazy dylan

        //
        //List<int> hairManipIndicesToTickle = new List<int>();
        hairManipIndicesToTickle.Clear();
        spotsStillToTickle.Clear();

        for (int i = 0; i < amountOfSpotsToTicklePerPhase[phasesCompleted.Value]; i++)
        {
            int k = GetRandomIndex(0, spotIndices.Count);
            var climbingSpot = climbingSpots[spotIndices[k]];
            var parentHair = climbingSpot.GetComponentInParent<HairManuiplation>();
            if (spotsStillToTickle.Add(parentHair.name)) // add non-duplicates
            {
                hairManipIndicesToTickle.Add(allHairManipsOrdered.FindIndex(x => x == parentHair));
            }
            else // iterate through duplicates until we find a unique climbing spot to tickle
            {
                k = GetRandomIndex(0, spotIndices.Count);
                climbingSpot = climbingSpots[spotIndices[k]];
                while (!spotsStillToTickle.Add(climbingSpot.transform.parent.parent.name))
                {
                    if (spotsStillToTickle.Count == amountOfSpotsToTicklePerPhase[phasesCompleted.Value])
                    {
                        // chosen all the spots we are able to, so stop searching
                        break;
                    }
                    k = GetRandomIndex(0, spotIndices.Count);
                    climbingSpot = climbingSpots[spotIndices[k]];
                }
                // Debug.Log(spots[k].GetComponentInParent<HairManuiplation>());
                //
                parentHair = climbingSpot.GetComponentInParent<HairManuiplation>();
                hairManipIndicesToTickle.Add(allHairManipsOrdered.FindIndex(x => x == parentHair));
            }
        }
    }

    public void Tickle(string objName)
    {
        if (!IsServer) { return; }

        if (tickling) { return; }
        tickling = true;

        if (spotsStillToTickle.Contains(objName))
        {
            // Debug.Log($"I tickled {objName}");
            spotsStillToTickle.Remove(objName);
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
                PickTickleSpots(climableSpotIndices);
            }
        }

        tickling = false;
    }

    int GetRandomIndex(int min, int max)
    {
        return Random.Range(min, max);
    }

    // Everything from this point to the next section is strictly executed server-side /////////////

    // runs only on server's machine
    void OnServerUpdate()
    {
        // respond to changes in game state
        if (lastObservedGameState != currentGameState.Value)
        {
            switch ((GameState)currentGameState.Value)
            {
                case GameState.kNotStarted:
                    OnServerGameEnd();
                    break;
                case GameState.kStarted:
                    OnServerGameBegin();
                    break;
                case GameState.kWon:
                    OnServerWin();
                    break;
                case GameState.kLost:
                    OnServerLose();
                    break;
            }
        }

        // regular updates
        switch ((GameState)currentGameState.Value)
        {
            case GameState.kStarted:
                OnServerUpdateGame();
                break;
        }
    }

    void OnServerGameBegin()
    {
        // pick an initial set of tickle spots

        // make an int array indexing all the climbing spots to start
        List<int> climbingSpotIndices = new List<int>();
        var count = climbingSpots.Count;
        for (var i = 0; i < count; ++i)
        {
            climbingSpotIndices.Add(i);
        }

        // apply our first set of tickle spots
        PickTickleSpots(climbingSpotIndices);
    }

    void OnServerGameEnd()
    {
        // TODO (called after win/loss)
    }

    void OnServerWin()
    {
        // TODO (may not be necessary)
    }

    void OnServerLose()
    {
        // TODO (may not be necessary)
    }

    void OnServerUpdateGame()
    {
        if (timeRemaining.TotalSeconds <= 0)
        {
            ServerActivateLoss();
        }
    }

    // called once server-side to kick-off game process
    void ServerActivateGame()
    {
        currentGameState.Value = (int)GameState.kStarted;
    }

    // called once server-side to kick-off game win process
    void ServerActivateWin()
    {
        currentGameState.Value = (int)GameState.kWon;
    }

    // called once server-side to kick-off game lose process
    void ServerActivateLoss()
    {
        currentGameState.Value = (int)GameState.kLost;
    }

    // Everything from this point to the next section is strictly executed locally (by everyone) ///

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        win_loss_message.gameObject.SetActive(false);

        // no game started yet
        currentGameState.Value = (int)GameState.kNotStarted;
    }

    void UpdateTimer(string newTime)
    {
        timerText.text = newTime;
    }

    // runs on everyone's machine downstream of a win condition
    void OnLocalWin()
    {
        // show you won
        // swap to win scene/UI
        
        win_loss_message.gameObject.SetActive(true);
        win_loss_message.color = Color.green;
        win_loss_message.text = $"You tickled that giant so good! Great job!";
    }

    // runs on everyone's machine downstream of a lose condition
    void OnLocalLose()
    {
        // show timer has ran out
        // swap to lost scene/UI

        win_loss_message.gameObject.SetActive(true);
        win_loss_message.color = Color.red;
        win_loss_message.text = $"Oh no! You ran out of time!";
    }

    // runs on everyone's machine per update
    void OnLocalUpdate()
    {
        // respond to changes in game state
        if (lastObservedGameState != currentGameState.Value)
        {
            Debug.Log("OnLocalUpdate() w/ " + lastObservedGameState + " => " + currentGameState.Value);
            switch ((GameState)currentGameState.Value)
            {
                case GameState.kNotStarted:
                    OnLocalGameEnd();
                    break;
                case GameState.kStarted:
                    OnLocalGameBegin();
                    break;
                case GameState.kWon:
                    OnLocalWin();
                    break;
                case GameState.kLost:
                    OnLocalLose();
                    break;
            }
        }

        // regular updates
        switch ((GameState)currentGameState.Value)
        {
            case GameState.kStarted:
                OnLocalUpdateGame();
                break;
        }
    }

    void OnLocalGameBegin()
    {
        Debug.Log("GameObserver.OnLocalGameBegin()");

        timerText.gameObject.SetActive(true);
        timeRemaining = TimeSpan.FromSeconds(matchTimer * 60);
        timerText.text = $"{timeRemaining.TotalMinutes}:00";

        win_loss_message.gameObject.SetActive(false);

        // decide a canonical ordering of climbing spots and hold on to these
        // FYI leveraging some code that is supposed to guarantee ordering...
        ClimbingSpot[] climbingSpotArray = GameObject.FindObjectsOfType<ClimbingSpot>();
        climbingSpotArray = xyz.HierarchicalSorting.Sort(climbingSpotArray);
        climbingSpots = new List<ClimbingSpot>();
        climbingSpots.AddRange(climbingSpotArray);

        // also decide a canonical ordering of all hair manipulations
        HairManuiplation[] hairManipulationArray = GameObject.FindObjectsOfType<HairManuiplation>();
        hairManipulationArray = xyz.HierarchicalSorting.Sort(hairManipulationArray);
        allHairManipsOrdered = new List<HairManuiplation>();
        allHairManipsOrdered.AddRange(hairManipulationArray);
    }

    void OnLocalGameEnd()
    {
        // after win/lose screens (TODO)
    }

    void OnLocalUpdateGame()
    {
        if (timeRemaining.TotalSeconds > 0)
        {
            timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(Time.deltaTime));
            UpdateTimer(timeRemaining.ToString(@"mm\:ss"));
        }

        bool newTickleList = lastObservedHairManipIndicesToTickle.Count != hairManipIndicesToTickle.Count;
        for (var i = 0; !newTickleList && i < lastObservedHairManipIndicesToTickle.Count; ++i)
        {
            newTickleList = lastObservedHairManipIndicesToTickle[i] != hairManipIndicesToTickle[i];
        }
        if (newTickleList)
        {
            lastObservedHairManipIndicesToTickle.Clear();
            for (var i = 0; i < hairManipIndicesToTickle.Count; ++i)
            {
                lastObservedHairManipIndicesToTickle.Add(hairManipIndicesToTickle[i]);
            }

            // convert hairManipIndicesToTickle to spotsToTickle
            spotsToTickle.Clear();
            for (var i = 0; i < hairManipIndicesToTickle.Count; ++i)
            {
                spotsToTickle.Add(allHairManipsOrdered[hairManipIndicesToTickle[i]]);
            }

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(LerpTickleSpotColors());
        }
    }

    // this is strictly a visual effect that we want to execute locally
    IEnumerator LerpTickleSpotColors()
    {
        float t = 0;
        Color minColor = startColor;
        Color maxColor = targetColor;
        
        while (true)
        {
            foreach (var t1 in spotsToTickle)
            {
                t1.shellColor = Color.Lerp(minColor, maxColor, t);
            }

            t += 0.3f * Time.deltaTime;

            if (t > 1)
            {
                (maxColor, minColor) = (minColor, maxColor);

                t = 0f;
            }

            yield return null;
        }
    }

    // another visual effect we want to execute locally
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

    // Public entry points that fork behavior based on network conditions //////////////////////////

    public void OnStartGame()
    {
        if (!IsServer)
        {
            return;
        }

        ServerActivateGame();
    }

    // Update is called once per frame
    void Update()
    {
        OnLocalUpdate();

        // wall out everyone but the server at this point
        if (IsServer)
        {
            OnServerUpdate();
        }

        // update this now for next time
        lastObservedGameState = currentGameState.Value;

        /*
        // DJMC: commenting out for now -- logic not networked

        // debug for loss cond
        if (Input.GetKeyDown(KeyCode.F1))
        {
            timeRemaining = TimeSpan.FromSeconds(0);
            UpdateTimer(timeRemaining.ToString(@"mm\:ss"));
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ServerActivateWin();
        }
        //*/
    }
}
