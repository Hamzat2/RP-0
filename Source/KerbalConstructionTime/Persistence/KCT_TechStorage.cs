using System.Collections.Generic;

namespace KerbalConstructionTime
{
    public class KCT_TechStorageItem
    {
        [Persistent] string techName, techID;
        [Persistent] int scienceCost;
        [Persistent] double progress;
        [Persistent] List<string> parts;

        public TechItem ToTechItem()
        {
            TechItem ret = new TechItem(techID, techName, progress, scienceCost, parts);
            return ret;
        }

        public KCT_TechStorageItem FromTechItem(TechItem techItem)
        {
            techName = techItem.TechName;
            techID = techItem.TechID;
            progress = techItem.Progress;
            scienceCost = techItem.ScienceCost;
            parts = techItem.UnlockedParts;

            return this;
        }
    }

    public class KCT_TechStorage : ConfigNodeStorage
    {
        [Persistent] List<KCT_TechStorageItem> techBuildList = new List<KCT_TechStorageItem>();

        public override void OnEncodeToConfigNode()
        {
            base.OnEncodeToConfigNode();
            techBuildList.Clear();
            foreach (TechItem tech in KCTGameStates.TechList)
            {
                KCT_TechStorageItem tSI = new KCT_TechStorageItem();
                techBuildList.Add(tSI.FromTechItem(tech));
            }
        }

        public override void OnDecodeFromConfigNode()
        {
            base.OnDecodeFromConfigNode();
            KCTGameStates.TechList.Clear();
            foreach (KCT_TechStorageItem tSI in techBuildList)
            {
                TechItem tI = tSI.ToTechItem();
                KCTGameStates.TechList.Add(tI);
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
