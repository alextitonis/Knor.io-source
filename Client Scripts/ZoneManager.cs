using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager getInstance;
    void Awake() { getInstance = this; }

    [System.Serializable]
    public class Zone
    {
        public int ID;
        public Vector3 Position;
        public int OwnerID;
        public GameObject effect;

        public Zone(int ID, Vector3 Position, int OwnerID)
        {
            this.ID = ID;
            this.Position = Position;
            this.OwnerID = OwnerID;
            
            effect = getInstance.SpawnEffect(this);
        }
    }

    [SerializeField] GameObject zoneEffectNeutral, zoneEffectPrefabKnight, zoneEffectPrefabOrc;
    [SerializeField] List<Zone> zones = new List<Zone>();

    public void Add(ZoneDataPacket packet)
    {
        if (Get(packet.id) != null)
        {
            Debug.Log("Trying to add again a zone with id: " + packet.id);
            return;
        }

        Zone _zone = new Zone(packet.id, new Vector3(packet.centerX, packet.centerY, packet.centerZ), packet.ownerID);
        zones.Add(_zone);
    }
    public Zone Get(int ID)
    {
        return zones.Find(x => x.ID == ID);
    }
    public void SetOwner(ZoneUpdatePacket packet)
    {
        if (zones.Find(x => x.ID == packet.id) == null)
            return;

        zones.Find(x => x.ID == packet.id).OwnerID = packet.ownerID;
        zones.Find(x => x.ID == packet.id).effect = SpawnEffect(zones.Find(x=>x.ID==packet.id));
    }

    public GameObject SpawnEffect(Zone zone)
    {
        if (zone == null)
            return null;

        if (zone.effect != null)
            Destroy(zone.effect);

        GameObject _go = zoneEffectNeutral;
        if (zone.OwnerID == 0)
            _go = zoneEffectPrefabKnight;
        else if (zone.OwnerID == 1)
            _go = zoneEffectPrefabOrc;

        GameObject go = Instantiate(_go, zone.Position, Quaternion.Euler(new Vector3(-90f, 0f, 0f)));

        return go;
    }
}
