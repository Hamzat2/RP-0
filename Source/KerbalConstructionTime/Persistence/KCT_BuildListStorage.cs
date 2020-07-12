using System;
using System.Collections.Generic;

namespace KerbalConstructionTime
{
    public class KCT_BuildListStorage : ConfigNodeStorage
    {
        [Persistent]
        List<BuildListItem> VABBuildList = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> SPHBuildList = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> VABWarehouse = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> SPHWarehouse = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> VABPlans = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> SPHPlans = new List<BuildListItem>();

        [Persistent]
        Recon_Rollout LPRecon = new Recon_Rollout();
        [Persistent]
        AirlaunchPrep AirlaunchPrep = new AirlaunchPrep();

        public override void OnDecodeFromConfigNode()
        {
            KCTGameStates.ActiveKSC.VABList.Clear();
            KCTGameStates.ActiveKSC.SPHList.Clear();
            KCTGameStates.ActiveKSC.VABWarehouse.Clear();
            KCTGameStates.ActiveKSC.SPHWarehouse.Clear();
            KCTGameStates.ActiveKSC.VABPlans.Clear();
            KCTGameStates.ActiveKSC.SPHPlans.Clear();
            KCTGameStates.ActiveKSC.Recon_Rollout.Clear();
            KCTGameStates.ActiveKSC.AirlaunchPrep.Clear();

            foreach (BuildListItem b in VABBuildList)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                KCTGameStates.ActiveKSC.VABList.Add(blv);
            }
            foreach (BuildListItem b in SPHBuildList)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                KCTGameStates.ActiveKSC.SPHList.Add(blv);
            }
            foreach (BuildListItem b in VABWarehouse)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                KCTGameStates.ActiveKSC.VABWarehouse.Add(blv);
            }
            foreach (BuildListItem b in SPHWarehouse)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                KCTGameStates.ActiveKSC.SPHWarehouse.Add(blv);
            }

            foreach (BuildListItem b in VABPlans)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                if (KCTGameStates.ActiveKSC.VABPlans.ContainsKey(blv.ShipName))
                    KCTGameStates.ActiveKSC.VABPlans.Remove(blv.ShipName);

                KCTGameStates.ActiveKSC.VABPlans.Add(blv.ShipName, blv);
            }
            foreach (BuildListItem b in SPHPlans)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                if (KCTGameStates.ActiveKSC.SPHPlans.ContainsKey(blv.ShipName))
                    KCTGameStates.ActiveKSC.SPHPlans.Remove(blv.ShipName);

                KCTGameStates.ActiveKSC.SPHPlans.Add(blv.ShipName, blv);
            }

            KCTGameStates.ActiveKSC.Recon_Rollout.Add(LPRecon);
            KCTGameStates.ActiveKSC.AirlaunchPrep.Add(AirlaunchPrep);
        }

        public override void OnEncodeToConfigNode()
        {
            VABBuildList.Clear();
            SPHBuildList.Clear();
            VABWarehouse.Clear();
            SPHWarehouse.Clear();
            VABPlans.Clear();
            SPHPlans.Clear();
        }

        public class BuildListItem
        {
            [Persistent]
            string shipName, shipID;
            [Persistent]
            double progress, effectiveCost, buildTime, integrationTime;
            [Persistent]
            string launchSite, flag;
            [Persistent]
            bool cannotEarnScience;
            [Persistent]
            float cost = 0, integrationCost;
            [Persistent]
            float mass = 0, kscDistance = 0;
            [Persistent]
            int numStageParts = 0, numStages = 0;
            [Persistent]
            double stagePartCost = 0;
            [Persistent]
            int rushBuildClicks = 0;
            [Persistent]
            int EditorFacility = 0, LaunchPadID = -1;
            [Persistent]
            List<string> desiredManifest = new List<string>();

            public BuildListVessel ToBuildListVessel()
            {
                BuildListVessel ret = new BuildListVessel(shipName, launchSite, effectiveCost, buildTime, integrationTime, flag, cost, integrationCost, EditorFacility);
                ret.Progress = progress;
                ret.Id = new Guid(shipID);
                ret.CannotEarnScience = cannotEarnScience;
                ret.TotalMass = mass;
                ret.NumStageParts = numStageParts;
                ret.NumStages = numStages;
                ret.StagePartCost = stagePartCost;
                ret.DistanceFromKSC = kscDistance;
                ret.RushBuildClicks = rushBuildClicks;
                ret.LaunchSiteID = LaunchPadID;
                ret.DesiredManifest = desiredManifest;
                return ret;
            }

            public BuildListItem FromBuildListVessel(BuildListVessel blv)
            {
                progress = blv.Progress;
                effectiveCost = blv.EffectiveCost;
                buildTime = blv.BuildPoints;
                integrationTime = blv.IntegrationPoints;
                launchSite = blv.LaunchSite;
                flag = blv.Flag;
                //shipURL = blv.shipURL;
                shipName = blv.ShipName;
                shipID = blv.Id.ToString();
                cannotEarnScience = blv.CannotEarnScience;
                cost = blv.Cost;
                integrationCost = blv.IntegrationCost;
                rushBuildClicks = blv.RushBuildClicks;
                mass = blv.TotalMass;
                numStageParts = blv.NumStageParts;
                numStages = blv.NumStages;
                stagePartCost = blv.StagePartCost;
                kscDistance = blv.DistanceFromKSC;
                EditorFacility = (int)blv.GetEditorFacility();
                LaunchPadID = blv.LaunchSiteID;
                desiredManifest = blv.DesiredManifest;
                return this;
            }
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
