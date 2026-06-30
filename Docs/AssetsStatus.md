# 美术与音频素材状态

## 当前仓库内

| 类型 | 状态 |
|------|------|
| 3D 预制体 | **未入库**（`Resources/Art/ThirdParty/` 仅 `.gitkeep`） |
| 音频文件 | **无** `.wav`/`.mp3`；运行时由 `ProceduralToneUtility` / `GameAudioLibrary` 生成 |
| UI 图 | `UiSprites` 程序化色块 |
| 字体 | 系统默认（`UiFonts.Default`） |

游戏在缺素材时自动回退 **Primitive 几何体 + 纯色**（见各 `*GameSession.cs`）。

---

## 开源仓库可提供什么

执行 Unity 菜单 **ADHD Training → Import Open Source Art**（或 `Tools/import_oss_art/import.ps1`）可从下列仓库复制美术：

| 仓库 | 许可 | 适用游戏 | 可提供 |
|------|------|----------|--------|
| [AwesomeRunner](https://github.com/VladimirPirozhenko/AwesomeRunner) | MIT | 无尽跑酷者 | 角色、障碍、金币等 **3D/贴图**（需仓库内实际含 prefab） |
| [response-inhibition-game](https://github.com/jpickavance/response-inhibition-game-unity-project) | 未标明 | 红灯停绿灯行 | 收集器、晶石、陨石类 sprite/prefab |
| [Hira-Runner](https://github.com/Ruaneri-Portela/Hira-Runner) | 未标明 | 跑酷（备选） | 跑酷相关资源 |
| [infinity-run](https://github.com/LuzMurilo/infinity-run) | MIT | 跑酷（备选） | 车道跑酷资源 |

导入后运行 **Link Art Resource Bindings** 更新 `art_bindings.json`。

---

## 开源中 **没有**、需自行准备的内容

以下在对照的开源项目中 **无现成可用资源**，竞赛演示需自行制作或采购：

| 游戏 | 缺失素材 | 说明 |
|------|----------|------|
| **听音寻宝** | 动物/环境 **真实音效** | 开源仅有范式；当前用 880Hz/220Hz 正弦波代替目标/干扰声 |
| **听音寻宝** | 场景/UI 插画 | 无专用 Unity 开源包 |
| **指令反转** | 角色示范动画、指令语音（TTS） | StroopApp 等为桌面范式，无儿童向美术 |
| **双线救援** | 左右屏动物/无人机模型与音效 | track-of-thought 为 Java，美术不可直接复用 |
| **无尽跑酷者** | 儿童向角色与场景（若 AwesomeRunner 风格不合） | 可换 Hira-Runner / infinity-run |
| **红灯停绿灯行** | 若 ResponseInhibition 仓库无匹配 prefab | 仍为方块/球体占位 |
| **全局** | 主菜单背景、Logo、统一 HUD 皮肤 | 需设计稿 |
| **全局** | 中文配音 / 指令朗读 | 无开源 TTS 集成 |

---

## 音频（已实现）

| 用途 | 实现 |
|------|------|
| 听音寻宝 目标/干扰 | `ProceduralToneUtility` 双音轨 |
| 得分正误 | `GameAudioLibrary.Correct` / `Wrong` |
| 开源 BGM/SFX | 导入脚本会复制 `.wav/.ogg`，但 **尚未** 在游戏代码中绑定路径 |

若你从 AwesomeRunner 等仓库导入 `Audio/` 文件夹，需在 `GameAudioLibrary` 或各 Session 中增加 `Resources.Load<AudioClip>` 绑定。

---

## 推荐下一步

1. 本机运行 Import Open Source Art（需 Git 与网络）
2. 在 Unity 中检查 `Resources/Art/ThirdParty/*` 是否有 prefab
3. 听音寻宝：从 [freesound.org](https://freesound.org) 等获取 CC0 动物声，放入 `Resources/Audio/Selective/`
4. 主菜单：提供一张 1920×1080 背景图即可显著提升观感
