using Assets.Scripts.Common;
using LitJson;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Core.Client.ConfigManager
{
    public class ConfigPool : ScriptableObject
    {
        [SerializeField]
        public StringDictionary configs;

        public JsonData this[string idx]
        {
            get
            {
                return JsonMapper.ToObject(configs[idx]);
            }
        }
    }
}