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

# HybridCLR注意事项
1. 打增量包时AOT的dll是不能更改的，必须和打整包时的dll一致

# UI框架使用教程
1. 基于ET6组件式UI框架的使用 https://www.bilibili.com/video/BV1Ra411q7Ct
2. ScrollView无限滑动列表 https://www.bilibili.com/video/BV1YR4y1g7tN

# 演示视频
1. 触发器演示视频 https://www.bilibili.com/video/BV1iY4y1t7Kj
2. 服务端跨地图战斗 https://www.bilibili.com/video/BV1DB4y1B7qH
