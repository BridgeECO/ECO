using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ECO
{
    [DisallowMultipleComponent]
    public class TempFSM : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            public string name;
            public Color activeColor;
        }

        [Serializable]
        public class Transition
        {
            public string from;
            public string to;
        }

        private static readonly Color DEFAULT_IDLE = new Color(0.20f, 0.70f, 1.00f, 0.90f);
        private static readonly Color DEFAULT_RUN = new Color(0.30f, 1.00f, 0.30f, 0.90f);
        private static readonly Color DEFAULT_JUMP = new Color(1.00f, 0.80f, 0.10f, 0.95f);
        private static readonly Color DEFAULT_FALL = new Color(1.00f, 0.30f, 0.30f, 0.95f);
        private static readonly Color DEFAULT_OTHER = new Color(1.00f, 0.40f, 1.00f, 0.95f);

        [Header("States / Transitions")]
        public List<State> states = new List<State>();
        public List<Transition> transitions = new List<Transition>();

        [Header("Runtime")]
        public string initialState = "Idle";
        [SerializeField] private string _currentState;

        public string CurrentState => _currentState;
        public Color CurrentStateColor => GetActiveColor(_currentState);

        [Header("Apply To (Instance Renderers)")]
        public List<SpriteRenderer> spriteTargets = new List<SpriteRenderer>();
        public List<Renderer> meshTargets = new List<Renderer>();
        public List<Graphic> uiTargets = new List<Graphic>();

        [Tooltip("대상이 비어있으면 Awake에서 자동 탐색")]
        public bool IsAutoFindTargets = true;

        private MaterialPropertyBlock _mpb;
        public event Action<string, string> OnStateChanged;

        private void Awake()
        {
            if (string.IsNullOrEmpty(_currentState)) _currentState = initialState;

            if (IsAutoFindTargets)
            {
                if (spriteTargets.Count == 0)
                    spriteTargets.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
                if (meshTargets.Count == 0)
                    meshTargets.AddRange(GetComponentsInChildren<Renderer>(true));
                if (uiTargets.Count == 0)
                    uiTargets.AddRange(GetComponentsInChildren<Graphic>(true));
            }

            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            ApplyCurrentStateColor();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetState("Idle");
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetState("Run");
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetState("Jump");
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetState("Fall");
        }

        public void SetState(string nextState)
        {
            if (string.IsNullOrEmpty(nextState) || _currentState == nextState) return;

            bool hasTargetState = states.Exists(s => s.name == nextState);
            if (!hasTargetState) return;

            string prevState = _currentState;
            _currentState = nextState;

            ApplyCurrentStateColor();
            Debug.Log($"[FSM] {prevState} → {nextState}");
            OnStateChanged?.Invoke(prevState, _currentState);
        }

        public Color GetActiveColor(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return DEFAULT_OTHER;

            State found = states.Find(s => s.name == stateName);
            if (found != null && HasColorAssigned(found.activeColor))
                return found.activeColor;

            string key = stateName.ToLowerInvariant();
            if (key == "idle") return DEFAULT_IDLE;
            if (key == "run") return DEFAULT_RUN;
            if (key == "jump") return DEFAULT_JUMP;
            if (key == "fall") return DEFAULT_FALL;
            return DEFAULT_OTHER;
        }

        private bool HasColorAssigned(Color c)
        {
            return c.a > 0.0001f || c.maxColorComponent > 0.0001f;
        }

        private void ApplyCurrentStateColor()
        {
            Color c = CurrentStateColor;

            for (int i = 0; i < spriteTargets.Count; i++)
            {
                SpriteRenderer sr = spriteTargets[i];
                if (sr == null) continue;
                sr.color = c;
            }

            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            for (int i = 0; i < meshTargets.Count; i++)
            {
                Renderer r = meshTargets[i];
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);
                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseColor"))
                    _mpb.SetColor("_BaseColor", c);
                if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Color"))
                    _mpb.SetColor("_Color", c);
                r.SetPropertyBlock(_mpb);
            }

            for (int i = 0; i < uiTargets.Count; i++)
            {
                Graphic g = uiTargets[i];
                if (g == null) continue;
                g.color = c;
            }
        }
    }
}
