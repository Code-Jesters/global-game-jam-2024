﻿using System.Collections.Generic;
using UnityEngine;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// Associates a symbol index with the sprite to display for symbol objects matching the index.
    /// </summary>
    public class SymbolData : ScriptableObject
    {
        [SerializeField] public List<Sprite> m_availableSymbols;
        public int SymbolCount => m_availableSymbols.Count;

        public Sprite GetSymbolForIndex(int index)
        {
            if (index < 0 || index >= SymbolCount)
                index = 0;
            return m_availableSymbols[index];
        }
    }
}
