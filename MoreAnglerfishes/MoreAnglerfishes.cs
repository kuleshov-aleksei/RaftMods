﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreAnglerfishes
{
    public class MoreAnglerfishes : Mod
    {
        private Harmony m_harmony;
        public void Start()
        {
            Debug.Log("Mod MoreAnglerfishes has been loaded!");
            m_harmony = new Harmony("com.encamy.more-anglerfishes");
            m_harmony.PatchAll();
        }

        public void OnModUnload()
        {
            m_harmony.UnpatchAll(m_harmony.Id);
            Debug.Log("Mod MoreAnglerfishes has been unloaded!");
        }

        [ConsoleCommand(name: "getPos", docs: "getPos")]
        public static void GetPosition()
        {
            Raft_Network raftNetwork = ComponentManager<Raft_Network>.Value;
            Vector3 position = raftNetwork.GetLocalPlayer().FeetPosition;
            Debug.Log($"Position is {position.x} {position.y} {position.z}");
        }
    }

    [HarmonyPatch(typeof(Landmark), "OnSpawn")]
    public class Landmark_CreateEntity_Patch
    {
        // Exclusive upper bound
        private const int MAXIMUM_ANGLERFISHES_PER_LANDMARK = 6;
        private const int MINUMUM_ANGLERFISHES_PER_LANDMARK = 4;
        private const int MAX_AVAILABLE_LOCATIONS = 20;
        private const int RANDOM_SEED = 42069;

        private static readonly List<string> m_allowedSpawnPoints = new List<string>
        {
            "Item/Stone",
            "Item/Sand",
            "Item/Scrap",
            "Item/Clay",
            "Item/GiantClam",
        };

        private static readonly Queue<uint> m_killList = new Queue<uint>();

        public static void Postfix(ref Landmark __instance)
        {
            Landmark landmark = __instance;
            Debug.Log($"Landmark {landmark.name} spawned");

            landmark.OnLandmarkReset += LandmarkReset;

            if (!Raft_Network.IsHost)
            {
                return;
            }

            System.Random random = new System.Random(RANDOM_SEED);
            IEnumerable<PickupItem> items = landmark.GetComponentsInChildren<PickupItem>()
                .Where(x => m_allowedSpawnPoints.Contains(x.pickupTerm))
                .OrderBy(x => x.transform.position.y) // deepest objects go first
                .Take(MAX_AVAILABLE_LOCATIONS) // narrow pool of spawnable location
                .OrderBy(x => random.Next()) // shuffle pool
                .Take(random.Next(MINUMUM_ANGLERFISHES_PER_LANDMARK, MAXIMUM_ANGLERFISHES_PER_LANDMARK)); // take N final locations

            foreach (PickupItem pickupItem in items)
            {
                GameObject gameObject = new GameObject();
                LandmarkEntitySpawner anglerFishSpawner = gameObject.AddComponent<LandmarkEntitySpawner>();
                WaypointHandler waypointHandler = gameObject.AddComponent<WaypointHandler>();
                Waypoint headWaypoint = gameObject.AddComponent<Waypoint>();
                anglerFishSpawner.waypointHandler = waypointHandler;
                anglerFishSpawner.spawnerIndex = 420;
                anglerFishSpawner.landmark = landmark;
                headWaypoint.waypointHandler = waypointHandler;

                Vector3 position = pickupItem.transform.position;
                Quaternion quaternion = pickupItem.transform.rotation;
                position.y += random.Next(3, 5);

                WaypointFactory.CreateWaypoints(waypointHandler, position, random, headWaypoint, quaternion);

                Debug.Log($"Spawning Anglerfish at {position.x} {position.y} {position.z}");

                AI_NetworkBehaviour aiBehaviour = ComponentManager<Network_Host_Entities>.Value.CreateAINetworkBehaviour(AI_NetworkBehaviourType.AnglerFish, position, anglerFishSpawner);
                m_killList.Enqueue(aiBehaviour.ObjectIndex);
            }
        }

        private static void LandmarkReset()
        {
            Debug.Log("Landmark reset, clearing stale anglerfishes");
            while (m_killList.Count > 0)
            {
                uint objectToKillIndex = m_killList.Dequeue();
                NetworkIDManager.SendIDBehaviourDead(objectToKillIndex, typeof(AI_NetworkBehaviour), true);
            }
        }
    }
}
