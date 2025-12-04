namespace Framework
{
    /// <summary>
    /// 可以被PoolMgr管理的interface
    /// </summary>
    public interface IRecyclable
    {
        /// <summary>
        /// 需要在放入对象池之前调用，清空无用引用，防止内存泄露。
        /// </summary>
        public void Reset();
    }
}