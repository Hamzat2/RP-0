using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalConstructionTime
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class GuiDataAndWhitelistItemsDatabase : MonoBehaviour
    {
        internal static List<string> validFuelRes;

        private void Awake()
        {
            validFuelRes = new List<string>();

            var loaders = LoadingScreen.Instance.loaders;
            if (loaders != null)
            {
                for (var i = 0; i < loaders.Count; i++)
                {
                    var loadingSystem = loaders[i];
                    if (loadingSystem is FuelWhitelistLoader)
                    {
                        print("[KCT] found KCT_FuelWhitelistLoader: " + i);
                        (loadingSystem as FuelWhitelistLoader).Done = false;
                        break;
                    }
                    if (loadingSystem is PartLoader)
                    {
                        print("[KCT] found PartLoader: " + i);
                        var go = new GameObject();
                        var recipeLoader = go.AddComponent<FuelWhitelistLoader>();
                        loaders.Insert(i, recipeLoader);
                        break;
                    }
                }
            }
        }
    }

    public class FuelWhitelistLoader : LoadingSystem
    {
        public bool Done = false;

        private IEnumerator LoadCustomItems()
        {
            var nodes = GameDatabase.Instance.GetConfigNodes("KCT_FUEL_RESOURCES");
            if (nodes != null)
            {
                foreach (var configNode in nodes)
                {
                    if (configNode != null)
                    {
                        var items = configNode.GetValuesList("fuelResource");
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                if (item != null)
                                {
                                    if (!GuiDataAndWhitelistItemsDatabase.validFuelRes.Contains(item))
                                        GuiDataAndWhitelistItemsDatabase.validFuelRes.Add(item);
                                }
                            }
                        }
                        yield return null;
                    }
                }
            }

            Done = true;
        }

        public override bool IsReady()
        {
            return Done;
        }

        public override float ProgressFraction()
        {
            return 0;
        }

        public override string ProgressTitle()
        {
            return "KerbalConstructionTime Initialization & Setup";
        }

        public override void StartLoad()
        {
            Done = false;
            StartCoroutine(LoadCustomItems());
        }
    }
}
