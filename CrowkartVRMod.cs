using BepInEx;
using CrowkartVRMod.Utilities;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections;
using USceneMgr = UnityEngine.SceneManagement.SceneManager;
using static CrowkartVRMod.Utilities.LogUtils;
using UnityEngine.UI;
using CBKart;
using CBKart.UI;

namespace CrowkartVRMod
{
    [BepInPlugin($"org.bepinex.plugins.crowkartvrmod", "Crowkart VR Mod", "1.0.0.0")]
    [BepInProcess("Crowkart.exe")]
    public class CrowkartVRMod : BaseUnityPlugin
    {
        private GameObject _player;

        private void Awake()
        {
            AssetManager.Initialize();
            USceneMgr.activeSceneChanged += OnSceneChange;
        }

        private IEnumerator Start()
        {
            yield return InitXR();

            _player = Instantiate(AssetManager.FetchAsset<GameObject>("Player"));
            DontDestroyOnLoad(_player);
            GameObject vrCam = _player.transform.Find("SteamVRObjects/VRCamera").gameObject;
            vrCam.AddComponent<AkGameObj>();
            vrCam.AddComponent<AkAudioListener>();

            yield return AutoStartGame();
        }

        private void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            DisableAllCamerasExceptVRCamera();
            switch (nextScene.name)
            {
                case "The Village":
                case "The Village 2":
                case "The Village 3":
                case "The Village 4":
                case "The Village 5":
                case "The Forge":
                case "The Forge 2":
                case "The Forge 3":
                case "The Forge 4":
                case "The Forge 5":
                    StartCoroutine(ParentPlayerToKart());
                    break;
            }
        }

        private IEnumerator AutoStartGame()
        {
            yield return ProceedToMainMenu();

            yield return new WaitUntil(() => USceneMgr.GetActiveScene().name == "MainMenuSteam");
            yield return new WaitWhile(() => GameObject.Find("Canvas") == null);

            GameObject.Find("Canvas/Connect/Buttons/Button (Host)").GetComponent<Button>().onClick.Invoke();

            yield return new WaitUntil(() => USceneMgr.GetActiveScene().name == "RaceSetup");
            yield return new WaitWhile(() => GameObject.Find("Canvas") == null);

            yield return new WaitWhile(() => GameObject.Find("Canvas/Ready/Button").GetComponent<Button>().isActiveAndEnabled == false);
            GameObject.Find("Canvas/Ready/Button").GetComponent<Button>().onClick.Invoke();
        }

        private IEnumerator ProceedToMainMenu()
        {
            yield return new WaitWhile(() => GameObject.Find("Game") == null);
            
            GameObject.Find("Game").GetComponent<PKPGame>().StartGame();

            yield return new WaitWhile(() => USceneMgr.GetActiveScene().name != "Disclaimer");
            yield return new WaitWhile(() => GameObject.Find("Canvas/CG/Ip/Mid/Button (Back) ") == null);

            GameObject.Find("Canvas/CG/Ip/Mid/Button (Back) ").GetComponent<UINextScene>().ProceedToNextScene();
        }

        private void DisableAllCamerasExceptVRCamera()
        {
            foreach (var cam in FindObjectsOfType<Camera>(true))
            {
                if (cam.name == "VRCamera") continue;
                cam.enabled = false;
                cam.gameObject.SetActive(false);
            }
        }

        private const float _uiScale = 0.005f;
        private IEnumerator ParentPlayerToKart()
        {
            yield return new WaitWhile(() => GameObject.Find("Kart Drift RB Auto Online(Clone)") == null);
            yield return new WaitWhile(() => GameObject.Find("KartSprite") == null);
            yield return new WaitWhile(() => GameObject.Find("Canvas (1)") == null);

            var kart = GameObject.Find("Kart Drift RB Auto Online(Clone)");
            kart.AddComponent<KartInputVR>();
            Transform cart = kart.transform.Find("Rotator/Visual Holder/Visual Shaker/cart_final").transform;
            Destroy(cart.Find("PlayerRend").gameObject);

            var playerInst = Instantiate(_player, cart).transform;
            playerInst.localPosition += Vector3.back;
            playerInst.localRotation = Quaternion.Euler(Vector3.up * 180);
            playerInst.GetComponent<Player>().trackingOriginTransform = cart;

            foreach (var canvas in FindObjectsOfType<Canvas>())
            {
                if (canvas.name.Contains("Menu")) continue;
                canvas.worldCamera = playerInst.Find("SteamVRObjects/VRCamera").GetComponent<Camera>();
                canvas.transform.SetParent(cart, false);
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.localPosition = Vector3.back * 5 + Vector3.up * 2;
                canvas.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1) * _uiScale;
            }

            GameObject.Find("KartSprite").SetActive(false);
        }

        private IEnumerator InitXR()
        {
            StopXR();

            XRSettings.LoadDeviceByName("OpenVR Display");
            XRSettings.LoadDeviceByName("OpenVR Input");

            var generalSettings = AssetManager.FetchAsset<XRGeneralSettings>("Standalone Settings");

            yield return new WaitWhile(() => XRGeneralSettings.Instance == null);

            var managerSettings = AssetManager.FetchAsset<XRManagerSettings>("Standalone Providers");

            XRGeneralSettings.Instance.Manager = managerSettings;
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            yield return new WaitUntil(() => XRGeneralSettings.Instance.Manager.isInitializationComplete);

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                LogError("XR initialization failed. Check Editor or Player log for details.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }

            yield return InitSteamVR();
        }

        private IEnumerator InitSteamVR()
        {
            SteamVR.Initialize(true);
            

            yield return new WaitUntil(() => SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess);

            Log("SteamVR successfully initialized.");
        }
        private void StopXR()
        {
            if (XRGeneralSettings.Instance == null) return;
            if (!XRGeneralSettings.Instance.Manager.isInitializationComplete) return;

            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
