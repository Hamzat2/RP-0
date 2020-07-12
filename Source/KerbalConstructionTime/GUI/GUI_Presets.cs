using System;
using UnityEngine;

namespace KerbalConstructionTime
{
    public static partial class KCT_GUI
    {
        private const int _presetsWidth = 900, _presetsHeight = 600;
        private static Rect _presetPosition = new Rect((Screen.width-_presetsWidth) / 2, (Screen.height-_presetsHeight) / 2, _presetsWidth, _presetsHeight);
        private static Rect _presetNamingWindowPosition = new Rect((Screen.width - 250) / 2, (Screen.height - 50) / 2, 250, 50);
        private static int _presetIndex = -1;
        private static KCT_Preset _workingPreset;
        private static Vector2 _presetScrollView, _presetMainScroll;
        private static bool _isChanged = false, _showFormula = false;
        private static string _OMultTmp = "", _BEffTmp = "", _IEffTmp = "", _ReEffTmp = "", _MaxReTmp = "";

        private static string _saveName, _saveShort, _saveDesc, _saveAuthor;
        private static bool _saveCareer, _saveScience, _saveSandbox;
        private static KCT_Preset _toSave;

        private static bool _forceStopWarp, _disableAllMsgs, _debug, _overrideLaunchBtn, _autoAlarms;
        private static int _newTimewarp;

        public static void DrawPresetWindow(int windowID)
        {
            GUIStyle yellowText = new GUIStyle(GUI.skin.label);
            yellowText.normal.textColor = Color.yellow;

            if (_workingPreset == null)
            {
                SetNewWorkingPreset(new KCT_Preset(PresetManager.Instance.ActivePreset), false); //might need to copy instead of assign here
                _presetIndex = PresetManager.Instance.GetIndex(_workingPreset);
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            //preset selector
            GUILayout.BeginVertical();
            GUILayout.Label("Presets", yellowText, GUILayout.ExpandHeight(false));
            //preset toolbar in a scrollview
            _presetScrollView = GUILayout.BeginScrollView(_presetScrollView, GUILayout.Width(_presetPosition.width/6.0f)); //TODO: update HighLogic.Skin.textArea
            string[] presetShortNames = PresetManager.Instance.PresetShortNames(true);
            if (_presetIndex == -1)
            {
                SetNewWorkingPreset(null, true);
            }
            if (_isChanged && _presetIndex < presetShortNames.Length - 1 && !Utilities.ConfigNodesAreEquivalent(_workingPreset.AsConfigNode(), PresetManager.Instance.Presets[_presetIndex].AsConfigNode())) //!KCT_PresetManager.Instance.PresetsEqual(WorkingPreset, KCT_PresetManager.Instance.Presets[presetIndex], true)
            {
                SetNewWorkingPreset(null, true);
            }

            int prev = _presetIndex;
            _presetIndex = GUILayout.SelectionGrid(_presetIndex, presetShortNames, 1);
            if (prev != _presetIndex) //If a new preset was selected
            {
                if (_presetIndex != presetShortNames.Length - 1)
                {
                    SetNewWorkingPreset(new KCT_Preset(PresetManager.Instance.Presets[_presetIndex]), false);
                }
                else
                {
                    SetNewWorkingPreset(null, true);
                }
            }

            //presetIndex = GUILayout.Toolbar(presetIndex, presetNames);

            GUILayout.EndScrollView();
            if (GUILayout.Button("Save as\nNew Preset", GUILayout.ExpandHeight(false)))
            {
                //create new preset
                SaveAsNewPreset(_workingPreset);
            }
            if (_workingPreset.AllowDeletion && _presetIndex != presetShortNames.Length - 1 && GUILayout.Button("Delete Preset")) //allowed to be deleted and isn't Custom
            {

                DialogGUIBase[] options = new DialogGUIBase[2];
                options[0] = new DialogGUIButton("Delete File", DeleteActivePreset);
                options[1] = new DialogGUIButton("Cancel", RemoveInputLocks);
                MultiOptionDialog dialog = new MultiOptionDialog("deletePresetPopup", "Are you sure you want to delete the selected Preset, file and all? This cannot be undone!", "Confirm Deletion", null, options);
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), dialog, false, HighLogic.UISkin);
            }
            GUILayout.EndVertical();

            //Main sections
            GUILayout.BeginVertical();
            _presetMainScroll = GUILayout.BeginScrollView(_presetMainScroll);
            //Preset info section)
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.Label("Preset Name: " + _workingPreset.name);
            GUILayout.Label("Description: " + _workingPreset.description);
            GUILayout.Label("Author(s): " + _workingPreset.author);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            //Features section
            GUILayout.BeginVertical();
            GUILayout.Label("Features", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            _workingPreset.generalSettings.Enabled= GUILayout.Toggle(_workingPreset.generalSettings.Enabled, "Mod Enabled", HighLogic.Skin.button);
            _workingPreset.generalSettings.BuildTimes = GUILayout.Toggle(_workingPreset.generalSettings.BuildTimes, "Build Times", HighLogic.Skin.button);
            _workingPreset.generalSettings.ReconditioningTimes = GUILayout.Toggle(_workingPreset.generalSettings.ReconditioningTimes, "Launchpad Reconditioning", HighLogic.Skin.button);
            _workingPreset.generalSettings.ReconditioningBlocksPad = GUILayout.Toggle(_workingPreset.generalSettings.ReconditioningBlocksPad, "Reconditioning Blocks Pad", HighLogic.Skin.button);
            _workingPreset.generalSettings.TechUnlockTimes = GUILayout.Toggle(_workingPreset.generalSettings.TechUnlockTimes, "Tech Unlock Times", HighLogic.Skin.button);
            _workingPreset.generalSettings.KSCUpgradeTimes = GUILayout.Toggle(_workingPreset.generalSettings.KSCUpgradeTimes, "KSC Upgrade Times", HighLogic.Skin.button);
            _workingPreset.generalSettings.TechUpgrades = GUILayout.Toggle(_workingPreset.generalSettings.TechUpgrades, "Upgrades From Tech Tree", HighLogic.Skin.button);
            _workingPreset.generalSettings.SharedUpgradePool = GUILayout.Toggle(_workingPreset.generalSettings.SharedUpgradePool, "Shared Upgrade Pool (KSCSwitcher)", HighLogic.Skin.button);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Starting Upgrades:");
            _workingPreset.generalSettings.StartingPoints = GUILayout.TextField(_workingPreset.generalSettings.StartingPoints, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical(); //end Features


            GUILayout.BeginVertical(); //Begin time settings
            GUILayout.Label("Time Settings", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Overall Multiplier: ");
            double.TryParse(_OMultTmp = GUILayout.TextField(_OMultTmp, 10, GUILayout.Width(80)), out _workingPreset.timeSettings.OverallMultiplier);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build Effect: ");
            double.TryParse(_BEffTmp = GUILayout.TextField(_BEffTmp, 10, GUILayout.Width(80)), out _workingPreset.timeSettings.BuildEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Inventory Effect: ");
            double.TryParse(_IEffTmp = GUILayout.TextField(_IEffTmp, 10, GUILayout.Width(80)), out _workingPreset.timeSettings.InventoryEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reconditioning Effect: ");
            double.TryParse(_ReEffTmp = GUILayout.TextField(_ReEffTmp, 10, GUILayout.Width(80)), out _workingPreset.timeSettings.ReconditioningEffect);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Reconditioning: ");
            double.TryParse(_MaxReTmp = GUILayout.TextField(_MaxReTmp, 10, GUILayout.Width(80)), out _workingPreset.timeSettings.MaxReconditioning);
            GUILayout.EndHorizontal();
            GUILayout.Label("Rollout-Reconditioning Split:");
            GUILayout.BeginHorizontal();
            //GUILayout.Label("Rollout", GUILayout.ExpandWidth(false));
            _workingPreset.timeSettings.RolloutReconSplit = GUILayout.HorizontalSlider((float)Math.Floor(_workingPreset.timeSettings.RolloutReconSplit * 100f), 0, 100)/100.0;
            //GUILayout.Label("Recon.", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.Label((Math.Floor(_workingPreset.timeSettings.RolloutReconSplit*100))+"% Rollout, "+(100-Math.Floor(_workingPreset.timeSettings.RolloutReconSplit*100))+"% Reconditioning");
            GUILayout.EndVertical(); //end time settings
            GUILayout.EndVertical();
            GUILayout.EndHorizontal(); //end feature/time setting split

            //begin formula settings
            GUILayout.BeginVertical();
            GUILayout.Label("Formula Settings (Advanced)", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show/Hide Formulas"))
            {
                _showFormula = !_showFormula;
            }
            GUILayout.EndHorizontal();

            if (_showFormula)
            {
                //show half here, half on other side? Or all in one big list
                int textWidth = 350;
                GUILayout.BeginHorizontal();
                GUILayout.Label("NodeFormula: ");
                _workingPreset.formulaSettings.NodeFormula = GUILayout.TextField(_workingPreset.formulaSettings.NodeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradeFunds: ");
                _workingPreset.formulaSettings.UpgradeFundsFormula = GUILayout.TextField(_workingPreset.formulaSettings.UpgradeFundsFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradesForScience: ");
                _workingPreset.formulaSettings.UpgradesForScience = GUILayout.TextField(_workingPreset.formulaSettings.UpgradesForScience, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ResearchFormula: ");
                _workingPreset.formulaSettings.ResearchFormula = GUILayout.TextField(_workingPreset.formulaSettings.ResearchFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("EffectivePart: ");
                _workingPreset.formulaSettings.EffectivePartFormula = GUILayout.TextField(_workingPreset.formulaSettings.EffectivePartFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("ProceduralPart: ");
                _workingPreset.formulaSettings.ProceduralPartFormula = GUILayout.TextField(_workingPreset.formulaSettings.ProceduralPartFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("BPFormula: ");
                _workingPreset.formulaSettings.BPFormula = GUILayout.TextField(_workingPreset.formulaSettings.BPFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("KSCUpgrade: ");
                _workingPreset.formulaSettings.KSCUpgradeFormula = GUILayout.TextField(_workingPreset.formulaSettings.KSCUpgradeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Reconditioning: ");
                _workingPreset.formulaSettings.ReconditioningFormula = GUILayout.TextField(_workingPreset.formulaSettings.ReconditioningFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("BuildRate: ");
                _workingPreset.formulaSettings.BuildRateFormula = GUILayout.TextField(_workingPreset.formulaSettings.BuildRateFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("UpgradeReset: ");
                _workingPreset.formulaSettings.UpgradeResetFormula = GUILayout.TextField(_workingPreset.formulaSettings.UpgradeResetFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("InventorySale: ");
                _workingPreset.formulaSettings.InventorySaleFormula = GUILayout.TextField(_workingPreset.formulaSettings.InventorySaleFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("RolloutCosts: ");
                _workingPreset.formulaSettings.RolloutCostFormula = GUILayout.TextField(_workingPreset.formulaSettings.RolloutCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("IntegrationCosts: ");
                _workingPreset.formulaSettings.IntegrationCostFormula = GUILayout.TextField(_workingPreset.formulaSettings.IntegrationCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("IntegrationTime: ");
                _workingPreset.formulaSettings.IntegrationTimeFormula = GUILayout.TextField(_workingPreset.formulaSettings.IntegrationTimeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("NewLaunchPadCost: ");
                _workingPreset.formulaSettings.NewLaunchPadCostFormula = GUILayout.TextField(_workingPreset.formulaSettings.NewLaunchPadCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("RushCost: ");
                _workingPreset.formulaSettings.RushCostFormula = GUILayout.TextField(_workingPreset.formulaSettings.RushCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("AirlaunchCost: ");
                _workingPreset.formulaSettings.AirlaunchCostFormula = GUILayout.TextField(_workingPreset.formulaSettings.AirlaunchCostFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("AirlaunchTime: ");
                _workingPreset.formulaSettings.AirlaunchTimeFormula = GUILayout.TextField(_workingPreset.formulaSettings.AirlaunchTimeFormula, GUILayout.Width(textWidth));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical(); //end formula settings

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                PresetManager.Instance.ActivePreset = _workingPreset;
                PresetManager.Instance.SaveActiveToSaveData();
                _workingPreset = null;

                if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled)
                    Utilities.DisableModFunctionality();
                KCTGameStates.Settings.MaxTimeWarp = _newTimewarp;
                KCTGameStates.Settings.ForceStopWarp = _forceStopWarp;
                KCTGameStates.Settings.DisableAllMessages = _disableAllMsgs;
                KCTGameStates.Settings.OverrideLaunchButton = _overrideLaunchBtn;
                KCTGameStates.Settings.Debug = _debug;
                KCTGameStates.Settings.AutoKACAlarms = _autoAlarms;

                KCTGameStates.Settings.Save();
                GUIStates.ShowSettings = false;
                if (!IsPrimarilyDisabled && !GUIStates.ShowFirstRun)
                {
                    ResetBLWindow();
                    GUIStates.ShowBuildList = true;
                    RefreshToolbarState();
                }
                if (!PresetManager.Instance.ActivePreset.generalSettings.Enabled) InputLockManager.RemoveControlLock("KCTKSCLock");

                for (int j = 0; j < KCTGameStates.TechList.Count; j++)
                    KCTGameStates.TechList[j].UpdateBuildRate(j);

                foreach (KSCItem ksc in KCTGameStates.KSCs)
                {
                    ksc.RecalculateBuildRates();
                    ksc.RecalculateUpgradedBuildRates();
                }
                ResetFormulaRateHolders();
            }
            if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
            {
                _workingPreset = null;
                GUIStates.ShowSettings = false;
                if (!IsPrimarilyDisabled && !GUIStates.ShowFirstRun)
                {
                    ResetBLWindow();
                    GUIStates.ShowBuildList = true;
                    RefreshToolbarState();
                }

                for (int j = 0; j < KCTGameStates.TechList.Count; j++)
                    KCTGameStates.TechList[j].UpdateBuildRate(j);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical(); //end column 2

            GUILayout.BeginVertical(GUILayout.Width(100)); //Start general settings
            GUILayout.Label("General Settings", yellowText);
            GUILayout.Label("NOTE: Affects all saves!", yellowText);
            GUILayout.BeginVertical(HighLogic.Skin.textArea);
            GUILayout.Label("Max Timewarp");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
            {
                _newTimewarp = Math.Max(_newTimewarp - 1, 0);
            }
            //current warp setting
            GUILayout.Label(TimeWarp.fetch.warpRates[_newTimewarp] + "x");
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                _newTimewarp = Math.Min(_newTimewarp + 1, TimeWarp.fetch.warpRates.Length - 1);
            }
            GUILayout.EndHorizontal();

            _forceStopWarp = GUILayout.Toggle(_forceStopWarp, "Auto Stop TimeWarp", HighLogic.Skin.button);
            _autoAlarms = GUILayout.Toggle(_autoAlarms, "Auto KAC Alarms", HighLogic.Skin.button);
            _overrideLaunchBtn = GUILayout.Toggle(_overrideLaunchBtn, "Override Launch Button", HighLogic.Skin.button);
            //useBlizzyToolbar = GUILayout.Toggle(useBlizzyToolbar, "Use Toolbar Mod", HighLogic.Skin.button);
            _disableAllMsgs = !GUILayout.Toggle(!_disableAllMsgs, "Use Message System", HighLogic.Skin.button);
            _debug = GUILayout.Toggle(_debug, "Debug Logging", HighLogic.Skin.button);

            GUILayout.EndVertical();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal(); //end main split
            GUILayout.EndVertical(); //end window

            _isChanged = GUI.changed;

            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }

        public static void SetNewWorkingPreset(KCT_Preset preset, bool setCustom)
        {
            if (preset != null)
                _workingPreset = preset;
            if (setCustom)
            {
                _presetIndex = PresetManager.Instance.PresetShortNames(true).Length - 1; //Custom preset
                _workingPreset.name = "Custom";
                _workingPreset.shortName = "Custom";
                _workingPreset.description = "A custom set of configs.";
                _workingPreset.author = HighLogic.SaveFolder;
            }

            _OMultTmp = _workingPreset.timeSettings.OverallMultiplier.ToString();
            _BEffTmp = _workingPreset.timeSettings.BuildEffect.ToString();
            _IEffTmp = _workingPreset.timeSettings.InventoryEffect.ToString();
            _ReEffTmp = _workingPreset.timeSettings.ReconditioningEffect.ToString();
            _MaxReTmp = _workingPreset.timeSettings.MaxReconditioning.ToString();
        }

        public static void DrawPresetSaveWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset name:");
            _saveName = GUILayout.TextField(_saveName, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset short name:");
            _saveShort = GUILayout.TextField(_saveShort, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset author(s):");
            _saveAuthor = GUILayout.TextField(_saveAuthor, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Preset description:");
            _saveDesc = GUILayout.TextField(_saveDesc, GUILayout.Width(220));
            //GUILayout.EndHorizontal();

            _saveCareer = GUILayout.Toggle(_saveCareer, " Show in Career Games");
            _saveScience = GUILayout.Toggle(_saveScience, " Show in Science Games");
            _saveSandbox = GUILayout.Toggle(_saveSandbox, " Show in Sandbox Games");

            KCT_Preset existing = PresetManager.Instance.FindPresetByShortName(_saveShort);
            bool AlreadyExists = existing != null;
            bool CanOverwrite = AlreadyExists ? existing.AllowDeletion : true;

            if (AlreadyExists)
                GUILayout.Label("Warning: A preset with that short name already exists!");

            GUILayout.BeginHorizontal();
            if (CanOverwrite && GUILayout.Button("Save"))
            {
                _toSave.name = _saveName;
                _toSave.shortName = _saveShort;
                _toSave.description = _saveDesc;
                _toSave.author = _saveAuthor;

                _toSave.CareerEnabled = _saveCareer;
                _toSave.ScienceEnabled = _saveScience;
                _toSave.SandboxEnabled = _saveSandbox;

                _toSave.AllowDeletion = true;

                _toSave.SaveToFile(KSPUtil.ApplicationRootPath + "/GameData/RP-0/KCT_Presets/" + _toSave.shortName+".cfg");
                GUIStates.ShowPresetSaver = false;
                PresetManager.Instance.FindPresetFiles();
                PresetManager.Instance.LoadPresets();
            }
            if (GUILayout.Button("Cancel"))
            {
                GUIStates.ShowPresetSaver = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            CenterWindow(ref _presetNamingWindowPosition);
        }

        public static void SaveAsNewPreset(KCT_Preset newPreset)
        {
            _toSave = newPreset;
            _saveCareer = newPreset.CareerEnabled;
            _saveScience = newPreset.ScienceEnabled;
            _saveSandbox = newPreset.SandboxEnabled;

            _saveName = newPreset.name;
            _saveShort = newPreset.shortName;
            _saveDesc = newPreset.description;
            _saveAuthor = newPreset.author;

            GUIStates.ShowPresetSaver = true;
        }

        public static void DeleteActivePreset()
        {
            PresetManager.Instance.DeletePresetFile(_workingPreset.shortName);
        }

        private static void ShowSettings()
        {
            _newTimewarp = KCTGameStates.Settings.MaxTimeWarp;
            _forceStopWarp = KCTGameStates.Settings.ForceStopWarp;
            _disableAllMsgs = KCTGameStates.Settings.DisableAllMessages;
            _debug = KCTGameStates.Settings.Debug;
            _overrideLaunchBtn = KCTGameStates.Settings.OverrideLaunchButton;
            _autoAlarms = KCTGameStates.Settings.AutoKACAlarms;

            GUIStates.ShowSettings = !GUIStates.ShowSettings;
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
