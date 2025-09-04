using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    private SpriteRenderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        // Init("BigPlant_" + Random.Range(0, 15));
    }

    public bool Init(string spriteName)
    {
        string path = "Sprites/" + spriteName;
        if (!File.Exists(path))
        {
            print(path + " not found");
            return false;
        }
        print(path);
        _renderer.sprite = Resources.Load<Sprite>(path);
        return true;
    }
}
