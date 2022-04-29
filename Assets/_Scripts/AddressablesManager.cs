using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesManager : MonoBehaviour
{
    [SerializeField] private AssetReference playersAssetReference;

    private void Start()
    {
        Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        playersAssetReference.InstantiateAsync().Completed += (go) => { Debug.Log("World Loaded."); };
    }

    private void OnDestroy()
    {
        // playersAssetReference.ReleaseInstance(_playerController);
    }
}