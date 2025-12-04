using Data;
using Framework;

public class HarvestModeMgr : BaseManager<HarvestModeMgr>
{
    private PlantsInScene[] AllPlants => DataMgr.Instance.AllPlants;

    public HarvestModeMgr()
    {
        EventCenter.Instance.AddEventListener<int>(E_EventList.PlantGrown, CheckMerge);
    }

    /// <summary>
    /// 在EventCenter之前调用，在场景加载之前调用
    /// </summary>
    public void ReStart()
    {
        EventCenter.Instance.RemoveEventListener<int>(E_EventList.PlantGrown, CheckMerge);
    }

    public void CheckMergeForAllPlants()
    {
        for (int i = 1; i < AllPlants.Length; i = i + 2)
        {
            CheckMerge(i);
        }
    }

    private void CheckMerge(int i)
    {
        if (i - 1 >= 0 && AllPlants[i - 1] != null && !AllPlants[i - 1].MergedState.IsMerged &&
            AllPlants[i - 1] == AllPlants[i])
        {
            int i1 = i - 1;
            AllPlants[i1].MergedState = MergedState.FatherOf(i);
            AllPlants[i].MergedState = MergedState.ChildOf(i1);
            EventCenter.Instance.EventTrigger(E_EventList.PlantMerged, i1);
        }
        else if (i + 1 < AllPlants.Length && AllPlants[i + 1] != null && !AllPlants[i + 1].MergedState.IsMerged &&
                 AllPlants[i + 1] == AllPlants[i])
        {
            int i1 = i + 1;
            AllPlants[i1].MergedState = MergedState.ChildOf(i);
            AllPlants[i].MergedState = MergedState.FatherOf(i1);
            EventCenter.Instance.EventTrigger(E_EventList.PlantMerged, i);
        }
    }
}