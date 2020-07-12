using UnityEngine;

namespace KerbalConstructionTime
{
    public static class KCTDebug
    {
        public static void LogError(object message)
        {
            Log(message, true);
        }

        public static void Log(object message, bool always = false)
        {
        #if DEBUG
            bool isBetaVersion = true;
        #else
            bool isBetaVersion = always;
        #endif
            if (KCTGameStates.Settings.Debug || isBetaVersion)
            {
                Debug.Log("[KCT] " + message);
            }
        }
    }

    public class KCT_OnLoadError
    {
        public bool OnLoadCalled, OnLoadFinished, AlertFired;
        private int timeout = 100, timer = 0;

        public bool HasErrored()
        {
            if (timer >= timeout)
            {
                return (OnLoadCalled && !OnLoadFinished);
            }
            else if (timer >= 0)
            {
                timer++;
            }
            return false;
        }

        public void OnLoadStart()
        {
            KCTDebug.Log("OnLoad Started");
            OnLoadCalled = true;
            OnLoadFinished = false;
            timer = 0;
            AlertFired = false;
        }

        public void OnLoadFinish()
        {
            OnLoadCalled = false;
            OnLoadFinished = true;
            timer = -1;
            KCTDebug.Log("OnLoad Completed");
        }

        public void FireAlert()
        {
            if (!AlertFired)
            {
                AlertFired = true;
                Debug.LogError("[KCT] ERROR! An error while KCT loading data occurred. Things will be seriously broken!");
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "errorPopup", "Error Loading KCT Data", "ERROR! An error occurred while loading KCT data. Things will be seriously broken! Please report this error to RP-1 GitHub and attach the log file. The game will be UNPLAYABLE in this state!", "Understood", false, HighLogic.UISkin);

                //Enable debug messages for future reports
                KCTGameStates.Settings.Debug = true;
                KCTGameStates.Settings.Save();
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
