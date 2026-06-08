# 2D Adventure Game

一个 Unity 2D 横版冒险游戏，包含完整的关卡、敌人 AI、Boss 战、道具系统和场景特效。

## 运行环境

- Unity 2022.3 LTS 或更高版本
- Windows / macOS / Linux

## 如何运行

1. 克隆仓库
   ```bash
   git clone https://github.com/lh329/2DAdventureGame.git
   ```
2. 用 Unity Hub 打开项目文件夹
3. 在 Unity 编辑器中打开 `Assets/Scenes/Level1.unity` 或 `Level2.unity`
4. 点击 Play 按钮运行

## 核心功能

| 功能 | 说明 |
|------|------|
| 双关卡 | Level1 横版闯关 + Level2 Boss 战 |
| 敌人 AI | 巡逻敌人 + 飞行敌人，带追踪弹 |
| Boss 战 | 多阶段 Boss，50%血量狂暴，脚下魔毯波浪动画 |
| 道具掉落 | ❤️ 回血心 (30%) / ⭐ 无敌星 4秒 (20%) / 🛡 护盾 8秒挡1次 (20%) |
| 场景特效 | 魔毯波浪浮动动画、角色无敌闪烁、护盾光效 |

## 项目结构

```
Assets/
├── Scenes/          # 关卡场景
├── Scripts/         # 游戏逻辑脚本
│   ├── Player.cs
│   ├── Enemy.cs
│   ├── FlyingEnemy.cs
│   ├── Boss.cs
│   ├── Bullet.cs
│   ├── ItemDrop.cs
│   └── CarpetWave.cs
├── Sprites/         # 游戏素材
├── Prefabs/         # 预制体
└── Editor/          # 编辑器工具脚本
```

## 控制方式

| 按键 | 功能 |
|------|------|
| A / D 或 ← / → | 左右移动 |
| Space | 跳跃 |
| J | 攻击 |

## 更新记录

- ✅ Boss 战关卡搭建（魔毯波浪动画）
- ✅ 道具掉落系统（心 / 无敌星 / 护盾）
- ✅ 护盾视觉特效（角色蓝光 + 浮动盾牌图标）
- ✅ 飞行怪物位置优化 & 关卡清理
