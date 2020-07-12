using System;
using System.Linq;

namespace KerbalConstructionTime
{
    public class AirlaunchPrep : IKCTBuildItem
    {
        [Persistent] private string name = string.Empty;
        [Persistent] public double BP = 0, progress = 0, cost = 0;
        [Persistent] public string associatedID = string.Empty;

        public const string Name_Mount = "Mounting to carrier";
        public const string Name_Unmount = "Unmounting";

        public enum PrepDirection { Mount, Unmount };
        public PrepDirection Direction = PrepDirection.Mount;

        public BuildListVessel AssociatedBLV
        {
            get
            {
                return Utilities.FindBLVesselByID(new Guid(associatedID));
            }
        }

        public KSCItem KSC
        {
            get
            {
                return KCTGameStates.KSCs.FirstOrDefault(k => k.AirlaunchPrep.Exists(r => r.associatedID == associatedID));
            }
        }

        public AirlaunchPrep()
        {
            name = Name_Mount;
            progress = 0;
            BP = 0;
            cost = 0;
            Direction = PrepDirection.Mount;
            associatedID = "";
        }

        public AirlaunchPrep(BuildListVessel vessel, string id)
        {
            Direction = PrepDirection.Mount;
            associatedID = id;
            progress = 0;

            BP = MathParser.ParseAirlaunchTimeFormula(vessel);
            cost = MathParser.ParseAirlaunchCostFormula(vessel);
            name = Name_Mount;
        }

        public double GetBuildRate()
        {
            double buildRate = Utilities.GetSPHBuildRateSum(KSC);

            if (Direction == PrepDirection.Unmount)
                buildRate *= -1;

            return buildRate;
        }

        public string GetItemName()
        {
            return name;
        }

        public BuildListVessel.ListType GetListType()
        {
            return BuildListVessel.ListType.SPH;
        }

        public double GetTimeLeft()
        {
            double timeLeft = (BP - progress) / GetBuildRate();
            if (Direction == PrepDirection.Unmount)
                timeLeft = (-progress) / GetBuildRate();
            return timeLeft;
        }

        public bool IsComplete()
        {
            bool complete = progress >= BP;
            if (Direction == PrepDirection.Unmount)
                complete = progress <= 0;
            return complete;
        }

        public void IncrementProgress(double UTDiff)
        {
            double progBefore = progress;
            progress += GetBuildRate() * UTDiff;
            if (progress > BP) progress = BP;

            if (Utilities.CurrentGameIsCareer() && Direction == PrepDirection.Mount && cost > 0)
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

        public void SwitchDirection()
        {
            if (Direction == PrepDirection.Mount)
            {
                Direction =  PrepDirection.Unmount;
                name = Name_Unmount;
            }
            else
            {
                Direction = PrepDirection.Mount;
                name = Name_Mount;
            }
        }
    }
}
