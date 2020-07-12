using UnityEngine;
using KSP.UI.Screens;
using UnityEngine.UI;

namespace KerbalConstructionTime
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TrackingStationAddon : KerbalConstructionTime
    {
        public Button.ButtonClickedEvent originalCallback, flyCallback;
        Vessel selectedVessel = null;

        public new void Start()
        {
            base.Start();
            if (KCT_GUI.IsPrimarilyDisabled)
                return;

            KCTDebug.Log("KCT_Flight, Start");
            SpaceTracking trackingStation = FindObjectOfType<SpaceTracking>();
            if (trackingStation != null)
            {
                originalCallback = trackingStation.RecoverButton.onClick;
                flyCallback = trackingStation.FlyButton.onClick;

                trackingStation.RecoverButton.onClick = new Button.ButtonClickedEvent();
                trackingStation.RecoverButton.onClick.AddListener(NewRecoveryFunctionTrackingStation);
            }
        }

        private void Fly()
        {
            flyCallback.Invoke();
        }

        private void KCT_Recovery()
        {
            DialogGUIBase[] options = new DialogGUIBase[2];
            options[0] = new DialogGUIButton("Go to Flight scene", Fly);
            options[1] = new DialogGUIButton("Cancel", Cancel);

            MultiOptionDialog diag = new MultiOptionDialog("scrapVesselPopup", "KCT can only recover vessels in the Flight scene", "Recover Vessel", null, options: options);
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), diag, false, HighLogic.UISkin);
        }

        public void RecoverToVAB()
        {
            KCT_Recovery();
        }

        public void RecoverToSPH()
        {
            KCT_Recovery();
        }

        public void DoNormalRecovery()
        {
            originalCallback.Invoke();
        }

        public void Cancel()
        {
            return;
        }

        public void NewRecoveryFunctionTrackingStation()
        {
            selectedVessel = null;
            SpaceTracking trackingStation = (SpaceTracking)FindObjectOfType(typeof(SpaceTracking));
            selectedVessel = trackingStation.SelectedVessel;

            if (selectedVessel == null)
            {
                Debug.Log("[KCT] Error! No Vessel selected.");
                return;
            }

            bool sph = selectedVessel.IsRecoverable && selectedVessel.IsClearToSave() == ClearToSaveStatus.CLEAR;

            string reqTech = PresetManager.Instance.ActivePreset.generalSettings.VABRecoveryTech;
            bool vab = selectedVessel.IsRecoverable &&
                       selectedVessel.IsClearToSave() == ClearToSaveStatus.CLEAR &&
                       (selectedVessel.situation == Vessel.Situations.PRELAUNCH ||
                        string.IsNullOrEmpty(reqTech) ||
                        ResearchAndDevelopment.GetTechnologyState(reqTech) == RDTech.State.Available);

            int cnt = 2;
            if (sph) cnt++;
            if (vab) cnt++;

            DialogGUIBase[] options = new DialogGUIBase[cnt];
            cnt = 0;
            if (sph)
            {
                options[cnt++] = new DialogGUIButton("Recover to SPH", RecoverToSPH);
            }
            if (vab)
            {
                options[cnt++] = new DialogGUIButton("Recover to VAB", RecoverToVAB);
            }
            options[cnt++] = new DialogGUIButton("Normal recovery", DoNormalRecovery);
            options[cnt] = new DialogGUIButton("Cancel", Cancel);

            MultiOptionDialog diag = new MultiOptionDialog("scrapVesselPopup", "Do you want KCT to do the recovery?", "Recover Vessel", null, options: options);
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), diag, false, HighLogic.UISkin);
        }
    }
}
