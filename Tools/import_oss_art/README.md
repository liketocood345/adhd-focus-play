# 开源项目美术素材导入

从 [OpenSourceReferences.md](../../Docs/OpenSourceReferences.md) 所列 Unity 仓库 **克隆代码并一并复制美术资源** 到 `Assets/_Project/Art/ThirdParty/`。

## 用法

```powershell
# 在项目根目录执行（默认导入 manifest 中全部仓库）
.\Tools\import_oss_art\import.ps1

# 仅导入跑酷相关
.\Tools\import_oss_art\import.ps1 -RepoIds AwesomeRunner,HiraRunner

# 跳过 git clone（仅重新扫描已缓存仓库）
.\Tools\import_oss_art\import.ps1 -SkipClone
```

或在 Unity 菜单：**ADHD Training → Import Open Source Art**

## 导入后

1. 资源位于 `Assets/_Project/Art/ThirdParty/<RepoId>/`
2. 运行 **ADHD Training → Link Art Resource Bindings** 按文件名启发式生成 `Resources/Art/...` 符号链接/副本映射
3. 游戏运行时通过 `GameArtLibrary` 加载；无素材时回退到 Primitive

## 许可证

导入前请阅读各仓库 LICENSE。MIT 项目可优先用于竞赛/展示。
