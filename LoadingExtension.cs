﻿using ColossalFramework;
using ICities;
using MetroOverhaul.Detours;
using MetroOverhaul.Redirection;
using MetroOverhaul.NEXT;
using System;
using UnityEngine;
using MetroOverhaul.NEXT.Extensions;
using MetroOverhaul.NEXT.Texturing;

namespace MetroOverhaul
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public static Initializer Container;
        public static bool Done { get; private set; } // Only one Assets installation throughout the application

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            InstallAssets();
            if (Container == null)
            {
                Container = new GameObject("MetroOverhaul").AddComponent<Initializer>();
            }
            Redirector<DepotAIDetour>.Deploy();
            //Redirector<MetroTrainAIDetour>.Deploy(); //don't deploy this! For some reason that causes citizens not boarding trains
        }

        private static void InstallAssets()
        {
            if (Done) // Only one Assets installation throughout the application
            {
                return;
            }

            var path = Util.AssemblyPath;
            foreach (var action in AssetManager.instance.CreateLoadingSequence(path))
            {
                var localAction = action;

                Loading.QueueLoadingAction(() =>
                {
                    try
                    {
                        localAction();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                });
            }
            Done = true;
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (Container == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(Container.gameObject);
            Container = null;
            Redirector<DepotAIDetour>.Revert();
            Redirector<MetroTrainAI>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            MetroStations.UpdateMetro(0, 0);
            var vehicles = Singleton<VehicleManager>.instance.m_vehicles;
            for (ushort i = 0; i < vehicles.m_size; i++)
            {
                var vehicle = vehicles.m_buffer[i];
                if (vehicle.m_flags == ~Vehicle.Flags.All || vehicle.Info == null)
                {
                    continue;
                }
                if (vehicle.Info.name == "Metro")
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(i);
                }
            }
            UpdateEffect();
            var prefabElevated = PrefabCollection<NetInfo>.FindLoaded("Metro Track Elevated");
            if (prefabElevated != null)
            {
                LateBuildUp(prefabElevated, NetInfoVersion.Elevated);
            }
            var prefabBridge = PrefabCollection<NetInfo>.FindLoaded("Metro Track Bridge");
            if (prefabBridge != null)
            {
                LateBuildUp(prefabBridge, NetInfoVersion.Bridge);
            }
            var steelPrefabElevated = PrefabCollection<NetInfo>.FindLoaded("Steel Metro Track Elevated");
            if (steelPrefabElevated != null)
            {
                LateBuildUp(steelPrefabElevated, NetInfoVersion.Elevated);
            }
            var steelPrefabBridge = PrefabCollection<NetInfo>.FindLoaded("Steel Metro Track Bridge");
            if (steelPrefabBridge != null)
            {
                LateBuildUp(steelPrefabBridge, NetInfoVersion.Bridge);
            }
        }
        private void LateBuildUp(NetInfo prefab, NetInfoVersion version)
        {
            switch (prefab.name)
            {
                case "Steel Metro Track Elevated":
                    {
                        var steelElevatedPillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Steel Metro Elevated Pillar");
                        steelElevatedPillarInfo.SetMeshes(
                            @"Meshes\Elevated_Pillar_Steel.obj",
                            @"Meshes\Elevated_Pillar_Steel.obj"
                        ).SetConsistentUVs();

                        steelElevatedPillarInfo.SetTextures(
                            new TextureSet(
                                @"Textures\Elevated_Pillar_Steel__MainTex.png",
                                @"Textures\Elevated_Pillar_Steel__AlphaMap.png",
                                @"Textures\Elevated_Pillar_Steel__XYSMap.png"
                            ));
                        var bridgeAI = prefab.GetComponent<TrainTrackBridgeAI>();
                        if (steelElevatedPillarInfo != null && bridgeAI != null)
                        {
                            bridgeAI.m_bridgePillarInfo = steelElevatedPillarInfo;
                            bridgeAI.m_bridgePillarOffset = 2;
                        }
                        break;
                    }
                case "Steel Metro Track Bridge":
                    {
                        var steelBridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Steel Metro Bridge Pillar");
                        steelBridgePillarInfo.SetMeshes(@"Meshes\Elevated_Pillar.obj", @"Meshes\Elevated_Pillar.obj").SetConsistentUVs();
                        steelBridgePillarInfo.SetTextures(new TextureSet(@"Textures\Elevated_Pillar__MainTex.png"));
                        var bridgeAI = prefab.GetComponent<TrainTrackBridgeAI>();
                        if (steelBridgePillarInfo != null && bridgeAI != null)
                        {
                            bridgeAI.m_bridgePillarInfo = steelBridgePillarInfo;
                            bridgeAI.m_middlePillarInfo = steelBridgePillarInfo;
                        }
                        break;
                    }
                case "Metro Track Elevated":
                    {
                        var elevatedPillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Metro Elevated Pillar");
                        elevatedPillarInfo.SetMeshes(
                            @"Meshes\Elevated_Pillar.obj",
                            @"Meshes\Elevated_Pillar.obj"
                        ).SetConsistentUVs();

                        elevatedPillarInfo.SetTextures(
                            new TextureSet(
                                @"Textures\Elevated_Pillar__MainTex.png",
                                @"Textures\Elevated_Pillar__AlphaMap.png",
                                @"Textures\Elevated_Pillar__XYSMap.png"
                            ));

                        var bridgeAI = prefab.GetComponent<TrainTrackBridgeAI>();
                        if (elevatedPillarInfo != null && bridgeAI != null)
                        {
                            bridgeAI.m_bridgePillarInfo = elevatedPillarInfo;
                            //bridgeAI.m_bridgePillarOffset = -2;
                        }
                        break;
                    }
                case "Metro Track Bridge":
                    {
                        var bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded("Metro Bridge Pillar");
                        bridgePillarInfo.SetMeshes(
                            @"Meshes\Bridge_Pillar.obj",
                            @"Meshes\Bridge_Pillar.obj"
                        ).SetConsistentUVs();

                        bridgePillarInfo.SetTextures(
                            new TextureSet(
                                @"Textures\Bridge_Pillar__MainTex.png",
                                @"Textures\Bridge_Pillar__AlphaMap.png",
                                @"Textures\Bridge_Pillar__XYSMap.png"
                            ));

                        var bridgeAI = prefab.GetComponent<TrainTrackBridgeAI>();
                        if (bridgePillarInfo != null && bridgeAI != null)
                        {
                            bridgeAI.m_bridgePillarInfo = bridgePillarInfo;
                        }
                        break;
                    }
            }
        }
        private static void UpdateEffect()
        {
            var metro = PrefabCollection<VehicleInfo>.FindLoaded("Metro");
            var arriveEffect = ((MetroTrainAI)metro.m_vehicleAI).m_arriveEffect;
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); i++)
            {
                var info = PrefabCollection<VehicleInfo>.GetLoaded(i);
                var metroTrainAI = info?.m_vehicleAI as MetroTrainAI;
                if (metroTrainAI == null)
                {
                    continue;
                }
                info.m_effects = metro.m_effects;
                metroTrainAI.m_arriveEffect = arriveEffect;
            }
        }


    }
}