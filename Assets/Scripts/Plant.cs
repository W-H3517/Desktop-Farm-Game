using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(ResourceUser))]
public class Plant : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private ResourceUser _user;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
        Init("Blueberry_2");
    }
    
    public void Init(string spriteName)
    {
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
    }
}
