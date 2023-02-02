using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Saro.Core;
using Saro.Utility;
using System.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

namespace Saro.Entities
{
    public class EntityPrefab_JsonDataProvider
    {
        private readonly string m_FilePath = $"Assets/ResRaw/Json/{typeof(EntityPrefab).Name}.json";

        public List<Prefab> Load()
        {
#if UNITY_EDITOR //&& false
            var json = File.ReadAllText(m_FilePath);
#else 
            var bytes = IAssetManager.Current.GetRawFileBytes(m_FilePath);
            var json = Encoding.UTF8.GetString(bytes);
#endif
            var configs = JsonHelper.FromJson<List<Prefab>>(json);
            // TODO 检测数据合法性

            return configs;
        }

        public async ValueTask<List<Prefab>> LoadAsync()
        {
#if UNITY_EDITOR
            var json = await File.ReadAllTextAsync(m_FilePath);
#else
            var bytes = await IAssetManager.Current.GetRawFileBytesAsync(m_FilePath);
            //Log.INFO($"GetRawFileAsync 2: {m_FilePath} {bytes.Length}");
            var json = Encoding.UTF8.GetString(bytes);
            //Log.INFO($"{typeof(T).Name} {json}"); 
#endif
            var configs = JsonHelper.FromJson<List<Prefab>>(json);
            // TODO 检测数据合法性

            return configs;
        }

        public void Save()
        {
#if UNITY_EDITOR
            ToJson(m_FilePath);
#endif
        }

#if UNITY_EDITOR
        private void ToJson(string file)
        {
            var list = new List<Prefab>();

            // 这个只能找到场景里的
            //var finds = UnityEngine.Object.FindObjectsOfType(typeof(EntityPrefab)).Select(g => g as EntityPrefab);
            //foreach (var item in finds)
            //{
            //    list.Add(item.Bake());
            //}

            //var finds = AssetDatabase.FindAssets($"t:{typeof(EntityPrefab).Name}"); // TODO 这个不能获取 mono 脚本
            //foreach (var item in finds)
            //{
            //    var path = AssetDatabase.GUIDToAssetPath(item);
            //    var config = AssetDatabase.LoadAssetAtPath<EntityPrefab>(path);
            //    list.Add(config.Bake());
            //}

            //var json = JsonConvert.SerializeObject(list, m_Settings);
            var json = JsonHelper.ToJson(list);
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(file, json);
        }
#endif
    }
}
