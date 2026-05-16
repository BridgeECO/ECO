using System.Collections.Generic;
using UnityEngine;

public static class DebugTool
{
    private static Transform _playerTransform;

    public static void InitDebugTool(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public static void ChangeCurrentRoom(Room targetRoom, Vector3 spawnPoint)
    {
        if (_playerTransform == null)
        {
            return;
        }
        Region.Instance.SetCurrentRoom(targetRoom, spawnPoint);
        _playerTransform.position = spawnPoint;
    }
}