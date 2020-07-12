﻿using MagiCore;
using System.Collections.Generic;

namespace KerbalConstructionTime
{
    public class MathParser
    {
        public static double GetStandardFormulaValue(string formulaName, Dictionary<string, string> variables)
        {
            switch (formulaName)
            {
                case "Node": return MathParsing.ParseMath("KCT_NODE", PresetManager.Instance.ActivePreset.formulaSettings.NodeFormula, variables);
                case "UpgradeFunds": return MathParsing.ParseMath("KCT_UPGRADE_FUNDS", PresetManager.Instance.ActivePreset.formulaSettings.UpgradeFundsFormula, variables);
                case "UpgradesForScience": return MathParsing.ParseMath("KCT_UPGRADES_FOR_SCIENCE", PresetManager.Instance.ActivePreset.formulaSettings.UpgradesForScience, variables);
                case "Research": return MathParsing.ParseMath("KCT_RESEARCH", PresetManager.Instance.ActivePreset.formulaSettings.ResearchFormula, variables);
                case "EffectivePart": return MathParsing.ParseMath("KCT_EFFECTIVE_PART", PresetManager.Instance.ActivePreset.formulaSettings.EffectivePartFormula, variables);
                case "ProceduralPart": return MathParsing.ParseMath("KCT_PROCEDURAL_PART", PresetManager.Instance.ActivePreset.formulaSettings.ProceduralPartFormula, variables);
                case "BP": return MathParsing.ParseMath("KCT_BP", PresetManager.Instance.ActivePreset.formulaSettings.BPFormula, variables);
                case "KSCUpgrade": return MathParsing.ParseMath("KCT_KSC_UPGRADE", PresetManager.Instance.ActivePreset.formulaSettings.KSCUpgradeFormula, variables);
                case "Reconditioning": return MathParsing.ParseMath("KCT_RECONDITIONING", PresetManager.Instance.ActivePreset.formulaSettings.ReconditioningFormula, variables);
                case "BuildRate": return MathParsing.ParseMath("KCT_BUILD_RATE", PresetManager.Instance.ActivePreset.formulaSettings.BuildRateFormula, variables);
                case "UpgradeReset": return MathParsing.ParseMath("KCT_UPGRADE_RESET", PresetManager.Instance.ActivePreset.formulaSettings.UpgradeResetFormula, variables);
                case "InventorySales": return MathParsing.ParseMath("KCT_INVENTORY_SALES", PresetManager.Instance.ActivePreset.formulaSettings.InventorySaleFormula, variables);
                case "IntegrationTime": return MathParsing.ParseMath("KCT_INTEGRATION_TIME", PresetManager.Instance.ActivePreset.formulaSettings.IntegrationTimeFormula, variables);
                case "IntegrationCost": return MathParsing.ParseMath("KCT_INTEGRATION_COST", PresetManager.Instance.ActivePreset.formulaSettings.IntegrationCostFormula, variables);
                case "RolloutCost": return MathParsing.ParseMath("KCT_ROLLOUT_COST", PresetManager.Instance.ActivePreset.formulaSettings.RolloutCostFormula, variables);
                case "NewLaunchPadCost": return MathParsing.ParseMath("KCT_NEW_LAUNCHPAD_COST", PresetManager.Instance.ActivePreset.formulaSettings.NewLaunchPadCostFormula, variables);
                case "RushCost": return MathParsing.ParseMath("KCT_RUSH_COST", PresetManager.Instance.ActivePreset.formulaSettings.RushCostFormula, variables);
                case "AirlaunchCost": return MathParsing.ParseMath("KCT_AIRLAUNCH_COST", PresetManager.Instance.ActivePreset.formulaSettings.AirlaunchCostFormula, variables);
                case "AirlaunchTime": return MathParsing.ParseMath("KCT_AIRLAUNCH_TIME", PresetManager.Instance.ActivePreset.formulaSettings.AirlaunchTimeFormula, variables);
                default: return 0;
            }
        }

        public static double ParseBuildRateFormula(BuildListVessel.ListType type, int index, KSCItem KSC, bool UpgradedRates = false)
        {
            return ParseBuildRateFormula(type, index, KSC, UpgradedRates ? 1 : 0);
        }

        public static double ParseBuildRateFormula(BuildListVessel.ListType type, int index, KSCItem KSC, int upgradeDelta)
        {
            //N = num upgrades, I = rate index, L = VAB/SPH upgrade level, R = R&D level
            int level = 0, upgrades = 0;
            var variables = new Dictionary<string, string>();
            if (type == BuildListVessel.ListType.VAB)
            {
                level = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                if (KSC.VABUpgrades.Count > index)
                    upgrades = KSC.VABUpgrades[index];
            }
            else if (type == BuildListVessel.ListType.SPH)
            {
                level = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.SpaceplaneHangar);
                if (KSC.SPHUpgrades.Count > index)
                    upgrades = KSC.SPHUpgrades[index];
            }
            upgrades += upgradeDelta;
            variables.Add("L", level.ToString());
            variables.Add("LM", level.ToString());
            variables.Add("N", upgrades.ToString());
            variables.Add("I", index.ToString());
            variables.Add("R", Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment).ToString());
            int numNodes = 0;
            if (ResearchAndDevelopment.Instance != null)
                numNodes = ResearchAndDevelopment.Instance.snapshot.GetData().GetNodes("Tech").Length;
            variables.Add("S", numNodes.ToString());

            AddCrewVariables(variables);

            return GetStandardFormulaValue("BuildRate", variables);
        }

        public static double ParseNodeRateFormula(double ScienceValue, int index = 0, bool UpgradedRates = false)
        {
            return ParseNodeRateFormula(ScienceValue, index, UpgradedRates ? 1 : 0);
        }

        public static double ParseNodeRateFormula(double ScienceValue, int index, int upgradeDelta)
        {
            int RnDLvl = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment);
            int RnDMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.ResearchAndDevelopment);
            int upgrades = KCTGameStates.TechUpgradesTotal + upgradeDelta;
            var variables = new Dictionary<string, string>
            {
                { "S", ScienceValue.ToString() },
                { "N", upgrades.ToString() },
                { "R", RnDLvl.ToString() },
                { "RM", RnDMax.ToString() },
                { "O", PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString() },
                { "I", index.ToString() }
            };

            AddCrewVariables(variables);

            return GetStandardFormulaValue("Node", variables);
        }

        public static double ParseRolloutCostFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled || !PresetManager.Instance.ActivePreset.generalSettings.ReconditioningTimes)
                return 0;

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            return GetStandardFormulaValue("RolloutCost", variables);
        }

        public static double ParseIntegrationCostFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled ||
                string.IsNullOrEmpty(PresetManager.Instance.ActivePreset.formulaSettings.IntegrationCostFormula) ||
                PresetManager.Instance.ActivePreset.formulaSettings.IntegrationCostFormula == "0")
            {
                return 0;
            }

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            return GetStandardFormulaValue("IntegrationCost", variables);
        }

        public static double ParseIntegrationTimeFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled ||
                string.IsNullOrEmpty(PresetManager.Instance.ActivePreset.formulaSettings.IntegrationTimeFormula) ||
                PresetManager.Instance.ActivePreset.formulaSettings.IntegrationTimeFormula == "0")
            {
                return 0;
            }

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            return GetStandardFormulaValue("IntegrationTime", variables);
        }

        public static double ParseRushCostFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled ||
                string.IsNullOrEmpty(PresetManager.Instance.ActivePreset.formulaSettings.RushCostFormula))
            {
                return 0;
            }

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            variables.Add("TC", vessel.GetTotalCost().ToString());
            variables.Add("RC", vessel.RushBuildClicks.ToString());
            return GetStandardFormulaValue("RushCost", variables);
        }

        public static double ParseAirlaunchCostFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled ||
                string.IsNullOrEmpty(PresetManager.Instance.ActivePreset.formulaSettings.AirlaunchCostFormula))
            {
                return 0;
            }

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            return GetStandardFormulaValue("AirlaunchCost", variables);
        }

        public static double ParseAirlaunchTimeFormula(BuildListVessel vessel)
        {
            if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled ||
                string.IsNullOrEmpty(PresetManager.Instance.ActivePreset.formulaSettings.AirlaunchTimeFormula))
            {
                return 0;
            }

            Dictionary<string, string> variables = GetIntegrationRolloutVariables(vessel);
            return GetStandardFormulaValue("AirlaunchTime", variables);
        }

        private static Dictionary<string, string> GetIntegrationRolloutVariables(BuildListVessel vessel)
        {
            double loadedMass, emptyMass, loadedCost, emptyCost;
            loadedCost = vessel.Cost;
            emptyCost = vessel.EmptyCost;
            loadedMass = vessel.GetTotalMass();
            emptyMass = vessel.EmptyMass;

            int EditorLevel = 0, LaunchSiteLvl = 0, EditorMax = 0, LaunchSiteMax = 0;
            int isVABVessel = 0;
            if (vessel.Type == BuildListVessel.ListType.None)
                vessel.FindTypeFromLists();
            if (vessel.Type == BuildListVessel.ListType.VAB)
            {
                EditorLevel = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                LaunchSiteLvl = KCTGameStates.ActiveKSC.ActiveLPInstance.level;//KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.LaunchPad);
                EditorMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                LaunchSiteMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.LaunchPad);
                isVABVessel = 1;
            }
            else if (vessel.Type == BuildListVessel.ListType.SPH)
            {
                EditorLevel = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.SpaceplaneHangar);
                LaunchSiteLvl = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.Runway);
                EditorMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.SpaceplaneHangar);
                LaunchSiteMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.Runway);
            }
            double BP = vessel.BuildPoints;
            double OverallMult = PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier;

            var variables = new Dictionary<string, string>
            {
                { "M", loadedMass.ToString() },
                { "m", emptyMass.ToString() },
                { "C", loadedCost.ToString() },
                { "c", emptyCost.ToString() },
                { "VAB", isVABVessel.ToString() },
                { "E", vessel.EffectiveCost.ToString() },
                { "BP", BP.ToString() },
                { "L", LaunchSiteLvl.ToString() },
                { "LM", LaunchSiteMax.ToString() },
                { "EL", EditorLevel.ToString() },
                { "ELM", EditorMax.ToString() },
                { "O", OverallMult.ToString() },
                { "SN", vessel.NumStages.ToString() },
                { "SP", vessel.NumStageParts.ToString() },
                { "SC", vessel.StagePartCost.ToString() }
            };

            AddCrewVariables(variables);

            return variables;
        }

        public static double ParseReconditioningFormula(BuildListVessel vessel, bool isReconditioning)
        {

            double loadedMass, emptyMass, loadedCost, emptyCost;
            loadedCost = vessel.Cost;
            emptyCost = vessel.EmptyCost;
            loadedMass = vessel.GetTotalMass();
            emptyMass = vessel.EmptyMass;

            int EditorLevel, LaunchSiteLvl, EditorMax, LaunchSiteMax;
            int isVABVessel = 0;
            if (vessel.Type == BuildListVessel.ListType.VAB)
            {
                EditorLevel = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                LaunchSiteLvl = KCTGameStates.ActiveKSC.ActiveLPInstance.level;
                EditorMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
                LaunchSiteMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.LaunchPad);
                isVABVessel = 1;
            }
            else
            {
                EditorLevel = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.SpaceplaneHangar);
                LaunchSiteLvl = Utilities.BuildingUpgradeLevel(SpaceCenterFacility.Runway);
                EditorMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.SpaceplaneHangar);
                LaunchSiteMax = Utilities.BuildingUpgradeMaxLevel(SpaceCenterFacility.Runway);
            }
            double BP = vessel.BuildPoints;
            double OverallMult = PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier;
            double ReconEffect = PresetManager.Instance.ActivePreset.timeSettings.ReconditioningEffect;
            double MaxRecon = PresetManager.Instance.ActivePreset.timeSettings.MaxReconditioning;

            var variables = new Dictionary<string, string>
            {
                { "M", loadedMass.ToString() },
                { "m", emptyMass.ToString() },
                { "C", loadedCost.ToString() },
                { "c", emptyCost.ToString() },
                { "VAB", isVABVessel.ToString() },
                { "BP", BP.ToString() },
                { "L", LaunchSiteLvl.ToString() },
                { "LM", LaunchSiteMax.ToString() },
                { "EL", EditorLevel.ToString() },
                { "ELM", EditorMax.ToString() },
                { "O", OverallMult.ToString() },
                { "E", ReconEffect.ToString() },
                { "X", MaxRecon.ToString() },
                { "RE", (isReconditioning ? 1 : 0).ToString() },
                { "S", PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit.ToString() },
                { "SN", vessel.NumStages.ToString() },
                { "SP", vessel.NumStageParts.ToString() },
                { "SC", vessel.StagePartCost.ToString() }
            };

            AddCrewVariables(variables);

            return GetStandardFormulaValue("Reconditioning", variables);
        }

        public static void AddCrewVariables(Dictionary<string, string> crewVars)
        {
            int pilots=0, engineers=0, scientists=0;
            int pLevels=0, eLevels=0, sLevels=0;

            int pilots_total = 0, engineers_total = 0, scientists_total = 0;
            int pLevels_total = 0, eLevels_total = 0, sLevels_total = 0;

            foreach (ProtoCrewMember pcm in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available || pcm.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                {
                    if (pcm.trait == "Pilot")
                    {
                        if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            pilots++;
                            pLevels += pcm.experienceLevel;
                        }
                        pilots_total++;
                        pLevels_total += pcm.experienceLevel;
                    }
                    else if (pcm.trait == "Engineer")
                    {
                        if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            engineers++;
                            eLevels += pcm.experienceLevel;
                        }
                        engineers_total++;
                        eLevels_total += pcm.experienceLevel;
                    }
                    else if (pcm.trait == "Scientist")
                    {
                        if (pcm.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            scientists++;
                            sLevels += pcm.experienceLevel;
                        }
                        scientists_total++;
                        sLevels_total += pcm.experienceLevel;
                    }
                }
            }

            crewVars.Add("PiK", pilots.ToString());
            crewVars.Add("PiL", pLevels.ToString());

            crewVars.Add("EnK", engineers.ToString());
            crewVars.Add("EnL", eLevels.ToString());

            crewVars.Add("ScK", scientists.ToString());
            crewVars.Add("ScL", sLevels.ToString());

            crewVars.Add("TPiK", pilots_total.ToString());
            crewVars.Add("TPiL", pLevels_total.ToString());

            crewVars.Add("TEnK", engineers_total.ToString());
            crewVars.Add("TEnL", eLevels_total.ToString());

            crewVars.Add("TScK", scientists_total.ToString());
            crewVars.Add("TScL", sLevels_total.ToString());
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
