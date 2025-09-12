using System;
using System.IO;
using Data;
using Tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(ResourceUser))]
public class Plant : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private ResourceUser _user;
    
    public PlantInfo Info;
    
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
    }
    public void Init(PlantInfo info)
    {
        Info = info;
        var spriteName = Info.Name + "_" + Info.StageID;
        transform.position = Info.LocationIndex.LocationIndexToVector2();
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
    }
}
