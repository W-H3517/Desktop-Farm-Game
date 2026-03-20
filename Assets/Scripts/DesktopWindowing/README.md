# DesktopWindowing

## 这是什么

`DesktopWindowing` 是一组给 Windows 桌宠、桌面挂件、透明悬浮窗口使用的 Unity 脚本。

它解决两个核心问题：

1. 让 Unity 窗口支持透明背景
2. 让窗口在空白透明区域点击穿透到桌面或其他应用，在 UI 或 2D 可交互区域保留点击

这个目录面向“新项目直接接入”，建议整目录一起导出。

## 适用场景

- 桌宠
- 桌面农场
- 桌面挂件
- Windows 悬浮层
- 透明背景且只在角色或 UI 区域接收点击的程序

## 目录结构

- `DesktopWindowController.cs`
  主入口组件，也是外部最主要会直接使用的脚本。
- `PointerHitTester.cs`
  Unity 内部命中检测，负责 UI 和 2D Collider。
- `WindowsTransparentWindow.cs`
  Windows 原生窗口层，负责透明窗口和点击穿透。
- `HitTestResult.cs`
  命中类型枚举 `HitTestKind`。

## 怎么接入

1. 导入整个 `Assets/Scripts/DesktopWindowing/` 目录
2. 把 `DesktopWindowController` 挂到主相机，或你的窗口根对象上
3. 将 `Target Camera` 指向主相机
4. 保证场景里有 `EventSystem`
5. 如果要让 2D 物体阻止穿透，确保它们有 `Collider2D`，并且图层包含在 `Interactable 2D Mask` 里
6. 相机背景色 alpha 需要是 `0`
7. 在 Windows Standalone 中测试透明和点击穿透

## Inspector 字段说明

- `Target Camera`
  用来做屏幕坐标到世界坐标转换，也用于 2D 命中检测。一般拖主相机。
- `Enable Transparency`
  是否启用透明窗口。通常保持勾选。
- `Keep Topmost`
  是否保持窗口置顶。桌宠项目通常保持勾选。
- `Interactable 2D Mask`
  哪些 2D 图层算“可交互区域”。
- `Enable In Editor`
  是否在编辑器里更新命中状态。编辑器里不要把它当成原生窗口效果的最终依据。
- `Log Diagnostics`
  是否输出简单调试日志。
- `Enable Click Through`
  是否启用点击穿透。通常保持勾选。

## 外部怎么读取状态

外部脚本主要读取这些状态：

- `DesktopWindowController.Primary.IsPointerOverUnityInteractive`
- `DesktopWindowController.Primary.IsPointerOverUI`
- `DesktopWindowController.Primary.IsPointerOverPhysics2D`
- `DesktopWindowController.Primary.LastHitKind`

`LastHitKind` 目前有三种值：

- `HitTestKind.None`
- `HitTestKind.UI`
- `HitTestKind.Physics2D`

## 使用示例

### 示例 1：最小接入

1. 选中主相机
2. 挂上 `DesktopWindowController`
3. 将 `Target Camera` 指向主相机自己
4. 勾选 `Enable Transparency`
5. 勾选 `Enable Click Through`
6. 如果希望窗口始终在最前，勾选 `Keep Topmost`

这样配置后：

- 鼠标压在空白透明区域时，窗口会尝试点击穿透
- 鼠标压在 UI 或 2D Collider 上时，窗口保留交互

### 示例 2：判断当前是否应该响应输入

```csharp
using DesktopWindowing;
using UnityEngine;

public class ExampleClickGuard : MonoBehaviour
{
    public bool CanHandleInput()
    {
        if (DesktopWindowController.Primary == null)
        {
            return true;
        }

        return DesktopWindowController.Primary.IsPointerOverUnityInteractive;
    }
}
```

### 示例 3：点击 UI 时不执行场景逻辑

```csharp
using DesktopWindowing;
using UnityEngine;

public class ExampleWorldAction : MonoBehaviour
{
    public void TryDoWorldAction()
    {
        var controller = DesktopWindowController.Primary;
        if (controller != null && controller.IsPointerOverUI)
        {
            return;
        }

        Debug.Log("Execute world action");
    }
}
```

### 示例 4：根据命中类型分支处理

```csharp
using DesktopWindowing;
using UnityEngine;

public class ExampleHitTypeReader : MonoBehaviour
{
    private void Update()
    {
        var controller = DesktopWindowController.Primary;
        if (controller == null)
        {
            return;
        }

        switch (controller.LastHitKind)
        {
            case HitTestKind.UI:
                Debug.Log("Pointer is over UI");
                break;
            case HitTestKind.Physics2D:
                Debug.Log("Pointer is over a 2D collider");
                break;
            default:
                Debug.Log("Pointer is over transparent empty area");
                break;
        }
    }
}
```

## 当前边界

当前版本默认面向：

- Windows Standalone
- Unity UI
- 2D Collider 交互

当前版本不负责：

- 自动修改场景文件
- 自动修改 Project Settings
- 自动设置相机背景
- 3D 命中检测
- 像素级 alpha 命中

## 注意事项

1. 透明背景是否生效，不只取决于脚本，还取决于相机背景 alpha 和 Windows 图形合成行为。
2. 点击穿透请以 Windows Standalone 为准，不要只看编辑器表现。
3. 如果透明正常但无法穿透，优先检查：
   - `Enable Click Through` 是否开启
   - 鼠标是否一直命中 UI
   - 鼠标是否一直命中某个 2D Collider
4. 如果打算迁移到别的项目，建议整目录一起导出，避免漏文件。
