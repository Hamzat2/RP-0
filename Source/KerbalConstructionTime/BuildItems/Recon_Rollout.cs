using System;
using System.Linq;

namespace KerbalConstructionTime
{
    public class Recon_Rollout : IKCTBuildItem
    {
        [Persistent] private string name = "";
        [Persistent] public double BP = 0, progress = 0, cost = 0;
        [Persistent] public string associatedID = "";
        [Persistent] public string launchPadID = "LaunchPad";

        public enum RolloutReconType { Reconditioning, Rollout, Rollback, Recovery, None };
        private RolloutReconType _RRTypeInternal = RolloutReconType.None;

        public RolloutReconType RRType
        {
            get
            {
                if (_RRTypeInternal != RolloutReconType.None)
                    return _RRTypeInternal;
                else
                {
                    if (name == "LaunchPad Reconditioning")
                        _RRTypeInternal = RolloutReconType.Reconditioning;
                    else if (name == "Vessel Rollout")
                        _RRTypeInternal = RolloutReconType.Rollout;
                    else if (name == "Vessel Rollback")
                        _RRTypeInternal = RolloutReconType.Rollback;
                    else if (name == "Vessel Recovery")
                        _RRTypeInternal = RolloutReconType.Recovery;
                    return _RRTypeInternal;
                }
            }
            set
            {
                _RRTypeInternal = value;
            }
        }

        public BuildListVessel AssociatedBLV => Utilities.FindBLVesselByID(new Guid(associatedID));

        public KSCItem KSC => KCTGameStates.KSCs.FirstOrDefault(k => k.Recon_Rollout.Exists(r => r.associatedID == associatedID));

        public Recon_Rollout()
        {
            name = "LaunchPad Reconditioning";
            progress = 0;
            BP = 0;
            cost = 0;
            RRType = RolloutReconType.None;
            associatedID = "";
            launchPadID = "LaunchPad";
        }

        public Recon_Rollout(Vessel vessel, RolloutReconType type, string id, string launchSite)
        {
            RRType = type;
            associatedID = id;
            launchPadID = launchSite;
            KCTDebug.Log("New recon_rollout at launchsite: " + launchPadID);
            progress = 0;
            if (type == RolloutReconType.Reconditioning) 
            {
                try
                {
                    BP = MathParser.ParseReconditioningFormula(new BuildListVessel(vessel), true);
                }
                catch
                {
                    KCTDebug.Log("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "LaunchPad Reconditioning";
                }
            }
            else if (type == RolloutReconType.Rollout)
            {
                try
                {
                    BP = MathParser.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    KCTDebug.Log("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Rollout";
                }
            }
            else if (type == RolloutReconType.Rollback)
            {
                try
                {
                    BP = MathParser.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    KCTDebug.Log("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Rollback";
                    progress = BP;
                }
            }
            else if (type == RolloutReconType.Recovery)
            {
                try
                {
                    BP = MathParser.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    KCTDebug.Log("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Recovery";
                    double KSCDistance = (float)SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                    double maxDist = SpaceCenter.Instance.cb.Radius * Math.PI;
                    BP += BP * (KSCDistance / maxDist);
                }
            }
        }

        public Recon_Rollout(BuildListVessel vessel, RolloutReconType type, string id, string launchSite="")
        {
            RRType = type;
            associatedID = id;
            if (launchSite != "") //For when we add custom launchpads
                launchPadID = launchSite;
            else
                launchPadID = vessel.LaunchSite;

            progress = 0;
            if (type == RolloutReconType.Reconditioning)
            {
                BP = MathParser.ParseReconditioningFormula(vessel, true);
                //BP *= (1 - KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit);
                name = "LaunchPad Reconditioning";
            }
            else if (type == RolloutReconType.Rollout)
            {
                BP = MathParser.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                name = "Vessel Rollout";
                cost = MathParser.ParseRolloutCostFormula(vessel);
            }
            else if (type == RolloutReconType.Rollback)
            {
                BP = MathParser.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                progress = BP;
                name = "Vessel Rollback";
            }
            else if (type == RolloutReconType.Recovery)
            {
                BP = MathParser.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                name = "Vessel Recovery";
                double maxDist = SpaceCenter.Instance.cb.Radius * Math.PI;
                BP += BP * (vessel.DistanceFromKSC / maxDist);
            }
        }

        public void SwapRolloutType()
        {
            if (RRType == RolloutReconType.Rollout)
            {
                RRType = RolloutReconType.Rollback;
                name = "Vessel Rollback";
            }
            else if (RRType == RolloutReconType.Rollback)
            {
                RRType = RolloutReconType.Rollout;
                name = "Vessel Rollout";
            }
        }

        public double ProgressPercent()
        {
            return Math.Round(100 * (progress / BP), 2);
        }

        public string GetItemName()
        {
            return name;
        }

        public double GetBuildRate()
        {
            double buildRate;
            if (AssociatedBLV != null && AssociatedBLV.Type == BuildListVessel.ListType.SPH)
                buildRate = Utilities.GetSPHBuildRateSum(KSC);
            else
                buildRate = Utilities.GetVABBuildRateSum(KSC);

            if (RRType == RolloutReconType.Rollback)
                buildRate *= -1;

            return buildRate;
        }

        public double GetTimeLeft()
        {
            double timeLeft = (BP - progress) / ((IKCTBuildItem)this).GetBuildRate();
            if (RRType == RolloutReconType.Rollback)
                timeLeft = (-progress) / ((IKCTBuildItem)this).GetBuildRate();
            return timeLeft;
        }

        public BuildListVessel.ListType GetListType() => BuildListVessel.ListType.Reconditioning;

        public bool IsComplete()
        {
            bool complete = progress >= BP;
            if (RRType == RolloutReconType.Rollback)
                complete = progress <= 0;
            return complete;
        }

        public void IncrementProgress(double UTDiff)
        {
            double progBefore = progress;
            progress += GetBuildRate() * UTDiff;
            if (progress > BP) progress = BP;

            if (Utilities.CurrentGameIsCareer() && RRType == RolloutReconType.Rollout && cost > 0)
            {
                int steps;
                if ((steps = (int)(Math.Floor(progress / BP * 10) - Math.Floor(progBefore / BP * 10))) > 0) //passed 10% of the progress
                {
                    if (Funding.Instance.Funds < cost / 10) //If they can't afford to continue the rollout, progress stops
                    {
                        progress = progBefore;
                        if (TimeWarp.CurrentRate > 1f && KCTGameStates.WarpInitiated && this == KCTGameStates.TargetedItem)
                        {
                            ScreenMessages.PostScreenMessage("Timewarp was stopped because there's insufficient funds to continue the rollout");
                            TimeWarp.SetRate(0, true);
                            KCTGameStates.WarpInitiated = false;
                        }
                    }
                    else
                        Utilities.SpendFunds(cost / 10 * steps, TransactionReasons.VesselRollout);
                }
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
