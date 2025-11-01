using UnityEngine;

namespace Data
{
    public struct MergedState
    {
        public bool IsMerged;
        public bool IsFather;
        public int MergeWith;

        private MergedState(bool isMerged, bool isFather, int mergeWith)
        {
            IsMerged = isMerged;
            IsFather = isFather;
            MergeWith = mergeWith;
        }
        
        public static MergedState FatherOf(int other) => new MergedState(true, true, other);
        public static MergedState ChildOf(int other)  => new MergedState(true, false, other);

    }
    public class PlantsInScene
    {
        public double GrowingTime;
        private int _locationIndex;
        public int LocationIndex
        {
            get => _locationIndex;
            set
            {
                if (value < 0 )
                {
                    _locationIndex = 0;
                }
                else if (value > 39)
                {
                    _locationIndex = 39;
                }
                else
                {
                    _locationIndex = value;
                }
            }
        }
        public int StageID;
        public int BasicInfoID;
        
        public MergedState MergedState;
        
        public bool IsGrown => 3 + _basicInfo.StagesCounter == StageID;
        
        public static bool operator ==(PlantsInScene a, PlantsInScene b)
        {
            return a?.BasicInfoID == b?.BasicInfoID && a?.IsGrown == b?.IsGrown;
        }

        public static bool operator !=(PlantsInScene a, PlantsInScene b)
        {
            return !(a == b);
        }

        //返回引用的一个表达式主体属性，不占字段内存，相当于一个方法。（随BasicInfoID获取最新指向）
        private ref readonly PlantBasicInfo _basicInfo => ref DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
        
        public PlantsInScene(int basicInfoID, int stageID, int locationIndex)
        {
            BasicInfoID = basicInfoID;
            // _basicInfo = DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
            StageID = stageID;
            LocationIndex = locationIndex;
            GrowingTime = 0;
        }

        public string GetName() => _basicInfo.Name;
        public int GetStageCount() => _basicInfo.StagesCounter;
        public int GetGrownNeedTime() => _basicInfo.GrownNeedTime;

        public PlantsInScene() { }
    }
    
    
    public struct PlantBasicInfo
    {
        public int PlantID;
        public string Name;
        public int StagesCounter;
        public int GrownNeedTime;
    }
}