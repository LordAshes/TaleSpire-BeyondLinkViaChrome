using BepInEx;
using BepInEx.Configuration;
using Bounce.TaleSpire.AssetManagement;
using System;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(StatMessaging.Guid)]
    public partial class BeyondLinkViaChromePlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Beyond Link Via Chrome Plug-In";
        public const string Guid = "org.lordashes.plugins.beyondlinkviachrome";
        public const string Version = "1.3.0.0";

        public DateTime lastUpdate = DateTime.UtcNow;

        private string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        private string data = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+@"/CustomData/";

        private int refreshRate = 5000;
        private string[] statSource = new string[9];

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            // Not required but good idea to log this state for troubleshooting purpose
            Debug.Log("Beyond Link Via Chrome Plugin: Active.");

            // Set refresh rate at which plugin checks for updates (sync this valiue with Chrome updates) 
            refreshRate = Config.Bind("Settings", "Refreh Rate (ms)", 5000).Value;

            statSource[0] = Config.Bind("Settings", "HP Slot", "HP.Current,HP.Max").Value;
            statSource[1] = Config.Bind("Settings", "Stat Slot 0", "HD.Used,level").Value;
            statSource[2] = Config.Bind("Settings", "Stat Slot 1", "AC,AC").Value;
            statSource[3] = Config.Bind("Settings", "Stat Slot 2", "Order,Move").Value;
            statSource[4] = Config.Bind("Settings", "Stat Slot 3", "").Value;
            statSource[5] = Config.Bind("Settings", "Stat Slot 4", "").Value;
            statSource[6] = Config.Bind("Settings", "Stat Slot 5", "").Value;
            statSource[7] = Config.Bind("Settings", "Stat Slot 6", "").Value;
            statSource[8] = Config.Bind("Settings", "Stat Slot 7", "").Value;

            // Check to see if the BeyondLinkServer is already running
            System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("BeyondLinkServer");
            if (pname.Length == 0)
            {
                Debug.Log("Beyond Link Via Chrome Plugin: Starting the BeyondLinkServer At "+ location + @"\BeyondLinkServer.exe "+ Config.Bind("Settings", "Beyond Link Server Port", 9100).Value.ToString());
                // Start the BeyondLinkServer
                System.Diagnostics.Process server = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = location+@"\BeyondLinkServer.exe",       // BeyondLinkServer executable
                        Arguments = Config.Bind("Settings", "Beyond Link Server Port", 9100).Value.ToString(), // Port
                        CreateNoWindow = !Config.Bind("Settings", "Show Server Window", true).Value,
                        UseShellExecute = true,
                        WorkingDirectory = location
                    }
                };
                server.Start();
            }
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                if (DateTime.UtcNow.Subtract(lastUpdate).TotalMilliseconds > refreshRate)
                {
                    foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                    {
                        for (int s = -1; s < (statSource.Length-1); s++)
                        {
                            if (statSource[s + 1] != "")
                            {
                                string[] names = statSource[s + 1].Split(',');
                                Debug.Log("Beyond Link Via Chrome Plugin: Looking For " + data + StatMessaging.GetCreatureName(asset) + "." + names[0]+" and "+ data + StatMessaging.GetCreatureName(asset) + "." + names[1]);
                                if (System.IO.File.Exists(data + StatMessaging.GetCreatureName(asset) + "." + names[0]) && System.IO.File.Exists(data + StatMessaging.GetCreatureName(asset) + "." + names[1]))
                                {
                                    Debug.Log("Beyond Link Via Chrome Plugin: Found. Checking For Change");
                                    string current = System.IO.File.ReadAllText(data + StatMessaging.GetCreatureName(asset) + "." + names[0]);
                                    string max = System.IO.File.ReadAllText(data + StatMessaging.GetCreatureName(asset) + "." + names[1]);
                                    CreatureDataV2 cd;
                                    CreatureManager.TryGetCreatureData(asset.Creature.CreatureId, out cd);
                                    bool changed = false;
                                    switch (s)
                                    {
                                        case -1:
                                            if (asset.Creature.Hp.Value != float.Parse(current) || asset.Creature.Hp.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 0:
                                            if (cd.Stat0.Value != float.Parse(current) || cd.Stat0.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 1:
                                            if (cd.Stat1.Value != float.Parse(current) || cd.Stat1.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 2:
                                            if (cd.Stat2.Value != float.Parse(current) || cd.Stat2.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 3:
                                            if (cd.Stat3.Value != float.Parse(current) || cd.Stat3.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 4:
                                            if (cd.Stat4.Value != float.Parse(current) || cd.Stat4.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 5:
                                            if (cd.Stat5.Value != float.Parse(current) || cd.Stat5.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 6:
                                            if (cd.Stat6.Value != float.Parse(current) || cd.Stat6.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                        case 7:
                                            if (cd.Stat7.Value != float.Parse(current) || cd.Stat7.Max != float.Parse(max))
                                            {
                                                changed = true;
                                            }
                                            break;
                                    }
                                    if (changed)
                                    {
                                        Debug.Log("Beyond Link Via Chrome Plugin: Syncing '" + StatMessaging.GetCreatureName(asset) + "." + names[0] + "/" + names[1] + " To Slot "+s);
                                        CreatureStat cs = new CreatureStat(float.Parse(current), float.Parse(max));
                                        CreatureManager.SetCreatureStatByIndex(asset.Creature.CreatureId, cs, s);
                                        // Set creature stats based on how this plugin will update the stats
                                        CampaignSessionManager.SetCreatureStatNames(Config.Bind("Settings","Stat Names", "HD,AC,Move").Value.ToString().Split(','));
                                    }
                                }
                            }
                        }
                    }
                    lastUpdate = DateTime.UtcNow;
                }
            }
        }
    }
}
