# 网络架构说明文档

## 概述

本项目采用 **Mihon 式混合架构** 处理网络请求，兼顾性能和 Cloudflare 兼容性。

## 核心组件

### 1. NetUtils (主入口)

**文件**: `Masiro/Utils/NetUtils.cs`

**职责**: 所有网络请求的**统一入口**，内部自动处理 Cloudflare 绕过逻辑。

**使用场景**:
- ✅ **获取小说章节内容** - `MasiroHtmlWithBypass()`
- ✅ **获取登录 Token** - `GetTokenWithBypass()`
- ✅ **执行登录操作** - `LoginMasiroWithBypass()`
- ✅ **检查图片链接** - `IsImageUrl()` (无需绕过)

**示例**:
```csharp
// 获取章节内容（自动处理 Cloudflare）
var result = await NetUtils.MasiroHtmlWithBypass(cookies, subUrl, ownerWindow);

// 获取 Token（自动处理 Cloudflare）
var token = await NetUtils.GetTokenWithBypass(ownerWindow);

// 登录（自动处理 Cloudflare）
var loginResult = await NetUtils.LoginMasiroWithBypass(token, username, password, ownerWindow);
```

**工作原理**:
1. 首先尝试使用 RestSharp 发送请求
2. 如果响应包含 Cloudflare 验证页特征，自动弹出 WebView2 窗口
3. 用户完成验证后，自动获取 Cookie 和 HTML
4. 后续请求继续使用 RestSharp（携带已验证的 Cookie）

---

### 2. WebView2Service (底层服务)

**文件**: `Masiro/Services/WebView2Service.cs`

**职责**: 封装 WebView2 功能，提供 Cloudflare 绕过能力。

**使用场景**:
- ⚠️ **一般情况下不要直接使用**
- ⚠️ 仅在 NetUtils 无法满足需求时使用
- ✅ 需要**自定义 WebView2 行为**时
- ✅ 需要**直接操作浏览器**时

**方法**:
- `InitializeAsync()` - 初始化 WebView2 环境
- `BypassCloudflareAsync()` - 弹出窗口绕过 Cloudflare
- `NavigateAndGetHtmlAsync()` - 导航并获取 HTML
- `GetCookiesAsync()` / `SetCookies()` - Cookie 管理
- `IsCloudflareChallengePage()` - 检测 Cloudflare 页面

**示例** (仅供特殊需求):
```csharp
var service = new WebView2Service();
await service.InitializeAsync();

// 手动绕过 Cloudflare
var result = await service.BypassCloudflareAsync(url, ownerWindow);
if (result.Success) {
    var html = result.Html;
    var cookies = result.Cookies;
}
```

---

### 3. LoginUserControl (用户登录界面)

**文件**: `Masiro/Views/LoginUserControl.xaml.cs`

**职责**: 提供**可视化登录界面**，让用户在 WebView2 中手动完成登录。

**使用场景**:
- ✅ **用户首次登录**时
- ✅ **用户需要手动输入账号密码**时
- ✅ **需要可视化浏览器界面**时

**特点**:
- 独立的 WebView2 实例
- 长期显示，用户可随时操作
- 登录完成后提取 Cookie 保存到本地

**与 NetUtils 的关系**:
- LoginUserControl 获取的 Cookie 会被保存到 `data/user.json`
- NetUtils 后续使用这些 Cookie 进行请求

---

## 决策流程图

```
需要发送网络请求？
    │
    ├─► 是获取小说内容/Token/登录？
    │       │
    │       └─► 使用 NetUtils.*WithBypass() 方法
    │               │
    │               ├─► 正常响应 → 返回结果
    │               └─► 遇到 Cloudflare → 自动弹出 WebView2
    │
    ├─► 是检查图片链接？
    │       └─► 使用 NetUtils.IsImageUrl()
    │
    ├─► 是用户首次登录？
    │       └─► 使用 LoginUserControl (可视化界面)
    │
    └─► 需要自定义 WebView2 行为？
            └─► 直接使用 WebView2Service (罕见)
```

---

## 使用规范

### ✅ 推荐做法

```csharp
// 1. 导出小说章节 - 使用 NetUtils
var html = await NetUtils.MasiroHtmlWithBypass(cookies, subUrl, Window.GetWindow(this));

// 2. 获取 Token - 使用 NetUtils
var token = await NetUtils.GetTokenWithBypass(Window.GetWindow(this));

// 3. 登录 - 使用 NetUtils
var result = await NetUtils.LoginMasiroWithBypass(token, user, pass, Window.GetWindow(this));
```

### ❌ 避免做法

```csharp
// 不要直接创建 WebView2Service 实例来发送普通请求
var service = new WebView2Service();  // ❌ 错误
var html = await service.NavigateAndGetHtmlAsync(url);  // ❌ 错误

// 不要在业务代码中重复实现 Cloudflare 检测逻辑
if (IsCloudflarePage(content)) {  // ❌ 错误 - NetUtils 已经处理了
    // ...
}
```

---

## 架构优势

1. **性能优先** - 默认使用 RestSharp，速度快
2. **自动降级** - 遇到 Cloudflare 时自动切换到 WebView2
3. **用户友好** - 只在需要时才弹出验证窗口
4. **代码简洁** - 业务代码只需调用 NetUtils，无需关心底层实现
5. **维护简单** - Cloudflare 绕过逻辑集中在一个地方

---

## 故障排查

### 问题: WebView2 窗口没有弹出

**检查**:
1. 是否正确传递了 `ownerWindow` 参数？
2. WebView2 Runtime 是否已安装？
3. `WebView2Data` 目录是否有写入权限？

### 问题: Cloudflare 验证通过后仍然失败

**检查**:
1. Cookie 是否正确保存？
2. 请求时是否正确携带了 Cookie？
3. Cookie 是否已过期？

### 问题: 性能太慢

**说明**:
- 首次请求遇到 Cloudflare 时需要等待用户验证，这是正常的
- 验证通过后，后续请求会使用 RestSharp，速度会快很多
- 如果需要频繁请求，建议保持 Cookie 有效

---

## 更新记录

| 日期       | 变更                                             |
| ---------- | ------------------------------------------------ |
| 2026-04-09 | 初始版本，集成 WebView2 作为 Cloudflare 绕过方案 |
