using UnityEngine;

namespace Data
{
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
                else if (value > 38)
                {
                    _locationIndex = 38;
                }
                else
                {
                    _locationIndex = value;
                }
            }
        }
        public int StageID;
        public int BasicInfoID;
        
        private PlantBasicInfo _basicInfo;
        
        public PlantsInScene(int basicInfoID, int stageID, int locationIndex)
        {
            BasicInfoID = basicInfoID;
            _basicInfo = DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
            StageID = stageID;
            LocationIndex = locationIndex;
            GrowingTime = 0;
        }

        public string GetName()
        {
            _basicInfo ??= DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
            return _basicInfo.Name;
        }

        public int GetStageCount()
        {
            _basicInfo ??= DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
            return _basicInfo.StagesCounter;
        }
        
        public int GetGrownNeedTime()
        {
            _basicInfo ??= DataMgr.Instance.AllBasicPlantInfo[BasicInfoID];
            return _basicInfo.GrownNeedTime;
        }
        
        public PlantsInScene() { }
    }
    
    
    public class PlantBasicInfo
    {
        public int PlantID;
        public string Name;
        public int StagesCounter;
        public int GrownNeedTime;
    }
}