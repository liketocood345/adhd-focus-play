# adhd-focus-play

面向 ADHD 儿童的**脑机接口注意力游戏训练系统**（Unity 6）。将 HybridBCI 头环信号（或摄像头代偿输入）映射为五类注意力维度的可玩关卡，支持会话记录与竞赛演示。

**仓库：** https://github.com/liketocood345/adhd-focus-play  
**竞赛：** 第十一届全国大学生生物医学工程创新设计竞赛

---

## 功能概览

| 模块 | 说明 |
|------|------|
| **五款训练游戏** | 听音寻宝、无尽跑酷者、指令反转、双线救援、红灯停绿灯行 |
| **三种输入模式** | 代偿（摄像头）、脑机 UDP（预留）、键鼠模拟 |
| **MediaPipe 代偿** | EVTS 级面部（眨眼/头动/注视）+ 肩颈举手；USB 摄像头 DirectShow |
| **眼球屏幕校准** | 左右屏心校准 → 归一化注视坐标（粗粒度，非眼动仪精度） |
| **视频源记忆** | 摄像头/视频文件、分辨率与帧率 PlayerPrefs 持久化 |
| **会话 CSV** | 每局得分、正确率、平均专注力自动写入本地 |
| **开源美术管线** | 可选导入 AwesomeRunner / ResponseInhibition 等资源 |

---

## 快速开始

### 环境

- **Unity** 6000.5.0f1（Windows，Input Manager）
- **代偿模式：** Python 3.10+ 与 MediaPipe（推荐 EasyVtuber 嵌入式 Python）

```powershell
# 安装 MediaPipe 桥接依赖（路径按本机调整）
python -m pip install -r Tools\mediapipe_bridge\requirements.txt
```

### 运行

1. Unity Hub 打开本仓库
2. 菜单 **ADHD Training → Setup All Scenes And Build Settings**（首次）
3. 打开 `Assets/_Project/Scenes/MainMenu.unity` → **Play**
4. 右下角选 **代偿**，右侧配置 USB 摄像头；可选 **屏幕校准**

键鼠测试：切换 **键鼠** 模式（空格眨眼，W/S/A/D 头动，Q/E 举手）。

---

## 代偿架构

```
USB 摄像头 → Tools/mediapipe_bridge/tracker.py
              FaceMesh + BlazePose → UDP :9878
           → MediaPipeMotionClient (Unity)
           → BciInputSnapshot → 游戏 Session
```

面部算法移植自 EasyVtuberStudio `get_pose`；详细映射见 [设计手册 §5](Docs/设计手册.md)。

---

## 文档

| 文档 | 说明 |
|------|------|
| **[设计手册.md](Docs/设计手册.md)** | 架构、BCI、UI、配置、开发规范（主文档） |
| **[Docs/Games/](Docs/Games/)** | 五款游戏玩法与信号映射 |
| [BciCompensationDevGuide.md](Docs/BciCompensationDevGuide.md) | BCI / 代偿接口与场景搭建 |
| [AssetsStatus.md](Docs/AssetsStatus.md) | 美术/音频素材状态与开源缺口 |
| [OpenSourceReferences.md](Docs/OpenSourceReferences.md) | 玩法 × 开源项目对照表 |

---

## 许可证

[Apache License 2.0](LICENSE)
