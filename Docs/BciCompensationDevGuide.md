# BCI 接口骨架与代偿模式开发说明

## 背景

正式设备为 **HybridBCI 单通道脑电头环**（专注力 0–100、眨眼 EOG、头动 IMU）。  
当前暂无头环，测试版通过 **代偿模式** 用 USB 摄像头 + OpenSeeFace 模拟部分输入。

## 架构

```
游戏逻辑 (各 *Controller)
        │
        ▼
   BciInputRouter  ←── 统一入口 (IBciInputProvider)
        │
   ┌────┴────┬──────────────┐
   ▼         ▼              ▼
代偿模式   模拟模式      HybridBCI（预留）
(OpenSee)  (键盘)       (未来 SDK)
```

| 信号 | HybridBCI（正式） | 代偿模式（测试） | 模拟模式（无摄像头） |
|------|-------------------|------------------|----------------------|
| 专注力 0–100 | 平台解码算法 | **鼠标滚轮**手动调节 | **鼠标滚轮** |
| 眨眼 | EOG | OpenSeeFace `EyeLeft/EyeRight` | 空格 |
| 点头/摇头/转头 | IMU | OpenSeeFace 头部 `rotation` | W/S/Q/E |

## OpenSeeFace 来源

已集成自 EasyVtuber 包内现成代码：

- 插件目录：`Assets/_Project/Plugins/OpenSeeFace/`
- 原始路径：`f:\EasyVtuber\OpenSeeFace-v1.20.4\`
- 追踪程序：`f:\EasyVtuber\OpenSeeFace-v1.20.4\Binary\facetracker.exe`
- 模型目录：`f:\EasyVtuber\OpenSeeFace-v1.20.4\models\`

UDP 默认：`127.0.0.1:11573`

## 代偿开关

- 组件：`BciInputRouter.useCompensation`
- 运行时按 **`C`** 切换代偿开/关
- 测试 HUD 左上角显示当前模式

## 测试版操作

| 操作 | 功能 |
|------|------|
| **鼠标滚轮** | 调节专注力（0–100） |
| **C** | 切换代偿模式 |
| **T** | 启动/停止 OpenSeeFace 追踪（仅代偿开时） |
| 空格 / 摄像头眨眼 | 眨眼（模拟模式 / 代偿） |
| W/S/Q/E | 头动（仅模拟模式） |

## Unity 场景搭建（手动）

1. 打开 `Assets/_Project/Scenes/Bootstrap.unity`
2. 创建空物体 `BciSystem`，挂载：
   - `OpenSee.OpenSee`
   - `OpenSee.OpenSeeLauncher`（设置 `exePath` 为 facetracker.exe 绝对路径，`modelPath` 为 models 目录，`cameraIndex=0`，`autoStart=false`）
   - `BciInputRouter`
   - `CompensationBciInputProvider`
   - `MockBciInputProvider`
   - `HybridBciInputProvider`（占位）
   - `BciTestHud`
3. 在 `BciInputRouter` 上绑定上述三个 Provider
4. Play → 按 **T** 启动摄像头追踪，滚轮调专注度

也可直接挂 **`BciTestBootstrap`**，会自动创建并配置（使用默认 OpenSeeFace 路径）。

## 接入正式 HybridBCI 时

1. 实现 `HybridBciInputProvider` 中 `// TODO` 部分（蓝牙/SDK）
2. `BciInputRouter.useCompensation = false`
3. 游戏逻辑无需修改（仍通过 `IBciInputProvider` 读取）

## 局限

- 代偿 **不能** 真实反映脑电专注力，滚轮仅用于手动测试游戏难度曲线
- 头动识别基于面部旋转，精度低于 IMU
- 眨眼阈值需按用户校准（`CompensationBciInputProvider.blinkThreshold`）
