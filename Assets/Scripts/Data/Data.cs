using UnityEngine;

namespace Data
{
    public class PlantInfo
    {
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
        public string Name;
        public int StageID;

        public PlantInfo(string name, int stageID, int locationIndex)
        {
            Name = name;
            StageID = stageID;
            LocationIndex = locationIndex;
        }

        public PlantInfo()
        {
            
        }
    }
}