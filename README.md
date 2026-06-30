# adhd-focus-play

面向 ADHD 儿童的脑机接口注意力游戏训练系统（Unity）。

**仓库地址：** https://github.com/liketocood345/adhd-focus-play  
参赛作品：**第十一届全国大学生生物医学工程创新设计竞赛** — 基于脑机接口的注意力缺陷多动障碍儿童精准化注意力游戏训练系统。

当前为 **BCI 测试版**：暂无 HybridBCI 头环时，可通过 USB 摄像头 + OpenSeeFace 代偿运行。

## 仓库结构

根目录仅包含文件夹与本文档：

```
.
├── README.md
├── Assets/              # Unity 资源与脚本
├── Docs/                # 开发文档
└── ProjectSettings/     # Unity 项目配置
```

## 功能概览

| 注意力维度 | 游戏（基础篇） | BCI 输入 |
|-----------|----------------|----------|
| 选择性注意 | 听音寻宝 | 专注力调音量 + 眨眼 |
| 持续性注意 | 无尽跑酷者 | 专注力调速 + 头动变道 |
| 注意力转移 | 指令反转游戏 | 专注力定节奏 + 头动确认 |
| 注意力分配 | 双线救援 | 专注力定间隔 + 眨眼/点头 |
| 注意力抑制 | 红灯停，绿灯行 | 专注力定速度 + 眨眼抑制 |

## 快速开始

### 环境

- Unity **2022.3 LTS** 或更高
- Windows（OpenSeeFace 代偿模式）
- 可选：USB 摄像头；正式设备为 HybridBCI 单通道脑电头环

### 运行测试版

1. 用 Unity Hub 打开本仓库根目录（含 `Assets` 的文件夹）
2. 打开场景 `Assets/_Project/Scenes/Bootstrap.unity`
3. 点击 Play（场景已挂 `BciTestBootstrap`）
4. 若使用代偿模式，需本机已安装 OpenSeeFace（见下方路径配置）

### 测试操作

| 操作 | 功能 |
|------|------|
| 鼠标滚轮 | 调节专注力（0–100） |
| `C` | 开关代偿模式 |
| `T` | 启动/停止 OpenSeeFace 摄像头追踪 |
| 空格 / W / S / Q / E | 代偿关闭时的模拟眨眼与头动 |

## BCI 架构

```
游戏模块 (*Controller)
        │
        ▼
  BciInputRouter          ← 统一入口 (IBciInputProvider)
        │
   ┌────┼────┬──────────────┐
   ▼    ▼    ▼              ▼
 代偿  模拟  HybridBCI     （未来 SDK）
 OSF   键盘   正式头环
```

- **代偿模式**：摄像头 + OpenSeeFace（眨眼、头动）；专注力由滚轮手动设置  
- **模拟模式**：键盘 + 滚轮，无摄像头  
- **正式模式**：`HybridBciInputProvider` 待接入琶洲实验室 HybridBCI 平台  

详见 [Docs/BciCompensationDevGuide.md](Docs/BciCompensationDevGuide.md)。

## OpenSeeFace 配置

代偿模式依赖本机 EasyVtuber 目录中的 OpenSeeFace（不纳入本仓库）。默认路径在 `BciTestBootstrap` 中：

- 追踪程序：`{EasyVtuber}\OpenSeeFace-v1.20.4\Binary\facetracker.exe`
- 模型目录：`{EasyVtuber}\OpenSeeFace-v1.20.4\models\`

可在 Inspector 中修改为实际安装路径。

## 文档

| 文档 | 说明 |
|------|------|
| [BciCompensationDevGuide.md](Docs/BciCompensationDevGuide.md) | 接口骨架与代偿开发 |
| [OpenSourceReferences.md](Docs/OpenSourceReferences.md) | 玩法对应的开源游戏参考 |

## 开源参考（节选）

| 玩法 | 推荐仓库 |
|------|----------|
| 无尽跑酷 + EEG | [Hira-Runner](https://github.com/Ruaneri-Portela/Hira-Runner) |
| 反应抑制 | [response-inhibition-game-unity-project](https://github.com/jpickavance/response-inhibition-game-unity-project) |
| Unity BCI 范例 | [BrainForm](https://github.com/BRomans/BrainForm) |

## 许可证

本项目采用 [Apache License 2.0](LICENSE)。  
`Assets/_Project/Plugins/OpenSeeFace/` 另遵循 [OpenSeeFace](https://github.com/emilianavt/OpenSeeFace) 原项目许可。

## 相关链接

- 正式 BCI 平台：琶洲实验室 HybridBCI 科研科创平台  
- OpenSeeFace：https://github.com/emilianavt/OpenSeeFace  
