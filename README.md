# ETPro

# 和ET6的区别
1. 使用YooAssets实现了资源热更新，包括多渠道安装包分包配置功能
2. 使用了基于YooAssets的资源管理系统替换了原有系统，包括对动态图集、Unity内置SpriteAtlas图集功能的支持
3. 使用组件模式的UI框架替换掉了原有部分，包括红点系统、多语言系统、引导系统等，易于集成第三方插件
4. 使用HybridCLR替换了ILRuntime，对C#语法支持更完备，BUG更少
5. 提供一个简单可扩展的对话框架
6. 替换AOI框架，支持OBB、球形触发器和射线检测，并且双端使用AOI
7. 提供一个简单可扩展的战斗框架，并且双端可单独使用
8. 提供一个简单的Ghost系统，服务端无缝世界跨逻辑地图战斗

# UI框架使用教程
1. 基于ET6组件式UI框架的使用 https://www.bilibili.com/video/BV1Ra411q7Ct
2. ScrollView无限滑动列表 https://www.bilibili.com/video/BV1YR4y1g7tN

# 演示视频
1. 触发器演示视频 https://www.bilibili.com/video/BV1iY4y1t7Kj
2. 服务端跨地图战斗 https://www.bilibili.com/video/BV1DB4y1B7qH

# 战斗框架设计思路
1. 分为两块内容，基于时间线的Skill，附着在Unit上的BUFF
2. 一个Skill包含多个组，释放时进入配置的默认组，在释放过程可以通过时间线上的触发器触发其他事件，如加BUFF、播动画、加音效、切换执行其他组等
3. 技能时间线执行中两个触发点之间如有间隔时间则可能被打断，打断会进入配置的默认打断组
4. buff通过优先级、分组等方式处理顶替等操作，一个buff可以有多个子状态，通过组合模式配置，如一个buff可以同时用来修改属性、添加控制状态等
5. buff还可以用来监听事件，如造成伤害前、后分别会轮询一次攻击者和被攻击者所有buff分发造成伤害事件，在buff对应事件可以获取以及修改伤害信息，其他还有诸如添加、移除buff事件，移动事件等可自己扩展
6. 由于是基于时间线的，技能编辑器用Timeline什么的很好做，配置也可采用其他方式如json，lua，scriptobject等，改下SkillStepComponent就好

# 注意事项
本仓库引用的HybridCLR是修改过支持reload dll的分支，具体修改内容参考 https://github.com/focus-creative-games/hybridclr/commit/db4e8c4fea9f8ccb9681b69ee8c50a919e16dece
