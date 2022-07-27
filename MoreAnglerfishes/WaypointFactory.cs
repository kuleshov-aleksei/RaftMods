using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreAnglerfishes
{
    internal static class WaypointFactory
    {
        private static int m_currentPathTypeId = 0;

        public static void CreateWaypoints(
            WaypointHandler waypointHandler,
            Vector3 initalPosition,
            System.Random random,
            Waypoint headWaypoint,
            Quaternion quaternion)
        {
            m_currentPathTypeId++;
            m_currentPathTypeId %= m_pathsElementsCount;
            PathType pathType = (PathType)m_currentPathTypeId;
            Debug.Log($"Current path type id is {m_currentPathTypeId} {pathType}");

            List<Waypoint> waypoints;
            switch (pathType)
            {
                case PathType.MRoute:
                    waypoints = CreateMRoute(initalPosition, random, headWaypoint, quaternion);
                    break;
                case PathType.Star:
                    waypoints = CreateStarRoute(initalPosition, random, headWaypoint, quaternion);
                    break;
                case PathType.Circle:
                default:
                    waypoints = CreateCircleRoute(initalPosition, random, headWaypoint, quaternion);
                    break;
            }

            waypointHandler.waypoints.AddRange(waypoints);
        }

        private static List<Waypoint> CreateMRoute(Vector3 initalPosition, System.Random random, Waypoint headWaypoint, Quaternion quaternion)
        {
            Debug.Log("Using M-route");
            Vector3 position = initalPosition;

            position.z -= 4;
            Waypoint waypointA = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.x -= 4;
            position.z += 2;
            Waypoint waypointB = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z += 2;
            position.x += 4;
            Waypoint waypointC = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.x -= 4;
            position.z += 2;
            Waypoint waypointD = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z += 2;
            position.x += 4;
            Waypoint waypointE = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            waypointA.next = waypointB;
            waypointB.next = waypointC;
            waypointC.next = waypointD;
            waypointD.next = waypointE;
            waypointE.next = waypointA;

            List<Waypoint> resultWaypoints = new List<Waypoint>(5);
            resultWaypoints.Add(waypointA);
            resultWaypoints.Add(waypointB);
            resultWaypoints.Add(waypointC);
            resultWaypoints.Add(waypointD);
            resultWaypoints.Add(waypointE);
            return resultWaypoints;
        }

        private static List<Waypoint> CreateStarRoute(Vector3 initalPosition, System.Random random, Waypoint headWaypoint, Quaternion quaternion)
        {
            Debug.Log("Using star route");
            Vector3 position = initalPosition;

            position.x -= 4;
            Waypoint waypointA = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z += 2;
            position.x += 6;
            Waypoint waypointB = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z -= 4;
            position.x -= 4;
            Waypoint waypointC = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z += 4;
            Waypoint waypointD = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            position.z -= 4;
            position.x += 4;
            Waypoint waypointE = GameObject.Instantiate<Waypoint>(headWaypoint, position, quaternion);

            waypointA.next = waypointB;
            waypointB.next = waypointC;
            waypointC.next = waypointD;
            waypointD.next = waypointE;
            waypointE.next = waypointA;

            List<Waypoint> resultWaypoints = new List<Waypoint>(5);
            resultWaypoints.Add(waypointA);
            resultWaypoints.Add(waypointB);
            resultWaypoints.Add(waypointC);
            resultWaypoints.Add(waypointD);
            resultWaypoints.Add(waypointE);

            return resultWaypoints;
        }

        private static List<Waypoint> CreateCircleRoute(Vector3 initalPosition, System.Random random, Waypoint headWaypoint, Quaternion quaternion)
        {
            Debug.Log("Using circle route");
            Vector3 position = initalPosition;

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

            List<Waypoint> resultWaypoints = new List<Waypoint>(4);
            resultWaypoints.Add(firstWaypoint);
            resultWaypoints.Add(secondWaypoint);
            resultWaypoints.Add(thirdWaypoint);
            resultWaypoints.Add(forthWaypoint);
            return resultWaypoints;
        }

        // hardcode for faster runtime
        private static readonly int m_pathsElementsCount = 3;
        public enum PathType
        {
            Circle = 0,
            MRoute,
            Star,
        }
    }
}
