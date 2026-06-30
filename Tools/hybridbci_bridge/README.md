# HybridBCI → Unity UDP 桥接

Unity 侧配置 `Assets/_Project/Config/bci_transport.json`：

```json
{ "transportType": "udp", "port": 9876 }
```

## 演示发送器

```bash
python Tools/hybridbci_bridge/udp_demo_sender.py
```

帧格式（JSON）：

```json
{ "focus": 72, "blink": false, "head": "nod" }
```

`head` 可选：`nod` | `shake` | `turnleft` | `turnright` | `none`

在 Unity 中选择 **脑机** 输入模式后即可接收。
