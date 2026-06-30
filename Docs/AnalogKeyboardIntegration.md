# 压感 / 模拟行程键盘接入

纯键鼠模式（`BciInputMode.Mock`）通过 `IAnalogKeyboardSource` 分层读取按键行程，映射到 `BciInputSnapshot`。

## 分层优先级

1. **Wooting Analog SDK** — `WootingAnalogKeyboardSource`（检测到 SDK 时启用）
2. **磁轴 HID 档位** — `HidTierKeyboardSource`（F1–F24 行程档，见 [iatkb 磁轴文档](https://docs.iatkb.com/ci-zhou-gu-jian.html)）
3. **标准键鼠** — `StandardKeyboardSource`（滚轮专注 + 空格/W/S/Q/E）

实现入口：[`AnalogKeyboardSourcePicker.cs`](../Assets/_Project/Scripts/Core/Input/AnalogKeyboardSourcePicker.cs)

## 信号映射

| BCI 信号 | 标准键鼠 | 压感增强 |
|----------|----------|----------|
| 专注力 0–100 | 鼠标滚轮 | `F` 键按压行程 × 100 |
| 眨眼 | 空格按下 | 深按（行程 > 0.85）触发 |
| 点头/摇头/转头 | W / S / Q / E | 同键位，可扩展为双键行程差 |

键位配置：[`Assets/_Project/Config/analog_keyboard.json`](../Assets/_Project/Config/analog_keyboard.json)

## Wooting 接入步骤（待硬件验证）

1. 将 [wooting-analog-sdk](https://github.com/WootingKb/wooting-analog-sdk) 原生库放入 `Assets/_Project/ThirdParty/WootingSdk/`
2. 在 `WootingAnalogKeyboardSource` 中 P/Invoke `read_full_buffer`
3. `IsAvailable` 在 SDK 初始化成功后返回 `true`

## Input System（可选后续）

若启用 `com.unity.inputsystem`，可将 `Player Settings → Active Input Handling` 设为 **Both**，在 `HidTierKeyboardSource` 中通过 `InputSystem.devices` 解析 HID 报告。

当前项目保持 **Input Manager** + `BciLegacyInput`，与现有滚轮逻辑兼容。
