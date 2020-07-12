﻿using System.Collections.Generic;
using System.Linq;

namespace KerbalConstructionTime
{
    public class WindowHelper
    {
        static int startCnt = UnityEngine.Random.Range(1000, 2000000);

        private static Dictionary<string, int> _windowDictionary;
        public static int NextWindowId(string windowKey)
        {
            if (_windowDictionary == null)
            {
                _windowDictionary = new Dictionary<string, int>();
            }

            if (_windowDictionary.ContainsKey(windowKey))
            {
                return _windowDictionary[windowKey];
            }

            var newId = _windowDictionary.Count() + 1 + startCnt;

            _windowDictionary.Add(windowKey, newId);

            return newId;
        }
    }
}
