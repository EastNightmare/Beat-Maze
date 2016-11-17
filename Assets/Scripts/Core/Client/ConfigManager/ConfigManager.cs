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
    public class ConfigManager : Singleton<ConfigManager>
    {
        private ConfigPool m_Pool;

        public ConfigPool pool
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = Resources.Load<ConfigPool>(StringCommon.ConfigPoolPathInResources);
                }
                return m_Pool;
            }
        }

        [MenuItem(StringCommon.ConfigPoolMenuItem)]
        public static void PackageJsonConfigs()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            string path = Application.dataPath;

            Debug.Log("Find all .json in " + path);
            var guid = AssetDatabase.AssetPathToGUID(path);
            var withoutExtensions = new List<string>() { ".json" };
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s =>
            {
                var extension = Path.GetExtension(s);
                return extension != null && withoutExtensions.Contains(extension.ToLower());
            }).ToArray();
            var nameToPathDic = new Dictionary<string, string>();
            var startIndex = 0;
            var jsonData = new StringDictionary();
            EditorApplication.update = delegate ()
            {
                var file = files[startIndex];

                var isCancel = EditorUtility.DisplayCancelableProgressBar("Finding Json Configs", file, (float)startIndex / (float)files.Length);
                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    Debug.Log(file + " Found");
                    var fileRawName = StringUtil.LastAfter(file, '\\');
                    var fileName = StringUtil.BeginBefore(fileRawName, '.');
                    if (nameToPathDic.ContainsKey(fileName))
                    {
                        Debug.Log("Same Name With:" + fileName);
                    }
                    else
                    {
                        var pathInAsset = "Assets" + file.Replace(path, string.Empty);
                        var txt = AssetDatabase.LoadAssetAtPath<TextAsset>(pathInAsset);
                        if (txt != null)
                        {
                            JsonData jData = JsonMapper.ToObject(txt.text);
                            jsonData.Add(fileName, jData.ToJson());
                        }
                    }
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("Json Configs Search Finished");
                    var configPool = ScriptableObject.CreateInstance<ConfigPool>();
                    configPool.configs = jsonData;
                    AssetDatabase.CreateAsset(configPool, StringCommon.ConfigPoolPathInAssets);
                    Debug.Log("Json Configs Packaged");
                }
            };
        }

        private static string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }
    }
}