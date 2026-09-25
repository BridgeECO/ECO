using System;
using UnityEngine;

public sealed class BossTerrainDropMotion
{
    private readonly GameObject[] _terrainObjects;
    private readonly Vector3[] _landingPositions;
    private float _dropHeight;
    private float _dropDuration;
    private float _elapsedTime;
    private bool _isPlaying;

    public BossTerrainDropMotion(GameObject[] terrainObjects)
    {
        _terrainObjects = terrainObjects is null
            ? Array.Empty<GameObject>()
            : terrainObjects;
        _landingPositions = new Vector3[_terrainObjects.Length];

        for (int i = 0; i < _terrainObjects.Length; i++)
        {
            GameObject terrainObject = _terrainObjects[i];
            if (terrainObject != null)
            {
                _landingPositions[i] = terrainObject.transform.localPosition;
            }
        }
    }

    public void Play(float dropHeight, float dropDuration)
    {
        _dropHeight = Mathf.Max(0f, dropHeight);
        _dropDuration = Mathf.Max(0f, dropDuration);
        _elapsedTime = 0f;
        _isPlaying = 0f < _dropHeight && 0f < _dropDuration;

        SetHeightOffset(_isPlaying ? _dropHeight : 0f);
        SetTerrainObjectsActive(true);
    }

    public void Tick(float deltaTime)
    {
        if (!_isPlaying)
        {
            return;
        }

        _elapsedTime += deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _dropDuration);
        float heightOffset = _dropHeight * (1f - progress * progress);
        SetHeightOffset(heightOffset);

        if (progress >= 1f)
        {
            Complete();
        }
    }

    public void Complete()
    {
        _isPlaying = false;
        _elapsedTime = 0f;
        SetHeightOffset(0f);
    }

    private void SetTerrainObjectsActive(bool isActive)
    {
        for (int i = 0; i < _terrainObjects.Length; i++)
        {
            GameObject terrainObject = _terrainObjects[i];
            if (terrainObject != null)
            {
                terrainObject.SetActive(isActive);
            }
        }
    }

    private void SetHeightOffset(float heightOffset)
    {
        for (int i = 0; i < _terrainObjects.Length; i++)
        {
            GameObject terrainObject = _terrainObjects[i];
            if (terrainObject == null)
            {
                continue;
            }

            Transform terrainTransform = terrainObject.transform;
            Transform parentTransform = terrainTransform.parent;
            Vector3 localOffset = parentTransform != null
                ? parentTransform.InverseTransformVector(Vector3.up * heightOffset)
                : Vector3.up * heightOffset;
            terrainTransform.localPosition = _landingPositions[i] + localOffset;
        }
    }
}
