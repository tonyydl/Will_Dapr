# Will_Dapr

本專案根據 [Will 保哥的教學文章](https://blog.miniasp.com/post/2021/10/19/Developing-Microservice-apps-using-Dapr-locally) 實作，示範如何在本機使用 Dapr 開發微服務應用程式。

> **注意：** 本實作使用 .NET 9.0 + Dapr 1.18.4，版本較原文更新。Controller 命名也與原文略有不同（詳見下方說明）。

---

## 專案架構

```
Will_Dapr/
├── DaprCounter/          # Console 客戶端應用程式
└── DaprCounterASPNET/    # ASP.NET Core Web API 服務
```

### 運作流程

```
DaprCounter (Console)
    │
    │ Dapr Service Invocation (PUT /counter/counter)
    ▼
DaprCounterASPNET (ASP.NET Core Web API)
    │
    │ Dapr State Management
    ▼
statestore (Redis / 本機 Dapr 預設 State Store)
```

1. **DaprCounterASPNET**：提供 REST API，透過 Dapr State Store 持久化計數器狀態。
2. **DaprCounter**：每秒呼叫一次 `DaprCounterASPNET` 的 PUT 端點，讓計數器遞增並印出目前數值。

---

## 前置需求

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) 已安裝並初始化

確認 Dapr 已正確初始化（本機 Docker 模式）：

```bash
dapr init
dapr --version
```

---

## 啟動方式

需要**同時開兩個終端機**分別執行兩個服務。

### 終端機 1：啟動 DaprCounterASPNET

```bash
cd DaprCounterASPNET
dapr run --app-id DaprCounterASPNET --app-port 5000 -- dotnet run --launch-profile http
```

- `--app-id DaprCounterASPNET`：Dapr 服務識別名稱，DaprCounter 用此名稱呼叫本服務
- `--app-port 5000`：對應 `launchSettings.json` 中 `http` profile 的 port

### 終端機 2：啟動 DaprCounter

```bash
cd DaprCounter
dapr run --app-id DaprCounter -- dotnet run
```

啟動後，Console 每秒會印出遞增的計數器數值：

```
Counter = 1
Counter = 2
Counter = 3
...
```

---

## API 端點

Base URL（透過 Dapr Sidecar 直接呼叫）：`http://localhost:5000`

| 方法  | 路由                | 說明                                 |
|-------|---------------------|--------------------------------------|
| GET   | `/counter/counter`  | 讀取目前計數器數值                   |
| PUT   | `/counter/counter`  | 將計數器加 1 並回傳新數值            |

### 手動測試範例

```bash
# 讀取計數器
curl http://localhost:5000/counter/counter

# 讓計數器加 1
curl -X PUT http://localhost:5000/counter/counter
```

---

## Controller 命名說明

原文範例的路由設計讓 API 路徑較簡短（如 `/counter`）。

本實作的 `CounterController` 使用預設的 `[Route("[controller]")]`，Controller 名稱為 `Counter`，動作方法也命名為 `counter`，因此實際路由為 `/counter/counter`（controller 名 + action 名）。

`DaprCounter` 的 `InvokeMethodAsync` 呼叫已對應調整：

```csharp
// DaprCounter/Program.cs
var counter = await daprClient.InvokeMethodAsync<int>(
    HttpMethod.Put,
    "DaprCounterASPNET",   // Dapr app-id
    "counter/counter"       // 路由：/counter/counter
);
```

---

## 使用的套件版本

| 套件                          | 版本     |
|-------------------------------|----------|
| Dapr.Client                   | 1.18.4   |
| Dapr.AspNetCore               | 1.18.4   |
| Microsoft.AspNetCore.OpenApi  | 9.0.17   |
| .NET                          | 9.0      |

---

## 清理環境

完成實驗後，可執行以下指令移除 Dapr 在本機建立的 Docker 容器與設定檔：

```bash
dapr uninstall
```

這會移除 Redis、Zipkin 容器以及 `~/.dapr/` 目錄下的預設元件設定。

---

## 參考資料

- [在本機使用 Dapr 開發微服務應用程式 — Will 保哥](https://blog.miniasp.com/post/2021/10/19/Developing-Microservice-apps-using-Dapr-locally)
- [Dapr 官方文件](https://docs.dapr.io/)
- [Dapr .NET SDK](https://github.com/dapr/dotnet-sdk)
