using System.Collections.Generic;
using System.Linq;

namespace KerbalConstructionTime
{
    public class KSCItem
    {
        public string KSCName;
        public List<BuildListVessel> VABList = new List<BuildListVessel>();
        public List<BuildListVessel> VABWarehouse = new List<BuildListVessel>();
        public SortedList<string, BuildListVessel> VABPlans = new SortedList<string, BuildListVessel>();
        public List<BuildListVessel> SPHList = new List<BuildListVessel>();
        public List<BuildListVessel> SPHWarehouse = new List<BuildListVessel>();
        public SortedList<string, BuildListVessel> SPHPlans = new SortedList<string, BuildListVessel>();
        public List<FacilityUpgrade> KSCTech = new List<FacilityUpgrade>();
        public List<int> VABUpgrades = new List<int>() { 0 };
        public List<int> SPHUpgrades = new List<int>() { 0 };
        public List<int> RDUpgrades = new List<int>() { 0, 0 }; //research/development
        public List<Recon_Rollout> Recon_Rollout = new List<Recon_Rollout>();
        public List<AirlaunchPrep> AirlaunchPrep = new List<AirlaunchPrep>();
        public List<double> VABRates = new List<double>(), SPHRates = new List<double>();
        public List<double> UpVABRates = new List<double>(), UpSPHRates = new List<double>();

        public List<KCT_LaunchPad> LaunchPads = new List<KCT_LaunchPad>();
        public int ActiveLaunchPadID = 0;

        public KSCItem(string name)
        {
            KSCName = name;
            RDUpgrades[1] = KCTGameStates.TechUpgradesTotal;
            LaunchPads.Add(new KCT_LaunchPad("LaunchPad", Utilities.BuildingUpgradeLevel(SpaceCenterFacility.LaunchPad)));
        }

        public KCT_LaunchPad ActiveLPInstance
        {
            get
            {
                return LaunchPads.Count > ActiveLaunchPadID ? LaunchPads[ActiveLaunchPadID] : null; 
            }
        }

        public int LaunchPadCount
        {
            get
            {
                int count = 0;
                foreach (KCT_LaunchPad lp in LaunchPads)
                    if (lp.level >= 0) count++;
                return count;
            }
        }

        public Recon_Rollout GetReconditioning(string launchSite = "LaunchPad")
        {
            return Recon_Rollout.FirstOrDefault(r => r.launchPadID == launchSite && ((IKCTBuildItem)r).GetItemName() == "LaunchPad Reconditioning");
        }

        public Recon_Rollout GetReconRollout(Recon_Rollout.RolloutReconType type, string launchSite = "LaunchPad")
        {
            return Recon_Rollout.FirstOrDefault(r => r.RRType == type && r.launchPadID == launchSite);
        }

        public void RecalculateBuildRates()
        {
            VABRates.Clear();
            SPHRates.Clear();
            double rate = 0.1;
            int index = 0;
            while (rate > 0)
            {
                rate = MathParser.ParseBuildRateFormula(BuildListVessel.ListType.VAB, index, this);
                if (rate >= 0)
                    VABRates.Add(rate);
                index++;
            }
            rate = 0.1;
            index = 0;
            while (rate > 0)
            {
                rate = MathParser.ParseBuildRateFormula(BuildListVessel.ListType.SPH, index, this);
                if (rate >= 0)
                    SPHRates.Add(rate);
                index++;
            }

            KCTDebug.Log("VAB Rates:");
            foreach (double v in VABRates)
            {
                KCTDebug.Log(v);
            }

            KCTDebug.Log("SPH Rates:");
            foreach (double v in SPHRates)
            {
                KCTDebug.Log(v);
            }
        }

        public void RecalculateUpgradedBuildRates()
        {
            UpVABRates.Clear();
            UpSPHRates.Clear();
            double rate = 0.1;
            int index = 0;
            while (rate > 0)
            {
                rate = MathParser.ParseBuildRateFormula(BuildListVessel.ListType.VAB, index, this, true);
                if (rate >= 0 && (index == 0 || VABRates[index - 1] > 0))
                    UpVABRates.Add(rate);
                else
                    break;
                index++;
            }
            rate = 0.1;
            index = 0;
            while (rate > 0)
            {
                rate = MathParser.ParseBuildRateFormula(BuildListVessel.ListType.SPH, index, this, true);
                if (rate >= 0 && (index == 0 || SPHRates[index - 1] > 0))
                    UpSPHRates.Add(rate);
                else
                    break;
                index++;
            }
        }

        public void SwitchToPrevLaunchPad()
        {
            SwitchLaunchPad(false);
        }

        public void SwitchToNextLaunchPad()
        {
            SwitchLaunchPad(true);
        }

        public void SwitchLaunchPad(bool forwardDirection)
        {
            if (KCTGameStates.ActiveKSC.LaunchPadCount < 2) return;

            int activePadCount = LaunchPads.Count(p => p.level >= 0);
            if (activePadCount < 2) return;

            int idx = KCTGameStates.ActiveKSC.ActiveLaunchPadID;
            KCT_LaunchPad pad;
            do
            {
                if (forwardDirection)
                {
                    idx = (idx + 1) % LaunchPads.Count;
                }
                else
                {
                    //Simple fix for mod function being "weird" in the negative direction
                    //http://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
                    idx = ((idx - 1) % LaunchPads.Count + LaunchPads.Count) % LaunchPads.Count;
                }
                pad = LaunchPads[idx];
            } while (pad.level < 0);

            KCTGameStates.ActiveKSC.SwitchLaunchPad(idx);
        }

        public void SwitchLaunchPad(int LP_ID, bool updateDestrNode = true)
        {
            //set the active LP's new state
            //activate new pad

            if (updateDestrNode)
                ActiveLPInstance?.RefreshDestructionNode();

            LaunchPads[LP_ID].SetActive();
        }

        /// <summary>
        /// Finds the highest level LaunchPad on the KSC
        /// </summary>
        /// <returns>The instance of the highest level LaunchPad</returns>
        public KCT_LaunchPad GetHighestLevelLaunchPad()
        {
            KCT_LaunchPad highest = LaunchPads[0];
            for (int i = LaunchPads.Count - 1; i >= 0; i--)
            {
                KCT_LaunchPad pad = LaunchPads[i];
            
            //foreach (KCT_LaunchPad pad in LaunchPads)
            //{
                if (pad.level > highest.level)
                {
                    highest = pad;
                }
            }
            return highest;
        }

        public ConfigNode AsConfigNode()
        {
            KCTDebug.Log("Saving KSC "+KSCName);
            ConfigNode node = new ConfigNode("KSC");
            node.AddValue("KSCName", KSCName);
            node.AddValue("ActiveLPID", ActiveLaunchPadID);
            
            ConfigNode vabup = new ConfigNode("VABUpgrades");
            foreach (int upgrade in VABUpgrades)
            {
                vabup.AddValue("Upgrade", upgrade.ToString());
            }
            node.AddNode(vabup);
            
            ConfigNode sphup = new ConfigNode("SPHUpgrades");
            foreach (int upgrade in SPHUpgrades)
            {
                sphup.AddValue("Upgrade", upgrade.ToString());
            }
            node.AddNode(sphup);
            
            ConfigNode rdup = new ConfigNode("RDUpgrades");
            foreach (int upgrade in RDUpgrades)
            {
                rdup.AddValue("Upgrade", upgrade.ToString());
            }
            node.AddNode(rdup);
            
            ConfigNode vabl = new ConfigNode("VABList");
            foreach (BuildListVessel blv in VABList)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                vabl.AddNode(cnTemp);
            }
            node.AddNode(vabl);
            
            ConfigNode sphl = new ConfigNode("SPHList");
            foreach (BuildListVessel blv in SPHList)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                sphl.AddNode(cnTemp);
            }
            node.AddNode(sphl);
            
            ConfigNode vabwh = new ConfigNode("VABWarehouse");
            foreach (BuildListVessel blv in VABWarehouse)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                vabwh.AddNode(cnTemp);
            }
            node.AddNode(vabwh);
            
            ConfigNode sphwh = new ConfigNode("SPHWarehouse");
            foreach (BuildListVessel blv in SPHWarehouse)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                sphwh.AddNode(cnTemp);
            }
            node.AddNode(sphwh);

            ConfigNode upgradeables = new ConfigNode("KSCTech");
            foreach (FacilityUpgrade buildingTech in KSCTech)
            {
                ConfigNode bT = new ConfigNode("UpgradingBuilding");
                buildingTech.Save(bT);
                upgradeables.AddNode(bT);
            }
            node.AddNode(upgradeables);

            ConfigNode vabplans = new ConfigNode("VABPlans");
            foreach (BuildListVessel blv in VABPlans.Values)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                vabplans.AddNode(cnTemp);
            }
            node.AddNode(vabplans);

            ConfigNode sphplans = new ConfigNode("SPHPlans");
            foreach (BuildListVessel blv in SPHPlans.Values)
            {
                KCT_BuildListStorage.BuildListItem ship = new KCT_BuildListStorage.BuildListItem();
                ship.FromBuildListVessel(blv);
                ConfigNode cnTemp = new ConfigNode("KCTVessel");
                cnTemp = ConfigNode.CreateConfigFromObject(ship, cnTemp);
                ConfigNode shipNode = new ConfigNode("ShipNode");
                blv.ShipNode.CopyTo(shipNode);
                cnTemp.AddNode(shipNode);
                sphplans.AddNode(cnTemp);
            }
            node.AddNode(sphplans);

            ConfigNode RRCN = new ConfigNode("Recon_Rollout");
            foreach (Recon_Rollout rr in Recon_Rollout)
            {
                ConfigNode rrCN = new ConfigNode("Recon_Rollout_Item");
                rrCN = ConfigNode.CreateConfigFromObject(rr, rrCN);
                RRCN.AddNode(rrCN);
            }
            node.AddNode(RRCN);

            ConfigNode APCN = new ConfigNode("Airlaunch_Prep");
            foreach (AirlaunchPrep ap in AirlaunchPrep)
            {
                ConfigNode cn = new ConfigNode("Airlaunch_Prep_Item");
                cn = ConfigNode.CreateConfigFromObject(ap, cn);
                APCN.AddNode(cn);
            }
            node.AddNode(APCN);

            ConfigNode LPs = new ConfigNode("LaunchPads");
            foreach (KCT_LaunchPad lp in LaunchPads)
            {
                ConfigNode lpCN = lp.AsConfigNode();
                lpCN.AddNode(lp.DestructionNode);
                LPs.AddNode(lpCN);
            }
            node.AddNode(LPs);

            //Cache the regular rates
            ConfigNode CachedVABRates = new ConfigNode("VABRateCache");
            foreach (double rate in VABRates)
            {
                CachedVABRates.AddValue("rate", rate);
            }
            node.AddNode(CachedVABRates);

            ConfigNode CachedSPHRates = new ConfigNode("SPHRateCache");
            foreach (double rate in SPHRates)
            {
                CachedSPHRates.AddValue("rate", rate);
            }
            node.AddNode(CachedSPHRates);
            return node;
        }

        public KSCItem FromConfigNode(ConfigNode node)
        {
            VABUpgrades.Clear();
            SPHUpgrades.Clear();
            RDUpgrades.Clear();
            VABList.Clear();
            VABWarehouse.Clear();
            SPHList.Clear();
            SPHWarehouse.Clear();
            VABPlans.Clear();
            SPHPlans.Clear();
            KSCTech.Clear();
            Recon_Rollout.Clear();
            AirlaunchPrep.Clear();
            VABRates.Clear();
            SPHRates.Clear();

            KSCName = node.GetValue("KSCName");
            if (!int.TryParse(node.GetValue("ActiveLPID"), out ActiveLaunchPadID))
                ActiveLaunchPadID = 0;
            ConfigNode vabup = node.GetNode("VABUpgrades");
            foreach (string upgrade in vabup.GetValues("Upgrade"))
            {
                VABUpgrades.Add(int.Parse(upgrade));
            }
            ConfigNode sphup = node.GetNode("SPHUpgrades");
            foreach (string upgrade in sphup.GetValues("Upgrade"))
            {
                SPHUpgrades.Add(int.Parse(upgrade));
            }
            ConfigNode rdup = node.GetNode("RDUpgrades");
            foreach (string upgrade in rdup.GetValues("Upgrade"))
            {
                RDUpgrades.Add(int.Parse(upgrade));
            }

            ConfigNode tmp = node.GetNode("VABList");
            foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
            {
                KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                ConfigNode.LoadObjectFromConfig(listItem, vessel);
                BuildListVessel blv = listItem.ToBuildListVessel();
                blv.ShipNode = vessel.GetNode("ShipNode");
                blv.KSC = this;
                VABList.Add(blv);
            }

            tmp = node.GetNode("SPHList");
            foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
            {
                KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                ConfigNode.LoadObjectFromConfig(listItem, vessel);
                BuildListVessel blv = listItem.ToBuildListVessel();
                blv.ShipNode = vessel.GetNode("ShipNode");
                blv.KSC = this;
                SPHList.Add(blv);
            }

            tmp = node.GetNode("VABWarehouse");
            foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
            {
                KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                ConfigNode.LoadObjectFromConfig(listItem, vessel);
                BuildListVessel blv = listItem.ToBuildListVessel();
                blv.ShipNode = vessel.GetNode("ShipNode");
                blv.KSC = this;
                VABWarehouse.Add(blv);
            }

            tmp = node.GetNode("SPHWarehouse");
            foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
            {
                KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                ConfigNode.LoadObjectFromConfig(listItem, vessel);
                BuildListVessel blv = listItem.ToBuildListVessel();
                blv.ShipNode = vessel.GetNode("ShipNode");
                blv.KSC = this;
                SPHWarehouse.Add(blv);
            }

            if (node.TryGetNode("VABPlans", ref tmp))
            {
                if (tmp.HasNode("KCTVessel"))
                foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
                {
                    KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig(listItem, vessel);
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.ShipNode = vessel.GetNode("ShipNode");
                    blv.KSC = this;
                    if (VABPlans.ContainsKey(blv.ShipName))
                        VABPlans.Remove(blv.ShipName);
                    
                    VABPlans.Add(blv.ShipName, blv);
                }
            }

            if (node.TryGetNode("SPHPlans", ref tmp))
            {
                if (tmp.HasNode("KCTVessel"))
                foreach (ConfigNode vessel in tmp.GetNodes("KCTVessel"))
                {
                    KCT_BuildListStorage.BuildListItem listItem = new KCT_BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig(listItem, vessel);
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.ShipNode = vessel.GetNode("ShipNode");
                    blv.KSC = this;
                    if (SPHPlans.ContainsKey(blv.ShipName))
                        SPHPlans.Remove(blv.ShipName);
                    SPHPlans.Add(blv.ShipName, blv);
                }
            }

            tmp = node.GetNode("Recon_Rollout");
            foreach (ConfigNode RRCN in tmp.GetNodes("Recon_Rollout_Item"))
            {
                Recon_Rollout tempRR = new Recon_Rollout();
                ConfigNode.LoadObjectFromConfig(tempRR, RRCN);
                Recon_Rollout.Add(tempRR);
            }

            if (node.TryGetNode("Airlaunch_Prep", ref tmp))
            {
                foreach (ConfigNode APCN in tmp.GetNodes("Airlaunch_Prep_Item"))
                {
                    AirlaunchPrep temp = new AirlaunchPrep();
                    ConfigNode.LoadObjectFromConfig(temp, APCN);
                    AirlaunchPrep.Add(temp);
                }
            }

            if (node.HasNode("KSCTech"))
            {
                tmp = node.GetNode("KSCTech");
                foreach (ConfigNode upBuild in tmp.GetNodes("UpgradingBuilding"))
                {
                    FacilityUpgrade tempUP = new FacilityUpgrade();
                    tempUP.Load(upBuild);
                    KSCTech.Add(tempUP);
                }
            }

            if (node.HasNode("LaunchPads"))
            {
                LaunchPads.Clear();
                tmp = node.GetNode("LaunchPads");
                foreach (ConfigNode LP in tmp.GetNodes("KCT_LaunchPad"))
                {
                    KCT_LaunchPad tempLP = new KCT_LaunchPad("LP0");
                    ConfigNode.LoadObjectFromConfig(tempLP, LP);
                    tempLP.DestructionNode = LP.GetNode("DestructionState");
                    LaunchPads.Add(tempLP);
                }
            }

            if (node.HasNode("VABRateCache"))
            {
                foreach (string rate in node.GetNode("VABRateCache").GetValues("rate"))
                {
                    if (double.TryParse(rate, out double r))
                    {
                        VABRates.Add(r);
                    }
                }
            }

            if (node.HasNode("SPHRateCache"))
            {
                foreach (string rate in node.GetNode("SPHRateCache").GetValues("rate"))
                {
                    if (double.TryParse(rate, out double r))
                    {
                        SPHRates.Add(r);
                    }
                }
            }

            return this;
        }
    }
}

/*
    KerbalConstructionTime (c) by Michael Marvin, Zachary Eck

    KerbalConstructionTime is licensed under a
    Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

    You should have received a copy of the license along with this
    work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.
*/
