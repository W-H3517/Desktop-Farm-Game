using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Tools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

[RequireComponent(typeof(ResourceUser))]
public class Plant : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private ResourceUser _user;
    private PlantsInScene _info;
    public int BasicInfoID => _info.BasicInfoID;
    private PolygonCollider2D polygonCollider;
    public int LocationIndex => _info.LocationIndex;
    private SpriteAtlas _atlas;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        EventCenter.Instance.AddEventListener<int>("PlantMerged",OnPlantMerged);
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener<int>("PlantMerged",OnPlantMerged);
    }

    private void OnPlantMerged(int locationIndex)
    {
        //不是已种植对象，不可以合并
        if(gameObject.layer != LayerMask.NameToLayer("Plant")) return;
        //如果被合并但不是父亲，则失活
        if(_info.MergedState is { IsMerged: true, IsFather: false }) gameObject.SetActive(false);
        //如果合并者是自己（被合并是父亲），激活且更新显示
        else if(_info.LocationIndex == locationIndex)
        {
            if(!gameObject.activeSelf) gameObject.SetActive(true);
            var spriteName = _info.GetName() + "_" + 3;
            _renderer.sprite = _atlas.GetSprite(spriteName);
            UpdateColliderShape();
        }
        //如果成熟，且未被合并
        else if (_info.IsGrown && !_info.MergedState.IsMerged)
        {
            if(!gameObject.activeSelf) gameObject.SetActive(true);
        }
    }
    
    private void InternalInit(PlantsInScene info)
    {
        var spriteName = _info.GetName() + "_" + _info.StageID;
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
    }

    /// <summary>
    /// 初始化植物方法
    /// </summary>
    /// <param name="info">植物场景信息实体</param>
    /// <param name="isPlanting">此次操作是否为玩家手动开始种植</param>
    public void Init(PlantsInScene info, bool isPlanting = false)
    {
        // InternalInit(info);
        _info = info;
        transform.position = _info.LocationIndex.LocationIndexToVector2();
        if (isPlanting)
        {
            //种植模式预览效果
            var spriteName = _info.GetName() + "_" + (3+info.GetStageCount());
            _user.Load<SpriteAtlas>("PlantStatesSprites", (handle) =>
            {
                _atlas = handle.Result;
                Debug.Log("atlas loaded");
                _renderer.sprite = _atlas.GetSprite(spriteName);
            });
        }
        else
        {
            var spriteName = _info.GetName() + "_" + _info.StageID;
            _user.Load<SpriteAtlas>("PlantStatesSprites", (handle) =>
            {
                _atlas = handle.Result;
                Debug.Log("atlas loaded");
                _renderer.sprite = _atlas.GetSprite(spriteName);
            });
            
            StartCoroutine(GrowingWithTimeIncressing());
        }
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
        DataMgr.Instance.AddPlant(_info);
        gameObject.layer = LayerMask.NameToLayer("Plant");
        var spriteName = _info.GetName() + "_" + _info.StageID;
        _renderer.sprite = _atlas.GetSprite(spriteName);
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
                var spriteName = _info.GetName() + "_" + ++_info.StageID;
                _renderer.sprite = _atlas.GetSprite(spriteName);
                UpdateColliderShape();
                if (_info.IsGrown)
                {
                    EventCenter.Instance.EventTrigger("PlantGrown", _info.LocationIndex);
                    print(spriteName + "has" + "grown!");
                    yield break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

}
