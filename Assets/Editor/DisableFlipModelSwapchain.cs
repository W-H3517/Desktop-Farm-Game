#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DisableFlipModelSwapchain : Editor
{
    // 在 Player 打包前自动调用
    [InitializeOnLoadMethod]
    static void DisableFlipModel()
    {
        // 只在 Windows Player 下设置
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            PlayerSettings.useFlipModelSwapchain = false;
            Debug.Log("Flip Model Swapchain 已禁用（BitBlt 模式）");
        }
    }
}
#endif

