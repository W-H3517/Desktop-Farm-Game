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
}