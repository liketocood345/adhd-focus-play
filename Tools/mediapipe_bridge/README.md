# MediaPipe 动作追踪桥接

代偿模式通过本目录 Python 进程采集 USB 摄像头，向 Unity 发送 UDP JSON（默认 `127.0.0.1:9878`）。

## 能力

| 模块 | 说明 |
|------|------|
| FaceMesh `refine_landmarks` | 与 EasyVtuberStudio 同级：眨眼、头动、眼球 |
| BlazePose | 肩、颈、举手 |
| 预览窗 | `--preview 1`（默认开启） |

## 安装

推荐使用 EasyVtuber 自带 Python（已含 mediapipe）：

```
f:\EasyVtuber\EasyVtuber_v0.8.1\EasyVtuber_v0.8.1\envs\python_embedded\python.exe -m pip install -r requirements.txt
```

或系统 Python 3.10+：

```
pip install -r requirements.txt
```

## 手动运行

```bash
python tracker.py --camera 0 --width 640 --height 360 --fps 24 --preview 1
python tracker.py --list-cameras
```

Unity Play 时代偿模式会自动启动本脚本。
