using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class TerrainObject : MonoBehaviour, IEnergyReceiver, IResettable
{
    [Foldout("Project")]
    [SerializeField]
    private List<TerrainGimmickEntry> _gimmickEntries = new List<TerrainGimmickEntry>();

    private List<TerrainGimmickBase> _runtimeGimmicks = new List<TerrainGimmickBase>();

    [Foldout("Energy")]
    [SerializeField]
    private Transform _activationPosition;

    [SerializeField]
    private Transform _deactivationPosition;

    public Transform ActivationPosition => _activationPosition;
    public Transform DeactivationPosition => _deactivationPosition;

    private bool _isEnergyActive;
    private Vector3 _initialPosition;

    public Rigidbody2D Rigidbody { get; set; }
    public Vector3 InitialPosition => _initialPosition;

    public bool HasMovementGimmick
    {
        get
        {
            for (int i = 0; i < _gimmickEntries.Count; i++)
            {
                if (_gimmickEntries[i].GimmickData != null &&
                    _gimmickEntries[i].GimmickData.IsMovementGimmick)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void Awake()
    {
        _initialPosition = transform.position;
        Rigidbody = GetComponent<Rigidbody2D>();
        foreach (var entry in _gimmickEntries)
        {
            if (entry.GimmickData != null)
            {
                _runtimeGimmicks.Add(entry.GimmickData.CreateGimmick(entry));
            }
        }
        ApplyGimmicks();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick.OnTerrainTriggerEnter2D(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick.OnTerrainTriggerExit2D(other);
        }
    }

    private void OnDestroy()
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick?.OnDestroy(this);
        }
    }

    private void ApplyGimmicks()
    {
        foreach (var gimmick in _runtimeGimmicks)
        {
            if (gimmick != null)
            {
                gimmick.Evaluate(this, _isEnergyActive);
            }
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (_activationPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _activationPosition.position);
        }

        if (_gimmickEntries != null)
        {
            foreach (var entry in _gimmickEntries)
            {
                if (entry.GimmickData != null)
                {
                    entry.GimmickData.DrawGizmos(this, entry);
                }
            }
        }
#endif
    }



    public void SetEnergyActive(bool isActive)
    {
        _isEnergyActive = isActive;
        ApplyGimmicks();
    }

    public void ResetState()
    {
        if (Rigidbody == null)
        {
            transform.position = _initialPosition;
        }
        else
        {
            Rigidbody.position = _initialPosition;
            Rigidbody.linearVelocity = Vector2.zero;
        }

        foreach (var gimmick in _runtimeGimmicks)
        {
            gimmick.ResetGimmick(this);
        }
        _isEnergyActive = false;
        ApplyGimmicks();
    }

#if UNITY_EDITOR
    [Button("Adjust Collider Size")]
    private void AdjustColliderSize()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        if (col != null)
        {
            float scaleX = Mathf.Abs(transform.localScale.x);
            float scaleY = Mathf.Abs(transform.localScale.y);

            if (0f < scaleX && 0f < scaleY)
            {
                UnityEditor.Undo.RecordObject(col, "Adjust Collider Size");

                col.edgeRadius = 0.2f;
                float sizeX = 1f - (0.4f / scaleX);
                float sizeY = 1f - (0.4f / scaleY);
                col.size = new Vector2(sizeX, sizeY);

                UnityEditor.EditorUtility.SetDirty(col);
                Debug.Log($"[TerrainObject] BoxCollider2D 사이즈 조정 완료. (Local Size: {col.size}, Local Edge Radius: {col.edgeRadius})");
            }
        }
        else
        {
            Debug.LogWarning("[TerrainObject] BoxCollider2D 컴포넌트를 찾을 수 없습니다.");
        }
    }
#endif
}
