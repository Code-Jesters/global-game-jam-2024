using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LobbyRelaySample.UI
{
    /// <summary>
    /// Basic UI element that can be shown or hidden.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelBase : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> m_onVisibilityChange;
        bool showing;

        protected GameManager Manager
        {
            get
            {
                if (m_gameManager != null) return m_gameManager;
                return m_gameManager = GameManager.Instance;
            }
        }

        GameManager m_gameManager;
        CanvasGroup m_canvasGroup;
        List<UIPanelBase> m_uiPanelsInChildren = new List<UIPanelBase>(); // Otherwise, when this Shows/Hides, the children won't know to update their own visibility.

        public virtual void Start()
        {
            var children = GetComponentsInChildren<UIPanelBase>(true); // Note that this won't detect children in GameObjects added during gameplay, if there were any.
            foreach (var child in children)
                if (child != this)
                    m_uiPanelsInChildren.Add(child);
        }

        protected CanvasGroup MyCanvasGroup
        {
            get
            {
                if (m_canvasGroup != null) return m_canvasGroup;
                return m_canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }


        public void Show()
        {
            Show(true);
        }

        public void Show(bool propagateToChildren)
        {
            MyCanvasGroup.alpha = 1;
            MyCanvasGroup.interactable = true;
            MyCanvasGroup.blocksRaycasts = true;
            showing = true;
            m_onVisibilityChange?.Invoke(true);
            if (!propagateToChildren)
                return;
            foreach (UIPanelBase child in m_uiPanelsInChildren)
                child.m_onVisibilityChange?.Invoke(true);
        }

        public void Hide() // Called by some serialized events, so we can't just have targetAlpha as an optional parameter.
        {
            Hide(0);
        }

        public void Hide(float targetAlpha)
        {
            MyCanvasGroup.alpha = targetAlpha;
            MyCanvasGroup.interactable = false;
            MyCanvasGroup.blocksRaycasts = false;
            showing = false;
            m_onVisibilityChange?.Invoke(false);
            foreach (UIPanelBase child in m_uiPanelsInChildren)
                child.m_onVisibilityChange?.Invoke(false);
        }
    }
}
