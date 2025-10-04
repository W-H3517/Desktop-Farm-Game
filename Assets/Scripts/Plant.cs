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
    public PlantsInScene Info;
    private PolygonCollider2D polygonCollider;
    
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _user = GetComponent<ResourceUser>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    public void Init(PlantsInScene info)
    {
        StartCoroutine(InternalInit(info));
    }
    
    
    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// 内部初始化用来等待Sprite加载完成后更新其碰撞体形状
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private IEnumerator InternalInit(PlantsInScene info)
    {
        Info = info;
        var spriteName = Info.GetName() + "_" + Info.StageID;
        transform.position = Info.LocationIndex.LocationIndexToVector2();
        _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
        yield return new WaitUntil(() => _renderer.sprite is not null);
        UpdateColliderShape();
        StartCoroutine(TimeIncressing());
    }
    /// <summary>
    /// 内部更新碰撞体形状
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
        Info.LocationIndex = locationIndex;
        transform.position = locationIndex.LocationIndexToVector2();
    }
    
    public void PlantAction()
    {
        // 写入运行时数据
        DataMgr.Instance.AllPlants.Add(Info);
        gameObject.layer = LayerMask.NameToLayer("Plant");
    }

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator TimeIncressing()
    {
        while (true)
        {
            Info.GrowingTime += 1;
            if (Info.GrowingTime >= 10)
            {
                Info.GrowingTime = 0;
                if (Info.StageID == Info.GetStageCount() + 3) yield break;
                var spriteName = Info.GetName() + "_" + ++Info.StageID;
                //存在未释放旧有sprite资源问题！！！！！！！！！！！
                _user.Load<Sprite>(spriteName, handle => _renderer.sprite = handle.Result );
                print(spriteName + "has"+"grown!");
            }
            yield return new WaitForSeconds(1f);
        }
    }

}
