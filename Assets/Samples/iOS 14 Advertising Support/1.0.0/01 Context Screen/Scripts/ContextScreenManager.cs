using Unity.Advertisement.IosSupport.Components;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Unity.Advertisement.IosSupport.Samples
{
    /// <summary>
    /// This component will trigger the context screen to appear when the scene starts,
    /// if the user hasn't already responded to the iOS tracking dialog.
    /// </summary>
    public class ContextScreenManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated by this component.
        /// The prefab has to have an ContextScreenView component on its root GameObject.
        /// </summary>
        public ContextScreenView contextScreenPrefab;

        private ContextScreenView contextScreen;

        void Start()
        {
#if UNITY_IOS && !UNITY_EDITOR
    var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

    // Only show the context screen if status is NOT_DETERMINED and we haven’t shown it yet
    if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED &&
        PlayerPrefs.GetInt("ATT_Request_Shown", 0) == 0)
    {
        contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

        contextScreen.sentTrackingAuthorizationRequest += () =>
        {
            // Mark that the ATT request has been shown
            PlayerPrefs.SetInt("ATT_Request_Shown", 1);
            PlayerPrefs.Save();

            Destroy(contextScreen.gameObject);
        };
    }
#else
                Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS.");
#endif
                StartCoroutine(LoadGame());
            }

            private IEnumerator LoadGame()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                Debug.Log("requesting");
                status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                yield return null;
            }
#endif
            if (contextScreen != null) 
            {
                Destroy(contextScreen.gameObject);
            }
            yield return null;
        }
    }   
}
