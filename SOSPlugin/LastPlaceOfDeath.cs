using System;
using System.Collections.Generic;
using SOSPlugin.DB;
using Microsoft.Xna.Framework;
using Terraria;

namespace SOSPlugin
{
    /// <summary>
    /// Each type of object will be saved in json file.
    /// </summary>
    public class SOSKey
    {
        public bool saveMoney = true;
        public int lastDeathPlaceX = 0;
        public int lastDeathPlaceY = 0;
        public string lastDeathMessage = "Death Message";
        public int deathCount = 0;

        public SOSKey(bool saveMoney, int lastDeathPlaceX = 0,
                    int lastDeathPlaceY = 0, string lastDeathMessage = "Death Message")
        {
            this.saveMoney = saveMoney;
            this.lastDeathPlaceX = lastDeathPlaceX;
            this.lastDeathPlaceY = lastDeathPlaceY;
            this.lastDeathMessage = lastDeathMessage;
        }
    }

    class LastPlaceOfDeath
    {
        public Dictionary<int, Dictionary<string, SOSKey>> data; // Data holder
        private string dbpath; // File path target

        public int WorldID { set; get; }

        public LastPlaceOfDeath(string dbpath)
        {
            this.dbpath = dbpath;
            this.WorldID = Main.worldID;
            data = JsonFiler.LoadDataJson<Dictionary<int, Dictionary<string, SOSKey>>>(dbpath);
            if (data == null)
                data = new Dictionary<int, Dictionary<string, SOSKey>>();
        }

        public bool GetUserSavedMoney(string UUID)
        {
            SOSKey sos;
            Dictionary<string, SOSKey> ds;

            if (!data.TryGetValue(WorldID, out ds))
            {
                ds = new Dictionary<string, SOSKey>();
                data.Add(WorldID, ds);
            }

            if (!ds.TryGetValue(UUID, out sos))
            {
                sos = new SOSKey(false);
                ds.Add(UUID, sos);
            }

            return sos.saveMoney;
        }

        public SOSKey GetUserDeathInfo(string UUID)
        {
            SOSKey sos;
            Dictionary<string, SOSKey> ds;

            if (!data.TryGetValue(WorldID, out ds))
            {
                ds = new Dictionary<string, SOSKey>();
                data.Add(WorldID, ds);
            }

            if (!ds.TryGetValue(UUID, out sos))
            {
                sos = new SOSKey(false);
                ds.Add(UUID, sos);
            }

            return sos;
        }

        public void UpdateSaveMoneyUser(string UUID)
        {
            SOSKey sos;
            Dictionary<string, SOSKey> ds;

            if (!data.TryGetValue(WorldID, out ds))
            {
                ds = new Dictionary<string, SOSKey>();
                data.Add(WorldID, ds);
            }

            if (!ds.TryGetValue(UUID, out sos))
            {
                sos = new SOSKey(false);
                ds.Add(UUID, sos);
            }

            // Update
            sos.saveMoney = !sos.saveMoney;
        }

        public void UpdateLocationUser(string UUID, Vector2 pos, string lastDeathMessage)
        {
            SOSKey sos;
            Dictionary<string, SOSKey> ds;
            if (!data.TryGetValue(WorldID, out ds))
            {
                ds = new Dictionary<string, SOSKey>();
                data.Add(WorldID, new Dictionary<string, SOSKey>());
            }

            if (!ds.TryGetValue(UUID, out sos))
            {
                sos = new SOSKey(false);
                ds.Add(UUID, sos);
            }

            // Update
            sos.lastDeathPlaceX = (int)pos.X;
            sos.lastDeathPlaceY = (int)pos.Y;
            sos.lastDeathMessage = lastDeathMessage;
            sos.deathCount++;
        }

        public void SaveProgress()
        {
            JsonFiler.SaveDataJson(dbpath, data);
        }
    }
}
