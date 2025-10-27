using System;
using System.Collections;
using System.Collections.Generic;
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
    private PlantsInScene _info;
    private PolygonCollider2D polygonCollider;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void InternalInit(PlantsInScene info)
    {
        _info = info;
        var spriteName = _info.GetName() + "_" + _info.StageID;
        transform.position = _info.LocationIndex.LocationIndexToVector2();
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
    }

    /// <summary>
    /// 初始化植物方法
    /// </summary>
    /// <param name="info">植物场景信息实体</param>
    /// <param name="isPlanting">此次操作是否为玩家手动开始种植</param>
    public void Init(PlantsInScene info, bool isPlanting = false)
    {
        InternalInit(info);
        if (!isPlanting) StartCoroutine(GrowingWithTimeIncressing());
    }
    
    
    /// <summary>
    /// 更新碰撞体形状
    /// </summary>
    private void UpdateColliderShape()
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
    /// 放置植物位置实时改变预览
    /// </summary>
    /// <param name="locationIndex"></param>
    public void PrePlace(int locationIndex)
    {
        _info.LocationIndex = locationIndex;
        transform.position = locationIndex.LocationIndexToVector2();
    }
    
    /// <summary>
    /// 种植动作的回调
    /// </summary>
    public void PlantAction()
    {
        // 写入运行时数据
        DataMgr.Instance.AllPlants.Add(_info);
        gameObject.layer = LayerMask.NameToLayer("Plant");
        StartCoroutine(GrowingWithTimeIncressing());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator GrowingWithTimeIncressing()
    {
        while (true)
        {
            _info.GrowingTime += 1;
            if (_info.GrowingTime >= _info.GetGrownNeedTime())
            {
                _info.GrowingTime = 0;
                if (_info.StageID == _info.GetStageCount() + 3) yield break;
                var spriteName = _info.GetName() + "_" + ++_info.StageID;
                //存在未释放旧有sprite资源问题！！！！！！！！！！！
                _user.Load<Sprite>(spriteName, handle =>
                {
                    _renderer.sprite = handle.Result;
                    //每次生长，更新碰撞体
                    UpdateColliderShape();
                } );
                print(spriteName + "has"+"grown!");
            }
            yield return new WaitForSeconds(1f);
        }
    }

}
