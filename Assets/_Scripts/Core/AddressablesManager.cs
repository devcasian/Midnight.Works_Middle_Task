using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesManager : MonoBehaviour
{
    [SerializeField]
    private AssetReference loadablePrefab;

    private async void Start()
    {
        var handle = loadablePrefab.LoadAssetAsync<GameObject>();
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded) return;

        var gameObjectPrefab = handle.Result;
        Instantiate(gameObjectPrefab);
        Debug.Log($"Prefab {gameObjectPrefab.name} loaded via Addressables.");
        Addressables.Release(handle);
    }
}