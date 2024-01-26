﻿using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// Used by the host to actually track scores for all players, and by each client to monitor for updates to their own score.
    /// </summary>
    public class Scorer : NetworkBehaviour
    {
        [SerializeField] NetworkedDataStore m_dataStore = default;
        ulong m_localId;
        [SerializeField] TMP_Text m_scoreOutputText = default;

        [Tooltip("When the game ends, this will be called once for each player in order of rank (1st-place first, and so on).")]
        [SerializeField] UnityEvent<PlayerData> m_onGameEnd = default;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        // Called on the host.
        public void ScoreSuccess(ulong id)
        {
            int newScore = m_dataStore.UpdateScore(id, 1);
            UpdateScoreOutput_ClientRpc(id, newScore);
        }
        public void ScoreFailure(ulong id)
        {
            int newScore = m_dataStore.UpdateScore(id, -1);
            UpdateScoreOutput_ClientRpc(id, newScore);
        }

        [ClientRpc]
        void UpdateScoreOutput_ClientRpc(ulong id, int score)
        {
            if (m_localId == id)
                m_scoreOutputText.text = score.ToString("00");
        }

        public void OnGameEnd()
        {
            m_dataStore.GetAllPlayerData(m_onGameEnd);
        }
    }
}
