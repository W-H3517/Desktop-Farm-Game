using System.IO;
using UnityEngine;
using LitJson;

public class JsonMgr
{
    private static JsonMgr _instance = new JsonMgr();
    public static JsonMgr Instance => _instance;
    private JsonMgr() { }
    
    public void SaveData(object data, string fileName)
    {
        string jsonstring = JsonMapper.ToJson(data);
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        File.WriteAllText(path, jsonstring, System.Text.Encoding.UTF8);
    }
    
    public T LoadConfigurationData<T>(string fileName) where T : new()
    {
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        if (!File.Exists(path)) 
            return new T();
        string jsonString = File.ReadAllText(path);
        return JsonMapper.ToObject<T>(jsonString);
    }
    
    public T LoadSaveData<T>(string fileName) where T : new()
    {
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            path = Application.streamingAssetsPath + "/" + fileName + ".json"; 
        }
        if (!File.Exists(path)) 
            return new T();
        string jsonString = File.ReadAllText(path);
        return JsonMapper.ToObject<T>(jsonString);
    }
}
