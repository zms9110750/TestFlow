# TestFlow

这是一个由 **zmsTemplate** 生成的开源项目。

---

## 特性

### 编译自动格式化

项目根目录有 `.editorconfig`，每次 `dotnet build` 前自动执行 `dotnet format`。代码风格统一，无需手动整理。

### GitHub 工作流

`.github/workflows/ci.yml` 包含了完整的 CI/CD：

- **PR 到 master** — 自动 `dotnet restore` → `build` → `test`（可在仓库 Settings → Branches 配置 required checks 要求测试通过才能合并）；同时打包 PR 预览 nupkg 并评论附下载链接，可在 PR 评论 `@github-actions[bot] pack <sha7>` 重打包某个提交
- **推送 `v*` 标签** — 自动打包发版

### 集中配置

所有项目的版本号、作者、仓库地址统一写在 `Directory.Build.props` 中。修改版本只需改这一个文件。

### 解决方案结构

解决方案已按文件夹组织：

- `/src/` — 类库
- `/samples/` — GUI 示例
- `/test/` — 测试项目

---

## 项目说明


### GUI

基于 `Photino.Blazor` + `Masa.Blazor` 的跨平台桌面应用。无需 WPF/WinForms 依赖，Windows、Linux、macOS 均可运行。

窗口大小默认 1280×750。顶部导航栏右侧带有 Masa logo，点击跳转 [Masa 官方文档](https://docs.masastack.com/)。

### 类库

GUI 通过项目引用调用。
生成 XML 文档文件，裸用 DLL 也能看到注释提示。

---


---

## 开发流程

### 分支策略

```
master        ← 稳定分支，PR 合并目标
  └─ feature/xxx  ← 功能分支，from master 分出
```

所有改动在功能分支上进行，完成后提交 Pull Request 到 `master`。

- 分支命名：`feature/简短描述` 或 `fix/简短描述`
- PR 标题：清晰说明改动内容
- 合并方式：**Squash merge**（将分支上所有提交压缩为一个提交）

### 发版

GitHub Actions 支持 **三种发版方式**，效果相同：编译 + 测试 → 打 nupkg → 创建 **草稿** GitHub Release（草稿需在网页上手动点「Publish release」才正式发布）。

**方式一：推送标签**（适合命令行）

推送 `v*` 标签（如 `v0.1.0`）时自动触发：

```bash
git tag v0.1.0
git push origin v0.1.0
```

标签名即版本号（不带 `v` 前缀的 `0.1.0` 会写入 nupkg）。

**方式二：网页按钮**（适合不发 git 命令的人）

仓库 **Actions** 页 → 左侧选 **CI** → 右侧 **Run workflow** 按钮：

- 不填版本号：使用 `Directory.Build.props` 中的 `<Version>` 发版
- 填版本号（如 `1.2.3`）：用填的版本发版（自动补 `v` 前缀，打 `v1.2.3`）

**方式三：PR 合并 + 里程碑**（适合按迭代发版）

给 PR 挂一个 `vX.Y.Z` 格式的里程碑（如 `v0.2.0`），合并该 PR 时自动触发，发布 `vX.Y.Z-build.{N}`（N 从 0 自动递增，如 `v0.2.0-build.0`、`v0.2.0-build.1`）。同一里程碑多次合并会自动累加 build 号。

- 里程碑必须是纯三段版本号 `vX.Y.Z`，不能带 `-pre`/`+meta` 后缀
- 里程碑缺失或格式不对 → 不发版（不会阻止合并）

**发版后：**

1. Release 是**草稿**状态，去仓库 **Releases** 页检查产物（zip / nupkg）
2. 确认无误后点 **Publish release** 正式发布
3. 版本号会同时写入 Release 标签名和 nupkg 版本号。注意方式三（里程碑发版）的 build 号只进 tag（如 `v0.2.0-build.0`），nupkg 版本是里程碑本体（`0.2.0`）
4. **防重复**：某版本已正式发布后，再触发同名版本不会重复发版（CI 自动跳过，避免死循环）。若想重发，先在 Releases 页删除旧 Release 和 tag，再重新触发

---

## 分发说明

GitHub Actions CI 在 `v*` 标签推送时自动构建并发布以下产物：

| 类型 | 说明 |
|------|------|
| **自包含 zip** | 6 个 RID（win-x64/arm64, linux-x64/arm64, osx-x64/arm64），基于最高 TFM 发布 |
| **FDD zip** | 按 TFM 分组，同一 TFM 的多个 exe 合并到同一 zip |
| **NuGet** | 类库项目的 `.nupkg` 包 |

### 限制

- **.NET Framework**（net472/net48 等）：CI 运行在 Linux runner 上，不支持发布基于 Framework 的项目。如果你的项目必须发布 .NET Framework 版本，请在 Windows runner 上自行构建
- **自包含发布的目标框架**：CI 的 `Get-Highest` 函数自动选择项目中的最高 TFM（如 net6.0;net8.0;net9.0 选 net9.0）。如需发布特定 TFM 的自包含包，请调整 `TargetFrameworks` 或手动构建
- **Linux x86-32**：.NET 6 起已移除对 32 位 Linux 的官方支持，本 CI 不提供 `linux-x86` RID
- **预览版目标框架**：CI 默认安装当年 GA 版的 .NET SDK。若你的项目使用了尚未正式发布的 TFM（如 net11.0 在 2026 年），会导致编译/打包失败。请等待 SDK GA 或手动指定 `dotnet-version`
