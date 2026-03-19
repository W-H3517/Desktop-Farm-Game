# DesktopWindowing

## 这是什么

`DesktopWindowing` 是一组给 Windows 桌面宠物、桌面覆盖层、透明悬浮窗口这类项目使用的 Unity 脚本。

它主要解决两个问题：

1. 让 Unity 窗口支持透明背景
2. 让窗口在“空白透明区域”点击穿透到桌面或其他应用，而在“UI 或可交互 2D 物体区域”保留点击能力

这个目录默认面向“新项目直接接入”。
如果你在旧项目里看到 `TransparentWindow` 兼容壳，那只是为了兼容历史代码，不属于这个模块的核心公开接口。

这套脚本适合：

- 桌宠
- 桌面挂件
- 桌面农场
- Windows 悬浮层
- 需要透明背景且只在角色/UI 区域接收点击的程序

## 能做什么

导入并正确挂载后，这组脚本可以提供下面这些能力：

- Windows 下的透明窗口支持
- 根据鼠标当前位置自动切换点击穿透
- Unity UI 命中检测
- 2D 物理命中检测
- 统一的命中结果状态，方便业务代码判断“当前鼠标是否压在可交互区域”
- 对旧 `TransparentWindow` 调用方式的兼容

## 文件说明

- `DesktopWindowController.cs`
  主入口组件。你通常只需要手动挂这个脚本。
- `WindowsLayeredWindowBackend.cs`
  Windows 原生窗口后端，负责透明窗口和点击穿透。
- `UnityHitTestService.cs`
  负责 Unity 里的 UI 和 2D 命中检测。
- `HitTestResult.cs`
  命中结果类型定义。
- `IHitTestService.cs`
  命中服务接口。
- `IWindowBackend.cs`
  窗口后端接口。

## 别人怎么用

建议直接导出整个文件夹：

- `Assets/Scripts/DesktopWindowing/`

导入到另一个 Unity 项目后，按下面步骤使用：

1. 把 `DesktopWindowController` 挂到主相机，或者你自己的窗口根对象上
2. 在 Inspector 里把 `Target Camera` 指向用于显示内容的主相机
3. 保证场景里存在 `EventSystem`
4. 如果你有 UI，确保对应 Canvas 可以正常做 UI 射线检测
5. 如果你要让 2D 物体阻止穿透，确保它们有 2D Collider，并且图层在 `Interactable 2D Mask` 内
6. 相机背景色 alpha 需要是 `0`
7. 在 Windows 打包程序里测试透明和点击穿透效果

## 组件字段怎么配

`DesktopWindowController` 目前有这些主要字段：

- `Target Camera`
  用来做屏幕坐标到世界坐标转换，也用于 2D 命中检测。一般拖主相机。
- `Enable Transparency`
  是否启用透明窗口能力。通常保持勾选。
- `Keep Topmost`
  是否让窗口保持置顶。桌宠项目通常勾选。
- `Interactable 2D Mask`
  哪些 2D 图层算“可交互区域”。鼠标压在这些图层的 2D Collider 上时，不会穿透。
- `Enable In Editor`
  是否在 Unity 编辑器里启用命中检测更新。默认不建议依赖编辑器测试原生窗口效果。
- `Log Diagnostics`
  是否输出简单诊断日志。
- `Enable Click Through`
  是否启用点击穿透。通常保持勾选。

## 业务代码怎么读取状态

如果别的脚本想知道“鼠标现在是不是压在可交互区域”，可以读取：

- `DesktopWindowController.Primary.IsPointerOverUnityInteractive`
- `DesktopWindowController.Primary.IsPointerOverUI`
- `DesktopWindowController.Primary.IsPointerOverPhysics2D`
- `DesktopWindowController.Primary.LastHitKind`

如果项目里还保留旧逻辑，也可以继续通过兼容层读取：

- `TransparentWindow.Instance.uiHit`
- `TransparentWindow.Instance.physicsHit`
- `TransparentWindow.Instance.clickThrough`

不过新项目不推荐依赖这个兼容壳，更推荐直接使用 `DesktopWindowController`。
兼容壳不属于建议导出的模块内容。

## 使用示例

### 示例 1：最小接入

场景中手动挂载：

1. 选中主相机
2. 挂上 `DesktopWindowController`
3. 将 `Target Camera` 指向主相机自己
4. 勾选 `Enable Transparency`
5. 勾选 `Enable Click Through`
6. 如果希望桌宠始终在最前，勾选 `Keep Topmost`

这样配置后：

- 鼠标压在空白透明区域时，窗口会尝试点击穿透
- 鼠标压在 UI 或 2D Collider 上时，窗口会保留交互

### 示例 2：业务代码判断当前是否应该响应点击

```csharp
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

适合用在这些情况：

- 只有鼠标压在角色或 UI 上时才允许交互
- 空白区域不执行游戏逻辑

### 示例 3：只在非 UI 区域执行场景操作

```csharp
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

这个写法适合：

- 点击 UI 时不触发场景种植
- 点击 UI 时不触发角色拖拽
- 点击 UI 时不触发拾取或放置

### 示例 4：根据命中类型做不同处理

```csharp
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

这个写法适合：

- 调试命中行为
- 根据当前指针区域切换光标或提示
- 分开处理 UI 点击和角色点击

## 使用前提

这套脚本目前默认面向：

- Windows Standalone
- Unity UI
- 2D Collider 交互

当前版本不负责：

- 自动修改场景文件
- 自动修改 Project Settings
- 自动设置相机背景
- 3D 命中检测
- 像素级 alpha 命中

也就是说，脚本本身已经尽量独立，但项目仍需要你自己在 Unity 里完成基本挂载和显示配置。

## 注意事项

1. 透明背景是否生效，不只取决于脚本，还取决于相机背景 alpha 和 Windows 图形合成行为。
2. 点击穿透在编辑器里不等于打包后行为，应该以 Windows Standalone 为准。
3. 如果透明正常但无法穿透，优先检查：
   - `Enable Click Through` 是否开启
   - 鼠标是否一直命中 UI
   - 鼠标是否一直命中某个 2D Collider
4. 如果想迁移到别的项目，最好整目录一起导出，避免漏掉接口文件或兼容层。

## 推荐的导出方式

推荐直接把下面这个目录作为一个整体导出：

- `Assets/Scripts/DesktopWindowing/`

这样后续一般只需要维护这个文件夹内的代码，不容易影响项目其他部分。
如果你的项目里还保留 `TransparentWindow.cs`，建议把它视为当前项目的过渡适配层，而不是模块正式接口的一部分。
