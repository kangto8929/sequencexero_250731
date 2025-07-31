using System.Collections.Generic;
using UnityEngine;

public class AIEscapePathfinder : MonoBehaviour
{
    public static List<PlaceConnector> FindEscapeRoute(PlaceConnector startPlace)
{
    Queue<PlaceConnector> queue = new Queue<PlaceConnector>();
    Dictionary<PlaceConnector, PlaceConnector> cameFrom = new Dictionary<PlaceConnector, PlaceConnector>();
    HashSet<PlaceConnector> visited = new HashSet<PlaceConnector>();

    queue.Enqueue(startPlace);
    visited.Add(startPlace);

    while (queue.Count > 0)
    {
        PlaceConnector current = queue.Dequeue();
        var state = current.GetComponentInChildren<PlaceState>();

        if (current != startPlace && IsSafeZone(state))
        {
            // 출발점에서 목적지까지 경로 전체를 반환
            return ReconstructPath(cameFrom, startPlace, current);
        }

        foreach (var neighbor in current.ConnectPlaces)
        {
            if (neighbor == null || visited.Contains(neighbor))
                continue;

            visited.Add(neighbor);
            cameFrom[neighbor] = current;
            queue.Enqueue(neighbor);
        }
    }

    return null;
}

// 경로 재구성 함수 예시
private static List<PlaceConnector> ReconstructPath(Dictionary<PlaceConnector, PlaceConnector> cameFrom, PlaceConnector start, PlaceConnector goal)
{
    List<PlaceConnector> path = new List<PlaceConnector>();
    PlaceConnector current = goal;
    while (current != start)
    {
        path.Add(current);
        current = cameFrom[current];
    }
    path.Add(start);
    path.Reverse();
    return path;
}


    /*private static PlaceConnector ReconstructFirstStep(Dictionary<PlaceConnector, PlaceConnector> cameFrom,
                                                       PlaceConnector start, PlaceConnector goal)
    {
        PlaceConnector current = goal;
        PlaceConnector prev = goal;

        while (cameFrom.ContainsKey(current))
        {
            prev = current;
            current = cameFrom[current];

            if (current == start)
                return prev;
        }

        return null;
    }*/

    private static bool IsSafeZone(PlaceState state)
    {
        if (state == null) return true;

        bool isBeware = state.BewareSystemCollapseIcon != null && state.BewareSystemCollapseIcon.activeSelf;
        bool isCollapsed = state.AlreadySystemCollapseIcon != null && state.AlreadySystemCollapseIcon.activeSelf;

        return !isBeware && !isCollapsed;
    }
}

