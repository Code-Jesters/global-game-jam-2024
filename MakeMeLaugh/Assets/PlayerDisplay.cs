using LobbyRelaySample;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;

public class PlayerDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject body;
    // Start is called before the first frame update
    void Start()
    {
        Material[] materials = body.GetComponent<Renderer>().materials;
        Color color = Color.white;
        switch (GameManager.Instance.m_LocalUser.Emote.Value)
        {
            case EmoteType.Tongue:
                color = Color.blue;
                break;
            case EmoteType.Unamused:
                color = Color.red;
                break;
            case EmoteType.Frown:
                color = Color.yellow;
                break;
            case EmoteType.Smile:
            case EmoteType.None:
                color = Color.green;
                break;
        }
        foreach (Material m in materials)
        {
            m.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
