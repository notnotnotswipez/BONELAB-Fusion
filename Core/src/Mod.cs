﻿using System;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using MelonLoader;

using LabFusion.Grabbables;

namespace LabFusion
{
    public class FusionMod : MelonMod
    {
        public struct FusionVersion {
            public const byte versionMajor = 0;
            public const byte versionMinor = 0;
            public const short versionPatch = 1;
        }

        public static readonly Version Version = new Version(FusionVersion.versionMajor, FusionVersion.versionMinor, FusionVersion.versionPatch);
        public static FusionMod Instance { get; private set; }
        public static Assembly FusionAssembly { get; private set; }

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = MelonAssembly.Assembly;

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
            AssetBundleManager.OnLoadBundles();

            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
            HookingUtilities.HookAll();
            InternalLayerHelpers.OnLateInitializeLayer();
        }

        protected void OnInitializeNetworking() {
            InternalLayerHelpers.SetLayer(new SteamNetworkLayer());
        }

        public override void OnDeinitializeMelon() {
            InternalLayerHelpers.OnCleanupLayer();
        }

        public static void OnMainSceneInitialized() {
            string sceneName = LevelWarehouseUtilities.GetCurrentLevel().Title;
            
#if DEBUG
            FusionLogger.Log($"Main scene {sceneName} was initialized.");
#endif

            SyncManager.OnCleanup();
            RigData.OnCacheRigInfo(sceneName);
            PlayerRep.OnRecreateReps();
        }

        public override void OnUpdate() {
            // Log byte counts
#if DEBUG
            //FusionLogger.Log($"Network Frame Data: Bytes Upload: {NetworkInfo.BytesUp}, Bytes Download: {NetworkInfo.BytesDown}");
#endif

            // Reset byte counts
            NetworkInfo.BytesDown = 0;
            NetworkInfo.BytesUp = 0;

            // Update the jank level loading check
            LevelWarehouseUtilities.OnUpdateLevelLoading();

            // Store rig info/update avatars
            RigData.OnRigUpdate();

            // Send world messages
            PlayerRep.OnSyncRep();
            SyncManager.OnUpdate();
            PhysicsUtilities.OnSendPhysicsInformation();

            // Update and push all network messages
            InternalLayerHelpers.OnUpdateLayer();
        }

        public override void OnFixedUpdate() {
            PlayerRep.OnFixedUpdate();
            SyncManager.OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            // Flush any left over network messages
            InternalLayerHelpers.OnLateUpdateLayer();
        }

        public override void OnGUI() {
            InternalLayerHelpers.OnGUILayer();
        }
    }
}
