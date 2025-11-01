
using Data;

public class HarvestModeMgr
{
    private static HarvestModeMgr _instance = new HarvestModeMgr();
    public static HarvestModeMgr Instance => _instance;

    private PlantsInScene[] AllPlants =>  DataMgr.Instance.AllPlants;
    private HarvestModeMgr()
    {
        EventCenter.Instance.AddEventListener<int>("PlantGrown", CheckMerge);
    }

    public void ReStart()
    {
        EventCenter.Instance.RemoveEventListener<int>("PlantGrown", CheckMerge);
        _instance = new HarvestModeMgr();
    }

    public void CheckMergeForAllPlants()
    {
        for (int i = 1; i < AllPlants.Length; i = i+2)
        {
            CheckMerge(i);
        }
    }
    
    private void CheckMerge(int i)
    {
        if (i - 1 >= 0 && AllPlants[i-1] != null && !AllPlants[i - 1].MergedState.IsMerged && AllPlants[i - 1] == AllPlants[i])
        {
            int i1 = i - 1;
            AllPlants[i1].MergedState = MergedState.FatherOf(i);
            AllPlants[i].MergedState =  MergedState.ChildOf(i1);
            EventCenter.Instance.EventTrigger("PlantMerged",i1);
        }
        else if (i + 1 < AllPlants.Length && AllPlants[i+1] != null&& !AllPlants[i + 1].MergedState.IsMerged && AllPlants[i + 1] == AllPlants[i])
        {
            int i1 = i + 1;
            AllPlants[i1].MergedState = MergedState.ChildOf(i);
            AllPlants[i].MergedState =  MergedState.FatherOf(i1);
            EventCenter.Instance.EventTrigger("PlantMerged",i);
        }
    }
}