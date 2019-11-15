using System.Collections.Generic;
using UnityEngine;
using static CharacterSelectionManager;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager getInstance;

    [SerializeField] GameObject orcWaypointsParent;
    [SerializeField] GameObject knightWaypointsParent;

[SerializeField]    List<GameObject> orcWaypoints = new List<GameObject>();
    [SerializeField] List<GameObject> knightWaypoints = new List<GameObject>();

    void Awake()
    {
        getInstance = this;

        foreach (var i in orcWaypointsParent.GetComponentsInChildren<WaypointIdentifier>())
        {
            orcWaypoints.Add(i.gameObject);
        }
        foreach (var i in knightWaypointsParent.GetComponentsInChildren<WaypointIdentifier>())
        {
            knightWaypoints.Add(i.gameObject);
        }
    }

    public List<GameObject> getWaypoint(PlayerFaction f)
    {
        List<GameObject> waypoints = new List<GameObject>();
        switch (f)
        {
            case PlayerFaction.Orc:
                waypoints = orcWaypoints;
                break;
            case PlayerFaction.Warrior:
                waypoints = knightWaypoints;
                break;
        }
        return waypoints;
    }
}