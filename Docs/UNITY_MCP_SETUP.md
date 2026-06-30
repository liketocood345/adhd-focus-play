# Unity MCP + Cursor（社区版）

本工程使用 [emeryporter/UnityMCP](https://github.com/emeryporter/UnityMCP)（本地 HTTP `localhost:8080`，无 Unity AI 套餐限制）。

## 参数

| 项 | 值 |
|----|-----|
| Unity | **6000.5.0f1** |
| 工程路径 | `f:\ADHD-Attention-Training` |
| MCP 包 | `Packages/com.emeryporter.unitymcp`（嵌入，2.2.4） |
| HTTP | `http://localhost:8080/` |
| Cursor 桥 | `scripts/unity_mcp_stdio_bridge.mjs` |

## 一次性步骤

### Unity Editor

1. Hub 打开 **`f:\ADHD-Attention-Training`**（含 `Assets` + `ProjectSettings`）。
2. 等待首次编译完成（会加载 MCP 包 + Newtonsoft.Json）。
3. **Window → Unity MCP → Status → Start Server**（默认端口 8080）。

### Cursor

工程内已配置 [`.cursor/mcp.json`](../.cursor/mcp.json)。若全局 `~/.cursor/mcp.json` 里已有 `unity-community`，可改为指向本工程桥脚本：

```json
"unity-community": {
  "command": "C:\\Program Files\\nodejs\\node.exe",
  "args": ["f:\\ADHD-Attention-Training\\scripts\\unity_mcp_stdio_bridge.mjs"]
}
```

修改后**重启 Cursor**，在 Settings → Tools & MCP 确认 `unity-community` 已连接且工具数 > 0。

## 验证

```powershell
f:\ADHD-Attention-Training\scripts\verify_unity_mcp.ps1
```

在对话中可让 Agent 调用 `read_console` 读取 Unity Console。

## 排障

| 现象 | 处理 |
|------|------|
| Window 无 Unity MCP | 等编译完成；Package Manager 应显示 Unity MCP |
| Cursor 红/0 tools | Unity 先 Start Server；重启 Cursor |
| 8080 占用 | Unity MCP 窗口改端口；设环境变量 `UNITY_MCP_URL` 或改桥脚本默认 URL |
| Window 无 Unity MCP | Console 有 `UnityMCP.Editor` 编译错误时菜单不会出现；确认 `manifest.json` 含 `com.unity.test-framework` 后重开工程 |
| 多工程切换 | 同一时刻只有一个 Unity 实例应占用 8080 |

## 超时规则（20 秒）

| 层级 | 配置 | 说明 |
|------|------|------|
| Cursor | `.cursor/mcp.json` → `"timeout": 20000` | MCP 工具调用最长等待 20s |
| 桥脚本 | `UNITY_MCP_TIMEOUT_MS=20000` | HTTP 转发无响应则返回 `-32001` |
| Unity MCP | `refresh_unity` 的 `wait_for_ready` 最多等 20s | 编译中请 `wait_for_ready: false` 轮询 |
| Agent 建议 | 避免 `refresh_unity` + `wait_for_ready: true` + `compile: request` 组合 | 域重载时易超过 20s |

修改 `mcp.json` 或桥脚本后需 **重启 Cursor**；修改 Unity MCP 包后需 **重编译/重开 Unity**。
