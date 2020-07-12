using System;
using System.Collections.Generic;
using UnityEngine;
using Upgradeables;

namespace KerbalConstructionTime
{
    public class FacilityUpgrade : IKCTBuildItem, IConfigNode
    {
        [Persistent]
        private string sFacilityType;   // Apparently KSP does not know how to persist nullable enums
        public SpaceCenterFacility? facilityType;

        [Persistent]
        public int upgradeLevel, currentLevel, launchpadID = 0;

        [Persistent]
        public string id, commonName;

        [Persistent]
        public double progress = 0, BP = 0, cost = 0;

        [Persistent]
        public bool UpgradeProcessed = false, isLaunchpad = false;

        private KSCItem _ksc = null;

        public FacilityUpgrade()
        {
        }

        public FacilityUpgrade(SpaceCenterFacility? type, string facilityID, int newLevel, int oldLevel, string name)
        {
            facilityType = type;
            id = facilityID;
            upgradeLevel = newLevel;
            currentLevel = oldLevel;
            commonName = name;

            KCTDebug.Log(string.Format("Upgrade of {0} requested from {1} to {2}", name, oldLevel, newLevel));
        }

        public void Downgrade()
        {
            KCTDebug.Log("Downgrading " + commonName + " to level " + currentLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = currentLevel;
                if (KCTGameStates.ActiveKSCName != KSC.KSCName || KCTGameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    return;
                }
            }
            foreach (UpgradeableFacility facility in GetFacilityReferences())
            {
                KCTEvents.AllowedToUpgrade = true;
                facility.SetLevel(currentLevel);
            }
        }

        public void Upgrade()
        {
            KCTDebug.Log("Upgrading " + commonName + " to level " + upgradeLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = upgradeLevel;
                KSC.LaunchPads[launchpadID].DestructionNode = new ConfigNode("DestructionState");
                if (KCTGameStates.ActiveKSCName != KSC.KSCName || KCTGameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    UpgradeProcessed = true;
                    return;
                }
                KSC.LaunchPads[launchpadID].Upgrade(upgradeLevel);
            }
            KCTEvents.AllowedToUpgrade = true;
            foreach (UpgradeableFacility facility in GetFacilityReferences())
            {
                facility.SetLevel(upgradeLevel);
            }
            int newLvl = Utilities.BuildingUpgradeLevel(id);
            UpgradeProcessed = (newLvl == upgradeLevel);

            KCTDebug.Log($"Upgrade processed: {UpgradeProcessed} Current: {newLvl} Desired: {upgradeLevel}");
        }

        public List<UpgradeableFacility> GetFacilityReferences()
        {
            return ScenarioUpgradeableFacilities.protoUpgradeables[id].facilityRefs;
        }

        public void SetBP(double cost)
        {
            BP = CalculateBP(cost, facilityType);
        }

        public bool AlreadyInProgress()
        {
            return KSC != null;
        }

        public KSCItem KSC
        {
            get
            {
                if (_ksc == null)
                {
                    if (!isLaunchpad)
                        _ksc = KCTGameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id) != null);
                    else
                        _ksc = KCTGameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id && ub.isLaunchpad && ub.launchpadID == this.launchpadID) != null);
                }
                return _ksc;
            }
        }

        public string GetItemName() => commonName;

        public double GetBuildRate()
        {
            double rateTotal = 0;
            if (KSC != null)
            {
                rateTotal = Utilities.GetBothBuildRateSum(KSC);
            }
            return rateTotal;
        }

        public double GetTimeLeft() => (BP - progress) / ((IKCTBuildItem)this).GetBuildRate();

        public bool IsComplete() => progress >= BP;

        public BuildListVessel.ListType GetListType() => BuildListVessel.ListType.KSC;

        public void IncrementProgress(double UTDiff)
        {
            if (!IsComplete()) AddProgress(GetBuildRate() * UTDiff);
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && (IsComplete() || !PresetManager.Instance.ActivePreset.generalSettings.KSCUpgradeTimes))
            {
                if (ScenarioUpgradeableFacilities.Instance != null && KCTGameStates.ErroredDuringOnLoad.OnLoadFinished)
                {
                    Upgrade();

                    try
                    {
                        KCTEvents.OnFacilityUpgradeComplete?.Fire(this);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public static double CalculateBP(double cost, SpaceCenterFacility? facilityType)
        {
            int isAdm = 0, isAC = 0, isLP = 0, isMC = 0, isRD = 0, isRW = 0, isTS = 0, isSPH = 0, isVAB = 0, isOther = 0;
            switch (facilityType)
            {
                case SpaceCenterFacility.Administration:
                    isAdm = 1;
                    break;
                case SpaceCenterFacility.AstronautComplex:
                    isAC = 1;
                    break;
                case SpaceCenterFacility.LaunchPad:
                    isLP = 1;
                    break;
                case SpaceCenterFacility.MissionControl:
                    isMC = 1;
                    break;
                case SpaceCenterFacility.ResearchAndDevelopment:
                    isRD = 1;
                    break;
                case SpaceCenterFacility.Runway:
                    isRW = 1;
                    break;
                case SpaceCenterFacility.TrackingStation:
                    isTS = 1;
                    break;
                case SpaceCenterFacility.SpaceplaneHangar:
                    isSPH = 1;
                    break;
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    isVAB = 1;
                    break;
                default:
                    isOther = 1;
                    break;
            }

            var variables = new Dictionary<string, string>()
            {
                { "C", cost.ToString() },
                { "O", PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString() },
                { "Adm", isAdm.ToString() },
                { "AC", isAC.ToString() },
                { "LP", isLP.ToString() },
                { "MC", isMC.ToString() },
                { "RD", isRD.ToString() },
                { "RW", isRW.ToString() },
                { "TS", isTS.ToString() },
                { "SPH", isSPH.ToString() },
                { "VAB", isVAB.ToString() },
                { "Other", isOther.ToString() }
            };

            double bp = MathParser.GetStandardFormulaValue("KSCUpgrade", variables);
            if (bp <= 0) { bp = 1; }

            return bp;
        }

        public static double CalculateBuildTime(double cost, SpaceCenterFacility? facilityType, KSCItem KSC = null)
        {
            double bp = CalculateBP(cost, facilityType);
            double rateTotal = Utilities.GetBothBuildRateSum(KSC ?? KCTGameStates.ActiveKSC);

            return bp / rateTotal;
        }

        public void Load(ConfigNode node)
        {
            if (ConfigNode.LoadObjectFromConfig(this, node))
            {
                // KSP doesn't support automatically persisting nullable enum values.
                // The following code is a workaround for that.
                try
                {
                    string oldFacilityType = node.GetValue("facilityType");
                    if (!string.IsNullOrEmpty(oldFacilityType))
                    {
                        // Some legacy saves may have facilityType enum directly persisted as a string value.
                        facilityType = (SpaceCenterFacility)Enum.Parse(typeof(SpaceCenterFacility), oldFacilityType);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sFacilityType))
                        {
                            facilityType = (SpaceCenterFacility)Enum.Parse(typeof(SpaceCenterFacility), sFacilityType);
                        }
                        else
                        {
                            facilityType = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void Save(ConfigNode node)
        {
            sFacilityType = facilityType?.ToString();
            ConfigNode.CreateConfigFromObject(this, node);
        }

        private void AddProgress(double amt)
        {
            progress += amt;
            if (progress > BP) progress = BP;
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
