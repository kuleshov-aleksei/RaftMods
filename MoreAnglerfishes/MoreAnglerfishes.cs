using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

[HarmonyPatch(typeof(LandmarkEntitySpawner), "CreateEntity")]
public class LandmarkEntitySpawner_CreateEntity_Patch
{
    // Exclusive upper bound
    private const int MAXIMUM_ANGLERFISHES_PER_LANDMARK = 4;
    private const int MINUMUM_ANGLERFISHES_PER_LANDMARK = 1;
    private const int MAX_AVAILABLE_LOCATIONS = 10;
    private const int RANDOM_SEED = 42069;
    private static DateTime m_lastSpawnTime = default;
    private static TimeSpan m_throttleDuration = TimeSpan.FromSeconds(5);

    private static readonly List<string> m_allowedSpawnPoints = new List<string>
    {
        "Item/Stone",
        "Item/Sand",
        "Item/Scrap",
        "Item/Clay",
        "Item/GiantClam",
    };

    public static void Postfix(ref LandmarkEntitySpawner __instance)
    {
        Landmark landmark = __instance.landmark;
        Debug.Log($"Landmark {landmark.name} spawned");

        // Prevent spawning multiple anglerfishes at the same location
        if (DateTime.UtcNow <= m_lastSpawnTime + m_throttleDuration)
        {
            return;
        }

        m_lastSpawnTime = DateTime.UtcNow;

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
            Vector3 position = pickupItem.transform.position;
            Quaternion quaternion = pickupItem.transform.rotation;

            GameObject gameObject = new GameObject();
            LandmarkEntitySpawner anglerFishSpawner = gameObject.AddComponent<LandmarkEntitySpawner>();
            WaypointHandler waypointHandler = gameObject.AddComponent<WaypointHandler>();
            Waypoint headWaypoint = gameObject.AddComponent<Waypoint>();
            anglerFishSpawner.waypointHandler = waypointHandler;
            anglerFishSpawner.spawnerIndex = 420;
            anglerFishSpawner.landmark = landmark;
            headWaypoint.waypointHandler = waypointHandler;

            position.y += random.Next(3, 5);

            position.x -= random.Next(1, 4);
            Waypoint firstWaypoint = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);
            position.z -= random.Next(1, 4);
            Waypoint secondWaypoint = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);
            position.x += random.Next(1, 4);
            Waypoint thirdWaypoint = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);
            position.z += random.Next(1, 4);
            Waypoint forthWaypoint = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);
            firstWaypoint.next = secondWaypoint;
            secondWaypoint.next = thirdWaypoint;
            thirdWaypoint.next = forthWaypoint;
            forthWaypoint.next = firstWaypoint;

            waypointHandler.waypoints.Add(firstWaypoint);
            waypointHandler.waypoints.Add(secondWaypoint);
            waypointHandler.waypoints.Add(thirdWaypoint);
            waypointHandler.waypoints.Add(forthWaypoint);

            Debug.Log($"Spawning Anglerfish at {position.x} {position.y} {position.z}");

            ComponentManager<Network_Host_Entities>.Value.CreateAINetworkBehaviour(AI_NetworkBehaviourType.AnglerFish, position, anglerFishSpawner);
        }
    }
}
