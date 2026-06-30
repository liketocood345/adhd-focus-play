#!/usr/bin/env node
/**
 * Minimal stdio -> Unity MCP HTTP bridge for Cursor.
 * Forwards one JSON-RPC line to http://127.0.0.1:8080/ (plain POST, no Streamable HTTP).
 *
 * Timeout: UNITY_MCP_TIMEOUT_MS (default 20000). No response within this window → JSON-RPC error.
 */
import readline from "node:readline";

const ENDPOINT = process.env.UNITY_MCP_URL ?? "http://127.0.0.1:8080/";
const TIMEOUT_MS = Number.parseInt(process.env.UNITY_MCP_TIMEOUT_MS ?? "20000", 10);

function writeJsonRpcError(id, message, code = -32603) {
  const payload = {
    jsonrpc: "2.0",
    id: id ?? null,
    error: { code, message },
  };
  process.stdout.write(`${JSON.stringify(payload)}\n`);
}

async function forwardRequest(line) {
  const trimmed = line.trim();
  if (!trimmed) {
    return;
  }

  let requestId = null;
  try {
    const parsed = JSON.parse(trimmed);
    requestId = parsed?.id ?? null;
  } catch {
    writeJsonRpcError(null, "Invalid JSON input");
    return;
  }

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);

  try {
    const response = await fetch(ENDPOINT, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: trimmed,
      signal: controller.signal,
    });

    const text = await response.text();
    if (!response.ok) {
      writeJsonRpcError(requestId, `Unity MCP HTTP ${response.status}: ${text}`);
      return;
    }

    process.stdout.write(`${text.trim()}\n`);
  } catch (error) {
    if (error?.name === "AbortError") {
      writeJsonRpcError(
        requestId,
        `Unity MCP timed out after ${TIMEOUT_MS}ms at ${ENDPOINT}. Avoid wait_for_ready on long compiles; poll refresh_unity separately.`,
        -32001,
      );
      return;
    }

    writeJsonRpcError(
      requestId,
      `Unity MCP unreachable at ${ENDPOINT}. Start Window -> Unity MCP -> Start Server. ${error}`,
    );
  } finally {
    clearTimeout(timer);
  }
}

const rl = readline.createInterface({ input: process.stdin, terminal: false });
for await (const line of rl) {
  await forwardRequest(line);
}
