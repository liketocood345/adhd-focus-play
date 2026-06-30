# 文档玩法 × 开源游戏对照表

对照来源：`f:\b_g\桂林医科大学护理学院参赛作品报告=张明榕.docx`

说明：
- **高匹配**：核心玩法机制高度一致，可直接 fork 改造
- **中匹配**：范式一致（如 Go/No-Go、Stroop），需较大改造或换输入方式（键鼠→BCI）
- **低匹配**：仅训练同一认知维度，玩法差异较大

---

## 1. 听音寻宝（选择性注意）

**文档玩法**：多种混合音效 + 目标声识别；专注力调节目标声清晰度；听到目标时眨眼得分。

| 匹配度 | 项目 | 技术栈 | 链接 | 可借鉴点 |
|--------|------|--------|------|----------|
| 高 | **ChrisBrooksbank/attention** | React/TS 网页 | https://github.com/ChrisBrooksbank/attention | 选择性注意训练，目标/干扰分离，d' 等指标 |
| 中 | **kelokko/ear-sync** | Web Audio 网页 | https://github.com/kelokko/ear-sync | 听觉辨别、模式检测、自适应难度 |
| 中 | **ehsanmir/Migration** | UE4 HTML5 | https://github.com/ehsanmir/Migration | 鸟群方向识别，忽略干扰物（视觉版选择性注意） |
| 低 | **vb000/SemanticHearing** | 音频 ML | https://github.com/vb000/SemanticHearing | 目标声提取算法，非游戏 |

**Unity 缺口**：未找到“混合音效 + 目标声识别 + 眨眼响应”的完整 Unity 开源实现。建议以 `attention` 的范式逻辑为参考，在 Unity 用 `AudioMixer` 实现专注力驱动的音量/滤波。

---

## 2. 无尽跑酷者（持续性注意）

**文档玩法**：无尽跑酷；专注力控制速度与得分倍率；头动左右变道；低专注自动暂停。

| 匹配度 | 项目 | 技术栈 | 许可 | 链接 | 可借鉴点 |
|--------|------|--------|------|------|----------|
| **极高** | **Ruaneri-Portela/Hira-Runner** | Unity + NeuroSky | 未标明 | https://github.com/Ruaneri-Portela/Hira-Runner | **专为 ADHD 注意力训练设计的无尽跑酷 + EEG 专注力控制** |
| 高 | **giuliatondin/unity-mindwave-runner** | Unity + MindWave | - | https://github.com/giuliatondin/unity-mindwave-runner | Hira-Runner 原版，脑电控制跑酷 |
| 高 | **VladimirPirozhenko/AwesomeRunner** | Unity 3D | MIT | https://github.com/VladimirPirozhenko/AwesomeRunner | 跑酷架构、对象池、状态机 |
| 高 | **zeeshan020dev/Endless-Runner-Game** | Unity 6 | 无明确许可 | https://github.com/zeeshan020dev/Endless-Runner-Game | 三车道变道、障碍生成 |
| 高 | **LuzMurilo/infinity-run** | Unity 移动端 | MIT | https://github.com/LuzMurilo/infinity-run | 多车道滑动切换 |
| 中 | **hshwang34/OpenBCIUnityProject** | Unity + OpenBCI | - | https://github.com/hshwang34/OpenBCIUnityProject | 专注力指数驱动上下移动，可改为速度倍率 |
| 中 | **0xnazmul/Runner-3D** | Unity | MIT | https://github.com/0xnazmul/Runner-3D | 完整跑酷模板 |

**首选 fork**：`Hira-Runner`（玩法目标最接近），将 NeuroSky 接口替换为 HybridBCI。

---

## 3. 指令反转游戏（注意力转移）

**文档玩法**：显示/语音指令 → 做相反动作；专注力控制指令间隔；点头确认完成、摇头表示无法完成。

| 匹配度 | 项目 | 技术栈 | 许可 | 链接 | 可借鉴点 |
|--------|------|--------|------|------|----------|
| 高 | **MylanBeghin/StroopApp** | WPF 桌面 | MIT | https://github.com/MylanBeghin/StroopApp | **任务切换范式**：圆形=读颜色，方形=读文字（类似“反转规则”） |
| 中 | **huxianyin/Stroop_Web** | Web | - | https://github.com/huxianyin/Stroop_Web | 可配置 Stroop 试次 |
| 中 | **bsdlab/dp-stroop** | Python | - | https://github.com/bsdlab/dp-stroop | 支持 focus 标志切换读色/读词 |
| 中 | **loethen/freefocusgames** | Next.js | - | https://github.com/loethen/freefocusgames | Stroop + 认知灵活性训练合集 |
| 低 | **zinodaks/Simon-Game** 等 | Unity | - | https://github.com/zinodaks/Simon-Game | 记忆序列，非反转指令 |

**Unity 缺口**：无“Simon Says 反转 + 头动确认”开源 Unity 项目。可参考 `StroopApp` 的任务切换逻辑移植。

---

## 4. 双线救援（注意力分配）

**文档玩法**：左右屏独立事件；左眨眼救动物，右点头救无人机；专注力控制事件间隔。

| 匹配度 | 项目 | 技术栈 | 链接 | 可借鉴点 |
|--------|------|--------|------|----------|
| 高 | **macie-k/track-of-thought** | Java/JavaFX | https://github.com/macie-k/track-of-thought | **明确标注 divided attention**：同时引导多个球到站点 |
| 中 | **sojinantony01/brain-development-games** | React | https://github.com/sojinantony01/brain-development-games | Dual Task Challenge：形状计数 + 数学并行 |
| 中 | **sourav15mukherjee/split_focus** | JS 网页 | https://github.com/sourav15mukherjee/split_focus | 分裂注意力小游戏 |
| 低 | **brodie-neuro/WAND-practice-and-fatigue-induction** | PsychoPy | https://github.com/brodie-neuro/WAND-practice-and-fatigue-induction | Dual N-back 研究范式，非儿童向 |

**Unity 缺口**：无左右分屏 + 不同动作响应的开源 Unity 游戏。`track-of-thought` 的多目标并行逻辑最有参考价值。

---

## 5. 红灯停，绿灯行（注意力抑制）

**文档玩法**：晶石掉落 → 眨眼收集；陨石 → 抑制眨眼；专注力控制掉落速度；摇头清陨石（限 3 次）。

| 匹配度 | 项目 | 技术栈 | 许可 | 链接 | 可借鉴点 |
|--------|------|--------|------|------|----------|
| 高 | **jpickavance/response-inhibition-game-unity-project** | Unity | - | https://github.com/jpickavance/response-inhibition-game-unity-project | **Unity 反应抑制游戏 Fruitbat Splat** |
| 高 | **RealityBending/DoggoNogo** | JS 网页 | - | https://github.com/RealityBending/DoggoNogo | 经典 Go/No-Go 神经心理学范式 |
| 高 | **Anyma-exe/cognitive-arcade** | JS 网页 | MIT | https://github.com/Anyma-exe/cognitive-arcade | 含 Go/No-Go + Stroop + N-Back |
| 中 | **RATWHAT96/AssessmentToolForCognitiveDysfunction** | Unity 3D | - | https://github.com/RATWHAT96/AssessmentToolForCognitiveDysfunction | 认知抑制评估任务脚本 |
| 中 | **neuropsychology/CognitiveControl** | Python | - | https://github.com/neuropsychology/CognitiveControl | 抑制控制游戏原型（规划中） |
| 低 | **nathanyaqueby/blink-and-bloom** | Web + MediaPipe | - | https://github.com/nathanyaqueby/blink-and-bloom | 眨眼检测花园游戏，非 Go/No-Go |

**首选 fork**：`response-inhibition-game-unity-project`（Unity 原生抑制游戏），叠加眨眼输入与掉落物机制。

---

## BCI / 专注力集成（跨游戏通用）

| 匹配度 | 项目 | 链接 | 说明 |
|--------|------|------|------|
| 高 | **BRomans/BrainForm** | https://github.com/BRomans/BrainForm | Unity BCI 严肃游戏，MIT，Unicorn SDK |
| 高 | **hshwang34/OpenBCIUnityProject** | https://github.com/hshwang34/OpenBCIUnityProject | Unity UDP 接收专注力/放松度指数 |
| 高 | **Ruaneri-Portela/Hira-Runner** | https://github.com/Ruaneri-Portela/Hira-Runner | ADHD 跑酷 + 脑电专注力（最接近本项目） |
| 中 | **Atul-Acharya-17/MindZone** | https://github.com/Atul-Acharya-17/MindZone | 专注力控制伤害倍率 |
| 中 | **nathanyaqueby/blink-and-bloom** | https://github.com/nathanyaqueby/blink-and-bloom | MediaPipe 眨眼检测（若不用 HybridBCI EOG） |

---

## 推荐实施路线

1. **持续性注意**：fork `Hira-Runner` → 替换为 HybridBCI 接口
2. **注意力抑制**：参考 `response-inhibition-game-unity-project` + 自定义掉落物
3. **选择性注意**：自研音频混合器 + 参考 `ChrisBrooksbank/attention` 范式
4. **注意力转移**：参考 `StroopApp` 任务切换逻辑
5. **注意力分配**：参考 `track-of-thought` 多目标并行

## 许可证提醒

fork 前请检查各仓库 LICENSE。商业/竞赛用途优先选择 MIT、Apache-2.0 等明确许可的项目。
