
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ECO
{
    public static class RES
    {
        public static bool TryLoadTextAddressableAsset(out string text, string key, bool isShowErr = true)
        {
            text = "";

            AsyncOperationHandle<TextAsset> handle = LoadAddressableAssetAsync<TextAsset>(key, isShowErr);
            handle.WaitForCompletion();

            if (!handle.IsValid())
                return false;

            TextAsset asset = handle.Result;

            if (asset == null)
                return false;

            text = asset.text;
            return true;
        }

        public static bool TryLoadPrefabAddressableAsset(out GameObject prefabGO, string key, bool isShowErr = true)
        {
            prefabGO = null;

            AsyncOperationHandle<GameObject> handle = LoadAddressableAssetAsync<GameObject>(key, isShowErr);
            handle.WaitForCompletion();

            if (!handle.IsValid())
                return false;

            prefabGO = handle.Result;
            return !UNITY.IsNullGameObj(prefabGO);
        }

        public static bool TryCreateInstanceAddressableAsset(out GameObject instanceGO, Transform rootTF, string key, bool isShowErr = true)
        {
            instanceGO = null;

            AsyncOperationHandle<GameObject> handle = LoadAndCreateAddressableGameObjectAsync(key, rootTF, isShowErr);
            handle.WaitForCompletion();

            if (!handle.IsValid())
                return false;

            instanceGO = handle.Result;

            return !UNITY.IsNullGameObj(instanceGO);
        }

        public static bool TryLoadAddressableAsset<T>(out T res, string key, bool isShowErr = true)
        {
            res = default(T);

            AsyncOperationHandle<T> handle = LoadAddressableAssetAsync<T>(key, isShowErr);
            handle.WaitForCompletion();

            if (!handle.IsValid())
                return false;

            res = handle.Result;
            return res != null;
        }

        private static AsyncOperationHandle<T> LoadAddressableAssetAsync<T>(string key, bool isShowErr = true)
        {
            ResourceManager.ExceptionHandler = CustomExceptionHandler;

            AsyncOperationHandle<T> handle = new AsyncOperationHandle<T>();

            try
            {
                handle = Addressables.LoadAssetAsync<T>(key);
            }
            catch (InvalidKeyException)
            {
                if (isShowErr)
                {
                    LOG.Error($"Invalid Key. Key({key})");
                }
            }
            catch (Exception exc)
            {
                if (isShowErr)
                {
                    LOG.Error($"Invalid Exception. Exception({exc})");
                }
            }

            return handle;
        }

        private static AsyncOperationHandle<GameObject> LoadAndCreateAddressableGameObjectAsync(string key, Transform rootTF, bool isShowErr = true)
        {
            ResourceManager.ExceptionHandler = CustomExceptionHandler;

            AsyncOperationHandle<GameObject> handle = new AsyncOperationHandle<GameObject>();

            try
            {
                handle = Addressables.InstantiateAsync(key, rootTF);
            }
            catch (InvalidKeyException)
            {
                if (isShowErr)
                {
                    LOG.Error($"Invalid Key. Key({key})");
                }
            }

            return handle;
        }

        private static void CustomExceptionHandler(AsyncOperationHandle handle, System.Exception exc)
        {
            throw exc;
        }
    }
}