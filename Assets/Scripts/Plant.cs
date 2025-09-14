using System;
using System.Collections;
using System.Collections.Generic;
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
    private PolygonCollider2D polygonCollider;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    public void Init(PlantInfo info)
    {
        StartCoroutine(InternalInit(info));
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator InternalInit(PlantInfo info)
    {
        Info = info;
        var spriteName = Info.Name + "_" + Info.StageID;
        transform.position = Info.LocationIndex.LocationIndexToVector2();
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
        yield return new WaitUntil(() => _renderer.sprite is not null);
        UpdateColliderShape();
    }
    
    void UpdateColliderShape()
    {
        int shapeCount = _renderer.sprite.GetPhysicsShapeCount();
        polygonCollider.pathCount = shapeCount;
        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            path.Clear();
            _renderer.sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }
    
    /// <summary>
    /// Sprite初始化
    /// </summary>
    /// <param name="spriteName"></param>
    public void Init(string spriteName)
    {
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
    }
    
    /// <summary>
    /// 放置植物位置预览
    /// </summary>
    /// <param name="locationIndex"></param>
    public void PrePlace(int locationIndex)
    {
        Info.LocationIndex = locationIndex;
        transform.position = Info.LocationIndex.LocationIndexToVector2();
    }
    
    public void PlantAction()
    {
        // 写入运行时数据
        DataMgr.Instance.AllPlantInfo.Add(Info);
    }
    
}
