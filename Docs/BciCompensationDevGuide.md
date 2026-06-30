# BCI 接口骨架与代偿模式开发说明

## 背景

正式设备为 **HybridBCI 单通道脑电头环**（专注力 0–100、眨眼 EOG、头动 IMU）。  
当前暂无头环，测试版通过 **代偿模式** 用 USB 摄像头 + MediaPipe 模拟部分输入。

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
(MediaPipe) (键盘)       (未来 SDK)
```

| 信号 | HybridBCI（正式） | 代偿模式（测试） | 模拟模式（无摄像头） |
|------|-------------------|------------------|----------------------|
| 专注力 0–100 | 平台解码算法 | **鼠标滚轮**手动调节 | **鼠标滚轮** |
| 眨眼 | EOG | MediaPipe 眼睑开合 | 空格 |
| 点头/摇头/转头 | IMU | MediaPipe 头姿 | W/S/A/D |
| 举手 | — | MediaPipe 腕高于肩 | Q/E（边沿） |
| 身体倾 | — | MediaPipe 肩线偏移 | — |

## 开发环境

- Unity **6000.5.0f1**（Unity 6）
- 输入：**Input Manager**（非 Input System Package）
- 目标平台：Windows（HybridBCI 蓝牙头环 / MediaPipe 代偿）

MediaPipe 桥接：

- 脚本目录：`Tools/mediapipe_bridge/`
- Unity 客户端：`Assets/_Project/Scripts/Core/MediaPipe/`
- 默认 Python：`f:\EasyVtuber\EasyVtuber_v0.8.1\...\envs\python_embedded\python.exe`（`AppRoot` 可改）
- 安装依赖：`pip install -r Tools/mediapipe_bridge/requirements.txt`
- 追踪 UDP：`127.0.0.1:9878`（JSON 帧）
- USB 摄像头：OpenCV `CAP_DSHOW`（Windows DirectShow）

## 代偿与模式切换

- 组件：`BciInputRouter.SetInputMode(BciInputMode)`
- 右下角 HUD 三按钮：**代偿 / 脑机 / 键鼠**
- 选择持久化于 `PlayerPrefs`（键 `adhd_bci_input_mode`）
- 旧测试场景 `Bootstrap` 仍可用 **`C`** 切换代偿

## 主菜单与场景

| 场景 | 脚本 |
|------|------|
| `MainMenu.unity` | `MainMenuController` + 运行时 `AppRoot` |
| `Games/Game_Selective.unity` | `SelectiveGameSession` |
| `Games/Game_Sustained.unity` | `SustainedGameSession` |
| `Games/Game_Divided.unity` | `DividedGameSession` |
| `Games/Game_Shifting.unity` | `ShiftingGameSession` |
| `Games/Game_Inhibition.unity` | `InhibitionGameSession` |

菜单 **ADHD Training → Setup All Scenes And Build Settings** 可一键生成场景并写入 Build Settings。

## 会话记录 CSV

- 服务：`SessionRecordService`
- 路径：`Application.persistentDataPath/session_records.csv`
- 每局 `GameSessionBase.EndSession()` 自动写入

## 脑机传输（非 Unity 插件）

- 接口：`IExternalBciTransport`（`BciTransport/`）
- 配置：`Assets/_Project/Config/bci_transport.json` 或 `Resources/bci_transport.json`
- 默认 UDP `127.0.0.1:9876`；演示脚本：`Tools/hybridbci_bridge/udp_demo_sender.py`

## Unity 场景搭建（Bootstrap 手动，可选）

1. 打开 `Assets/_Project/Scenes/Bootstrap.unity`
2. 创建空物体 `BciSystem`，挂载：
   - `BciInputRouter`
   - `CompensationBciInputProvider`
   - `MockBciInputProvider`
   - `HybridBciInputProvider`（占位）
   - `BciTestHud`
3. 在 `BciInputRouter` 上绑定上述三个 Provider；`CompensationBciInputProvider` 配置 Python 路径
4. Play → 代偿模式自动启动 `tracker.py`，滚轮调专注度

也可直接挂 **`BciTestBootstrap`**，会自动创建并配置（使用默认 Python 路径）。

## 接入正式 HybridBCI 时

1. 查阅官网协议，实现或扩展 `HybridBciUdpTransport` / 串口 Transport
2. `BciInputRouter.SetInputMode(BciInputMode.HybridBci)`
3. 游戏逻辑无需修改（仍通过 `IBciInputProvider` 读取）

## 局限

- 代偿 **不能** 真实反映脑电专注力，滚轮仅用于手动测试游戏难度曲线
- 头动/举手基于摄像头 2D 姿态，精度低于 IMU
- 眨眼阈值需按用户校准（`CompensationBciInputProvider.blinkThreshold`）
