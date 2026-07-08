# ETPro

基于ET6框架的Unity C#双端（客户端+服务端）游戏框架，使用HybridCLR实现热更新，YooAssets实现资源热更新，提供了完整的AOI、战斗、UI、对话等框架。

## 目录

- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [解决方案结构](#解决方案结构)
- [核心系统](#核心系统)
  - [UI框架](#ui框架)
  - [AOI框架](#aoi框架)
  - [战斗框架](#战斗框架)
  - [Ghost系统](#ghost系统)
  - [资源管理系统](#资源管理系统)
  - [对话框架](#对话框架)
  - [红点系统](#红点系统)
  - [多语言系统](#多语言系统)
  - [引导系统](#引导系统)
  - [AI框架](#ai框架)
  - [数值系统](#数值系统)
  - [有限状态机](#有限状态机)
  - [导航系统](#导航系统)
- [和ET6的区别](#和et6的区别)
- [快速开始](#快速开始)
- [配置系统](#配置系统)
- [协议系统](#协议系统)
- [工具链](#工具链)
- [部署](#部署)
- [UI框架使用教程](#ui框架使用教程)
- [演示视频](#演示视频)
- [战斗框架设计思路](#战斗框架设计思路)
- [注意事项](#注意事项)

## 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| Unity | 2020.3.48f1c1 | 使用C# 8语法 |
| .NET | 6.0 | 服务端运行时 |
| HybridCLR | 修改版（支持DLL reload） | C#热更新 |
| YooAssets | 1.2.4 | 资源热更新 |
| KCP | - | 网络传输层 |
| protobuf-net | - | 消息序列化 |
| MongoDB Bson | - | 数据序列化 |
| NLog | - | 服务端日志 |
| DOTween | 1.2.705 | 动画补间 |
| SuperScrollView | - | 无限滑动列表 |
| URP | 10.10.1 | 通用渲染管线 |
| TextMeshPro | 3.0.6 | UI文字渲染 |

## 项目结构

```
ETPro/
├── Unity/                      # Unity客户端工程
│   ├── Assets/
│   │   ├── Codes/              # 热更新代码（核心逻辑）
│   │   │   ├── Model/          # Model层：数据定义、组件声明
│   │   │   │   ├── Core/       #   框架核心（Entity、Object、Event、Timer、Scene）
│   │   │   │   ├── Module/     #   各模块定义（AOI、Battle、UI、AI等）
│   │   │   │   ├── Generate/   #   自动生成代码（Config、Message）
│   │   │   │   └── Demo/       #   示例代码
│   │   │   ├── Hotfix/         # Hotfix层：逻辑实现（可热更）
│   │   │   ├── ModelView/      # ModelView层：依赖Unity的数据定义
│   │   │   └── HotfixView/     # HotfixView层：依赖Unity的逻辑实现
│   │   ├── Mono/               # 非热更Unity代码
│   │   │   ├── Core/           #   核心入口（CodeLoader等）
│   │   │   ├── Helper/         #   辅助工具
│   │   │   ├── Module/         #   非热更模块（DynamicAtlas、Map、Network等）
│   │   │   └── MonoBehaviour/  #   MonoBehaviour
│   │   ├── Editor/             # 编辑器扩展工具
│   │   ├── ThirdParty/         # 第三方库（ETTask、LitJson、Mongo、protobuf-net等）
│   │   ├── AssetsPackage/      # YooAssets资源包（Audio、Code、Config、UI等）
│   │   ├── MapEditor/          # 地图编辑器
│   │   └── Plugins/            # 原生插件
│   ├── Packages/               # Unity包管理
│   │   ├── com.code-philosophy.hybridclr/  # HybridCLR
│   │   ├── com.tuyoogame.yooasset/        # YooAssets
│   │   ├── com.dg.tween@1.2.705/          # DOTween
│   │   └── com.unity.render-pipelines.universal@10.8.1/  # URP
│   └── ProjectSettings/        # Unity项目设置
├── Server/                     # 服务端工程
│   ├── App/                    # 启动入口（Program.cs）
│   ├── Model/                  # 服务端数据层
│   │   ├── Base/               #   基础工具（DllHelper、MongoHelper）
│   │   ├── Module/             #   服务端模块（Actor、ActorLocation、AOI/Ghost、Console、DB、Http等）
│   │   ├── Demo/               #   示例代码
│   │   ├── Generate/           #   自动生成代码
│   │   ├── Config/             #   配置加载
│   │   └── Watcher/           #   进程管理
│   └── Hotfix/                 # 服务端逻辑层
│       ├── Module/             #   服务端逻辑（Actor、ActorLocation、AOI/Ghost、Console、DB、Http等）
│       ├── Handler/            #   消息处理器
│       └── Demo/               #   示例逻辑
├── Robot/                      # 压测机器人工程
│   ├── App/                    # 启动入口
│   ├── Model/                  # 数据层
│   └── Hotfix/                 # 逻辑层
├── ThirdParty/                 # 共享第三方库
│   ├── UnityEngine/            # UnityEngine接口定义（供服务端引用）
│   ├── ShareLib/               # 共享代码库
│   └── ETTask/                 # ET异步框架
├── Tools/                      # 工具工程
│   ├── App/                    # 导表、导协议工具
│   ├── FileServer/             # CDN文件服务器
│   ├── Analyzer/               # 代码分析器（Roslyn Source Generator）
│   ├── Config/                 # 工具配置
│   └── RecastNavExportor/      # 导航网格导出工具
├── Proto/                      # 协议定义文件
│   ├── OuterMessage.proto      # 客户端-服务端通信协议
│   ├── InnerMessage.proto      # 服务端内部通信协议
│   └── MongoMessage.proto      # MongoDB存储协议
├── Excel/                      # 配置表源文件（xlsx）
├── Config/                      # 生成的二进制配置文件
│   ├── StartConfig/            # 服务端启动配置
│   │   ├── Localhost/          # 本地开发配置
│   │   └── Release/            # 发布配置
│   └── Recast/                 # 导航网格数据
├── Book/                        # ET框架设计文档
├── Bin/                         # 编译输出目录
├── Store/                       # 发布产物存储
├── Client-Server.sln            # 全局解决方案
├── copyGame.bat                 # 多开Unity实例（Junction链接）
├── startwinserver.bat           # 启动Windows服务端（Watcher模式）
└── publishlinux64.bat           # 发布Linux x64服务端
```

## 解决方案结构

`Client-Server.sln` 包含以下工程分组：

### Client（客户端）
| 工程 | 说明 |
|------|------|
| Unity.Model | 客户端Model层（数据定义，不依赖Unity引擎） |
| Unity.Hotfix | 客户端Hotfix层（逻辑实现，不依赖Unity引擎，可热更） |
| Unity.ModelView | 客户端ModelView层（数据定义，依赖Unity引擎） |
| Unity.HotfixView | 客户端HotfixView层（逻辑实现，依赖Unity引擎，可热更） |
| Unity.Mono | 非热更的Unity层（CodeLoader入口、原生模块） |
| Unity.ThirdParty | 第三方库（ETTask、LitJson、Mongo、protobuf-net、SuperScrollView） |
| Unity.Editor | 编辑器扩展 |
| Unity.Codes | 热更新代码集合（编译为单一DLL供HybridCLR加载） |
| YooAsset / YooAsset.Editor | YooAsset资源管理 |
| HybridCLR.Runtime / HybridCLR.Editor | HybridCLR热更新 |
| Unity.RenderPipelines.Universal.* | URP渲染管线 |
| Unity.TextMeshPro | 文字渲染 |

### Server（服务端）
| 工程 | 说明 |
|------|------|
| Server.App | 服务端启动入口 |
| Server.Model | 服务端数据层 |
| Server.Hotfix | 服务端逻辑层（可热更） |

### Robot（压测机器人）
| 工程 | 说明 |
|------|------|
| Robot.App | 机器人启动入口 |
| Robot.Model | 机器人数据层 |
| Robot.Hotfix | 机器人逻辑层 |

### ThirdParty（共享库）
| 工程 | 说明 |
|------|------|
| UnityEngine | UnityEngine接口定义（供服务端/机器人引用） |
| ShareLib | 共享代码库 |
| ETTask | ET异步框架 |

### Tools（工具）
| 工程 | 说明 |
|------|------|
| Tools.App | 导表工具（ExcelExporter）、导协议工具（Proto2CS） |
| FileServer | CDN文件服务器 |
| Analyzer | 代码分析器（Roslyn Source Generator，用于自动注册） |

## 核心系统

### UI框架

基于ET组件模式的UI框架，位于 `Unity/Codes/Model/Module/UIManager` 和 `Unity/Codes/Hotfix/Module/UIManager`。

- **UIManagerComponent**：UI总管理器，管理所有窗口的生命周期、层级、窗口栈
- **UIWindow**：窗口抽象，控制窗口创建、显示、隐藏、销毁
- **UILayer**：窗口层级管理（底层、普通、弹窗、顶层等）
- **UIBaseContainer / UIBaseView**：容器与视图基类，支持组件式嵌套
- **UI组件**：封装了Unity UGUI组件（UIButton、UIImage、UIText、UISlider、UIToggle、UIDropdown、UIInput、UILoopGridView、UILoopListView2等），使逻辑层可以像操作属性一样操作UI
- **UIWatcherComponent**：UI事件监听系统，通过Attribute自动注册
- **红点系统**：RedDotComponent，支持配置化红点树
- **多语言系统**：II18NSystem接口，UI组件自动绑定I18N Key
- **引导系统**：GuidanceComponent，支持聚焦高亮、圆形/矩形遮罩

### AOI框架

双端AOI（Area of Interest）系统，位于 `Unity/Codes/Model/Module/AOI`（客户端）和 `Server/Model/Module/AOI`（服务端）。

- **AOICell**：AOI格子，维护格子内单位列表和监听者列表
- **AOISceneComponent**：AOI场景管理，管理格子的创建与查询
- **AOIUnitComponent**：单位AOI组件，管理单位在格子中的进出
- **触发器系统**：
  - `AOITrigger`：触发器基类
  - `OBBComponent`：OBB（有向包围盒）碰撞检测
  - 球形触发器
  - 射线检测（Ray / RaycastHit / Physics）
- **事件驱动**：单位进出格子、位置变化、旋转变化均触发事件

### 战斗框架

完整的Skill + Buff战斗系统，位于 `Unity/Codes/Model/Module/Battle`，双端共用。

#### Skill系统
- **SkillAbility**：技能实例，一个技能可包含多个AbilityGroup
- **SkillAbilityGroup**：技能执行组，技能释放时进入默认组，可通过时间线触发切换到其他组
- **SkillStepComponent**：技能步骤组件，基于时间线驱动技能执行（等待、移动、播动画、播放音效、加Buff、生成碰撞器、加特效、消耗、切换组等）
- **SpellComponent**：施法组件，管理技能释放流程
- **MoveAndSpellComponent**：移动施法组件
- **SkillColliderComponent**：技能碰撞器，支持多种形状
- **SkillWatcherComponent**：技能事件监听系统，时间线触发器触发时分发事件（加Buff、播动画、加音效、切换组等）
- **ConditionWatcherComponent**：条件监听器，支持条件判断切换组

#### Buff系统
- **Buff**：Buff实例，支持优先级、分组、子状态组合
- **BuffComponent**：Buff管理器，处理Buff添加、移除、顶替
- **BuffSubType**：Buff子状态类型（属性修改、控制状态、流血、吟唱等）
- **BuffWatcherComponent**：Buff事件监听系统，支持监听以下事件：
  - 造成伤害前/后（可获取并修改伤害信息）
  - 添加/移除Buff事件
  - 移动事件
  - 吟唱事件
  - 施法事件
- **BuffBleedComponent**：流血Buff
- **BuffChantConfig**：吟唱Buff配置
- **BuffActionControlConfig**：行为控制Buff配置
- **BuffAttrConfig**：属性修改Buff配置

#### 战斗预览系统
位于 `Unity/Codes/ModelView/Module/Battle/SpellPreviewManagers`：
- **SpellPreviewComponent**：施法预览基类
- **选择器**：圆形选择（CircularSelect）、矩形选择（DirectRectSelect）、点选（PointSelect）、目标选择（TargetSelect）
- **SelectWatcherComponent**：选择器自动注册系统

### Ghost系统

服务端跨逻辑地图战斗Ghost系统，位于 `Server/Model/Module/AOI/Ghost`。

- **GhostComponent**：影子组件，当单位跨地图边界时在相邻地图创建Ghost
- **AreaComponent**：区域组件，管理逻辑地图区域
- **AreaConfig / AreaConfigComponent**：区域配置，定义地图边界和区域关系
- **消息转发**：GhostComponent.MsgMap 定义了需要转发到其他Area的协议映射（如M2C_PathfindingResult → M2M_PathfindingResult）

### 资源管理系统

基于YooAssets的资源管理系统，位于 `Unity/Codes/ModelView/Module/Resource` 和 `Unity/Assets/Mono/Module/YooAssets`。

- **ResourcesComponent**：资源管理核心，对YooAsset封装，提供统一的资源加载/释放接口
- **GameObjectPoolComponent**：GameObject对象池
- **ImageLoaderComponent**：图片加载器
- **ImageOnlineComponent**：在线图片加载（网络图片）
- **MaterialComponent**：材质管理
- **SoundComponent**：音效管理
- **IPackageFinder**：资源包查找接口，支持多渠道分包
- **动态图集**：DynamicAtlas模块（Unity/Assets/Mono/Module/DynamicAtlas），支持运行时动态合图
- **SpriteAtlas**：支持Unity内置SpriteAtlas图集功能

### 对话框架

GalGame对话引擎，位于 `Unity/Codes/ModelView/Demo/GalGame` 和 `Unity/Codes/HotfixView/Demo/GalGame`。

- **GalGameEngineComponent**：对话引擎核心，管理剧情播放
- **Chapter / ChapterCategory**：剧情章节管理
- **FSM状态机**：Preparing → Ready → Running / Suspended / FastForward
- **CommandWatcherComponent**：命令监听系统，通过Attribute自动注册命令处理器
- **OperaComponent**：演出组件，控制角色表情、位置等
- **UI组件**：MessageWindow、BgView、MaskView、StageView、StageRoleInfo、Review等

### 红点系统

- **RedDotComponent**：红点组件，挂载在UI上
- **RedDotConfig**：配置化红点树（Excel配置）
- 配置文件：`Excel/RedDotConfig@c.xlsx`

### 多语言系统

- **I18NComponent**：多语言管理组件
- **I18NConfig**：多语言配置
- **I18NKey**：多语言Key定义
- **LangType**：支持语言类型
- **II18NSystem**：UI组件接口，自动绑定I18N Key实现多语言
- 配置文件：`Excel/I18NConfig@i.xlsx`
- Mono层I18N模块：`Unity/Assets/Mono/Module/I18N`

### 引导系统

- **GuidanceComponent**：引导组件，管理引导步骤
- **GuidanceConfigCategory**：引导配置
- **GuidanceStepType**：引导步骤类型
- **UICircleMaskControl / UIRectMaskControl**：圆形/矩形遮罩控制
- **UIGuidanceView**：引导UI视图
- 配置文件：`Excel/GuidanceConfig@cs.xlsx`

### AI框架

- **AIComponent**：AI组件，挂载在Unit上
- **AIDispatcherComponent**：AI调度器，管理AI决策频率
- **AAIHandler**：AI处理器基类
- **AIConfig**：AI配置（状态切换条件）
- 内置AI：巡逻（AI_XunLuo）、攻击（AI_Attack）、追逐（AI_ZhuiZhu）

### 数值系统

- **NumericComponent**：数值组件，管理单位属性
- **NumericType**：数值类型定义（基础值、加成值、最终值）
- **NumericWatcherComponent**：数值变化监听器
- 配置文件：`Excel/AttributeConfig@cs.xlsx`、`Excel/FormulaConfig@cs.xlsx`

### 有限状态机

- **FSMComponent**：状态机组件
- **FSMSystem**：状态机系统接口
- **FSMWatcherComponent**：状态机事件监听
- 用于对话引擎、战斗状态等

### 导航系统

- **NavmeshComponent**：导航网格组件
- **PathfindingComponent**：寻路组件
- 导航数据：`Config/Recast/`
- 导出工具：`Tools/RecastNavExportor/`

## 和ET6的区别

1. 使用YooAssets实现了资源热更新，包括多渠道安装包分包配置功能
2. 使用了基于YooAssets的资源管理系统替换了原有系统，包括对动态图集、Unity内置SpriteAtlas图集功能的支持
3. 使用组件模式的UI框架替换掉了原有部分，包括红点系统、多语言系统、引导系统等，易于集成第三方插件
4. 使用HybridCLR替换了ILRuntime，对C#语法支持更完备，BUG更少
5. 提供一个简单可扩展的对话框架
6. 替换AOI框架，支持OBB、球形触发器和射线检测，并且双端使用AOI
7. 提供一个简单可扩展的战斗框架，并且双端可单独使用
8. 提供一个简单的Ghost系统，服务端无缝世界跨逻辑地图战斗

## 快速开始

### 环境要求

- **Unity**：2020.3.48f1c1（需支持C# 8语法）
- **IDE**：Visual Studio 2022+ 或 Rider 2021.3+（需安装Unity插件）
- **.NET SDK**：6.0+
- VS用户需安装"使用Unity的游戏开发"扩展，并在"工具-选项-适用于Unity的工具-常规"里将"禁止完整生成项目"设为False

### 步骤

1. **克隆仓库**
   ```bash
   git clone <repo-url>
   ```

2. **打开Unity项目**
   - 启动Unity，`File → Open Project → Open`，选中 `Unity/` 文件夹

3. **编译客户端代码**
   - Unity菜单 `Tools → BuildCode`

4. **编译全量工程**
   - 用Rider或VS打开 `Client-Server.sln`
   - 右键解决方案 → 全部编译（必须保证Unity处于开启状态，以生成Unity库引用）

5. **导出配置表**
   ```bash
   cd Bin
   dotnet Tools.dll --AppType=ExcelExporter
   ```

6. **导出协议**
   ```bash
   cd Bin
   dotnet Tools.dll --AppType=Proto2CS
   ```

7. **启动服务端**
   ```bash
   cd Bin
   dotnet Server.dll --Process=1 --Console=1
   ```
   或直接运行：
   ```bash
   startwinserver.bat
   ```

8. **运行客户端**
   - Unity中双击 `Scenes/Init` 场景
   - 点击Play运行

### 测试状态同步Demo

1. 修改配置：进入 `Excel/` 目录修改对应表格，重新导表并重启服务端
2. 打包PC客户端：Unity → Tools菜单 → 打包工具 → 选择PC → 勾选打包exe → 开始打包（输出到Release目录）
3. 运行Unity编辑器，登录 → 进入大厅 → 进入场景
4. 运行PC包，登录 → 进入大厅
5. 点击鼠标右键移动人物

### 多开Unity实例

运行 `copyGame.bat`，输入序号，会在同级目录创建带Junction链接的Unity副本，支持多开测试。

## 配置系统

### Excel配置表

位于 `Excel/` 目录，文件命名规则：

| 后缀 | 说明 | 示例 |
|------|------|------|
| `@cs` | 客户端+服务端共享 | `SkillConfig@cs.xlsx` |
| `@c` | 仅客户端 | `RedDotConfig@c.xlsx` |
| `@s` | 仅服务端 | `ServerConfig@c.xlsx` |
| `@m` | 地图配置 | `Map1AreaConfig@m.xlsx` |
| `@i` | 多语言 | `I18NConfig@i.xlsx` |

### 现有配置表

| 配置表 | 说明 |
|--------|------|
| AIConfig | AI行为配置 |
| AttributeConfig | 属性配置 |
| BuffActionControlConfig | Buff行为控制配置 |
| BuffAttrConfig | Buff属性修改配置 |
| BuffBleedConfig | Buff流血配置 |
| BuffChantConfig | Buff吟唱配置 |
| BuffConfig | Buff主配置 |
| EffectConfig | 特效配置 |
| FormulaConfig | 公式配置 |
| GuidanceConfig | 引导配置 |
| I18NConfig | 多语言配置 |
| Map1AreaConfig / Map2AreaConfig | 地图区域配置 |
| MapSceneConfig | 地图场景配置 |
| RedDotConfig | 红点配置 |
| SceneConfig | 场景配置 |
| ServerConfig | 服务器配置 |
| SkillConfig | 技能配置 |
| SkillJudgeConfig | 技能判定配置 |
| SkillStepConfig | 技能步骤配置 |
| UIRouterConfig | UI路由配置 |
| UnitConfig | 单位配置 |

### 服务端启动配置

位于 `Config/StartConfig/`：

| 目录 | 说明 |
|------|------|
| Localhost | 本地开发配置 |
| Release | 发布配置 |

包含：StartMachineConfig（机器配置）、StartProcessConfig（进程配置）、StartSceneConfig（场景配置）、StartZoneConfig（区服配置）

### 导出工具

| 命令 | 说明 |
|------|------|
| `dotnet Tools.dll --AppType=ExcelExporter` | 导出全部配置表 |
| `win_startExcelExport.bat` | 导出全部配置表 |
| `win_startAttrExport.bat` | 导出属性配置 |
| `win_startAreaExport.bat` | 导出区域配置 |
| `win_startI18NExport.bat` | 导出多语言配置 |
| `win_startExportAll.bat` | 导出全部 |

## 协议系统

协议定义文件位于 `Proto/` 目录：

| 文件 | 说明 |
|------|------|
| OuterMessage.proto | 客户端 ↔ 服务端通信协议 |
| InnerMessage.proto | 服务端内部进程间通信协议 |
| MongoMessage.proto | MongoDB存储消息定义 |

导出命令：
```bash
cd Bin
dotnet Tools.dll --AppType=Proto2CS
```

或运行 `Proto/win_startProtoExport.bat`

主要协议：
- **C2G_** / **G2C_**：客户端 ↔ Gate服务器
- **C2M_** / **M2C_**：客户端 ↔ Map服务器（通过Gate转发）
- **C2R_** / **R2G_**：客户端 ↔ Realm服务器
- **M2M_**：Map服务器间通信（Ghost跨地图）
- **G2M_** / **M2G_**：Gate ↔ Map服务器

## 工具链

### Tools.App

| 命令 | 说明 |
|------|------|
| `--AppType=ExcelExporter` | 导出配置表（xlsx → bytes） |
| `--AppType=Proto2CS` | 导出协议（proto → C#） |

### FileServer

CDN文件服务器，用于资源热更新分发。

### Analyzer

基于Roslyn Source Generator的代码分析器，自动生成系统注册代码，免去手动注册Event/Watcher/System的样板代码。

### RecastNavExportor

导航网格导出工具，在Unity编辑器中使用（`Unity/Assets/Editor/Common/RecastNavDataExporter`）。

## 部署

### Windows服务端

```bash
# 方式一：Watcher模式（管理所有子进程）
startwinserver.bat

# 方式二：直接启动指定进程
cd Bin
dotnet Server.dll --Process=1 --Console=1
```

### Linux服务端

```bash
# 发布
dotnet publish -r linux-x64 --no-self-contained --no-dependencies -c Release

# 运行
cd Bin
dotnet Server.dll --Process=1 --Console=1
```

或直接运行 `publishlinux64.bat`

### 服务端热重载

服务端运行时支持DLL热重载，通过控制台命令：
```
reloaddll  # 重载热更新DLL
reloadconfig  # 重载配置
```

### 客户端热更新

- **代码热更**：HybridCLR加载热更DLL（CodeLoader管理多种加载模式）
- **资源热更**：YooAssets资源热更新

CodeLoader支持的加载模式：
| 模式 | 说明 |
|------|------|
| BuildIn | 使用内置AOT代码 |
| LoadDll | 从资源包加载热更DLL |
| Wolong | HybridCLR模式（加载DLL + AOT元数据补充） |
| LoadFromUrl | 从URL下载DLL（调试用） |
| Reload | 热重载模式（编辑器内热重载） |

## UI框架使用教程

1. 基于ET6组件式UI框架的使用 https://www.bilibili.com/video/BV1Ra411q7Ct
2. ScrollView无限滑动列表 https://www.bilibili.com/video/BV1YR4y1g7tN

## 演示视频

1. 触发器演示视频 https://www.bilibili.com/video/BV1iY4y1t7Kj
2. 服务端跨地图战斗 https://www.bilibili.com/video/BV1DB4y1B7qH

## 战斗框架设计思路

1. 分为两块内容，基于时间线的Skill，附着在Unit上的BUFF
2. 一个Skill包含多个组，释放时进入配置的默认组，在释放过程可以通过时间线上的触发器触发其他事件，如加BUFF、播动画、加音效、切换执行其他组等
3. 技能时间线执行中两个触发点之间如有间隔时间则可能被打断，打断会进入配置的默认打断组
4. buff通过优先级、分组等方式处理顶替等操作，一个buff可以有多个子状态，通过组合模式配置，如一个buff可以同时用来修改属性、添加控制状态等
5. buff还可以用来监听事件，如造成伤害前、后分别会轮询一次攻击者和被攻击者所有buff分发造成伤害事件，在buff对应事件可以获取以及修改伤害信息，其他还有诸如添加、移除buff事件，移动事件等可自己扩展
6. 由于是基于时间线的，技能编辑器用Timeline什么的很好做，配置也可采用其他方式如json，lua，scriptobject等，改下SkillStepComponent就好

### 技能步骤类型（SkillStepType）

| 类型 | 说明 | 参数 |
|------|------|------|
| Wait | 仅等待 | - |
| Move | 改变坐标 | 相对坐标"x,y,z" |
| Anim | 播放动画 | 动画名 |
| Sound | 播放声音 | 音频路径 |
| Cost | 结算消耗 | 消耗属性Key, 固定值, 计算公式 |
| GenerateObject | 生成召唤物 | 召唤物Id |
| GenerateCollider | 生成碰撞器 | 触发器Id, 伤害公式Id, 伤害百分比, 最大单位数, Buff列表, 延时(ms) |
| AddEffect | 添加特效 | 特效路径 |
| AddBuff | 给自己加Buff | BuffId, 时间(ms) |
| RemoveBuff | 移除自己Buff | BuffId |
| ChangeGroup | 改变执行组 | Group或条件判断切换 |

## 注意事项

1. 本仓库引用的HybridCLR是修改过支持reload dll的分支，具体修改内容参考 https://github.com/focus-creative-games/hybridclr/commit/db4e8c4fea9f8ccb9681b69ee8c50a919e16dece
2. 中文目录、VS版本低、Rider未安装组件、未安装.NET 6、Unity版本低、Win7未特别设置等均可能导致编译错误
3. 如果打包报错缺少"StreamingAsset"，在 `Unity/Assets` 下新建 `StreamingAsset` 文件夹即可
4. 编译过程中如果出现依赖问题，检查 Unity → External Tools → Generate .csproj files for 中是否勾选了 Registry packages 和 Build-in packages（可能导致冲突）
5. 必须保证Unity处于开启状态再编译 `Client-Server.sln`，Unity库需要编译到 `Unity/Temp/Debug` 目录下供其他工程引用

## 框架文档

`Book/` 目录下包含ET框架的设计文档（中英双语）：

| 文档 | 说明 |
|------|------|
| 1.1运行指南 | 环境搭建与运行步骤 |
| 1.2为什么使用.NET Core | .NET Core选型说明 |
| 2.1CSharp协程 | C#协程实现原理 |
| 2.2更好的协程 | ETTask异步框架 |
| 2.3单线程异步 | 单线程异步模型 |
| 3.2MongoBson库 | MongoDB Bson库使用 |
| 3.3一切皆实体 | Entity设计理念 |
| 3.4事件机制EventSystem | 事件系统原理 |
| 4.1组件式设计 | 组件模式架构 |
| 5.4Actor模型 | Actor消息模型 |
| 5.5Actor Location | Actor定位机制 |
| 5.6数值组件设计 | 数值系统设计 |
| 6.1AI框架 | AI框架设计 |
| 6.2AI框架-行为机 | 行为机AI设计 |
