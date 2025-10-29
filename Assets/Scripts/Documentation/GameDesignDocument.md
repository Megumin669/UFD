# EFD - Enhanced First-person Defense
## Game Design Document

**Version:** 1.0  
**Date:** October 29, 2025  
**Genre:** Tower Defense / First-Person Survival  
**Platform:** PC (Unity 2025+)  
**Target Audience:** Solo and Co-op players who enjoy strategic defense games  

---

## 🎯 Game Overview

**EFD (Enhanced First-person Defense)** is a unique blend of tower defense strategy and first-person action combat. Players defend their Sanctum through alternating day/night cycles, building defenses during the day and fighting alongside them at night.

### Core Concept
- **Day Phase**: Switch between Top-Down build mode and First-Person exploration
- **Night Phase**: First-person combat defending against undead hordes
- **Meta Progression**: Soul-based upgrades that persist between runs
- **Resource Management**: Stamina, health, materials, and souls

---

## 🎮 Core Gameplay Loop

### 🕯 Day Phase (Rebuild & Prepare)
**Duration**: Time-limited preparation phase that ends at sunset

**Gameplay Modes**:
- **Top-Down Build Mode**: Strategic placement and management of defenses
- **First-Person Exploration**: Resource gathering and reconnaissance

**Activities**:
- Repair walls, barricades, and damaged structures
- Build and upgrade magical defenses (soul totems, rune traps, ballista towers)
- Collect resources from defeated enemies
- Enhance personal equipment for upcoming night
- Explore the surrounding graveyard for materials

### 🌑 Night Phase (Survive & Defend)
**Duration**: Wave-based survival until dawn

**Combat System**: First-person perspective with strategic weapon selection
- Fight alongside automated defenses
- Manage stamina for attacks, jumping, and sprinting
- Use terrain and defensive structures tactically
- Face increasingly difficult enemy waves

**Victory Condition**: Protect the Sanctum until dawn
**Failure Condition**: Sanctum destruction ends the run

### 💀 Between Runs (Meta Progression)
**Currency**: Souls (persistent across runs)

**Upgrade Categories**:
- Player survivability (+Max HP, +Stamina, +Regeneration)
- Combat effectiveness (+Weapon damage, +Attack speed)
- Defense improvements (+Turret damage/range, +Structure HP)
- Economic bonuses (+Soul gain, +Resource retention)
- Special abilities (+Spell cooldown reduction, +Magic damage)

---

## ⚔️ Combat System

### Weapon Types & Mechanics

#### 🗡️ Melee Weapons
**Examples**: Iron Sword, Blessed Axe, Spirit Scythe

**Mechanics**:
- **Stamina Cost**: 10 units per attack (configurable)
- **Damage System**: Physics-based with damage tags for targeting
- **Combat Feel**: Fast, responsive close-quarters combat
- **Special Features**: Combo attacks, swing arcs, impact feedback

**Current Implementation**:
- ScriptableObject-based weapon data system
- Configurable damage, speed, and stamina costs
- Safe damage tag validation system
- Integration with player stamina management

#### 🏹 Ranged Weapons  
**Examples**: Hunting Bow, Crossbow, Magic Wand

**Mechanics**:
- **Stamina Cost**: 5 units per shot (configurable)
- **Targeting**: Precision-based aiming system
- **Projectile System**: Physics-based arrow/bolt trajectory
- **Special Features**: Headshot multipliers, arrow recovery

**Current Implementation**:
- Projectile spawning and trajectory calculation
- Configurable arrow prefabs and damage systems
- Stamina integration for shot limiting

#### 🔮 Magic Weapons
**Examples**: Fire Staff, Frost Staff, Necro Orb

**Mechanics**:
- **Stamina Cost**: 15 units per cast (configurable, no separate mana system)
- **Spell System**: Area-of-effect and targeted magic
- **Magic Types**: Fire (damage), Frost (slow), Necro (utility)
- **Special Features**: Explosive projectiles, status effects
- **Cooldown System**: Future skills will use cooldown-based balancing instead of mana

**Current Implementation**:
- Staff projectile system with explosion mechanics
- Configurable damage tags for player-safe explosions
- Magic projectile physics and impact effects

### Health & Stamina Management

#### Health System
- **Base Health**: 100 HP (configurable)
- **Regeneration**: Automatic healing over time with delay
- **Damage Sources**: Enemy attacks, environmental hazards
- **UI Integration**: Real-time health display and events

#### Stamina System  
- **Base Stamina**: 100 points (configurable)
- **Consumption**:
  - Sprinting: 5 points/second
  - Jumping: 15 points per jump
  - Melee attacks: 10 points per swing
  - Ranged attacks: 5 points per shot
  - Magic attacks: 15 points per cast
- **Regeneration**: 10 points/second after 2-second delay
- **Exhaustion Prevention**: Actions blocked when insufficient stamina

---

## 🏗️ Building & Defense System

### Defense Structures

#### Passive Defenses
- **Stone Walls**: Basic barrier protection
- **Barricades**: Quick-build temporary obstacles  
- **Rune Traps**: Magical ground-based triggers
- **Soul Totems**: Area denial and enemy slowing

#### Active Defenses
- **Ballista Towers**: High-damage siege weapons
- **Arrow Turrets**: Rapid-fire anti-personnel
- **Magic Crystals**: Automated spell casting
- **Sanctum Core**: Primary objective to defend

### Resource System
**Primary Resources**:
- **Bone**: Basic construction material
- **Essence**: Magical component for enchantments
- **Crystal**: Advanced technology and powerful spells
- **Souls**: Meta-progression currency (persistent)

**Resource Sources**:
- Enemy drops upon death
- Environmental gathering during day phase
- Bonus rewards for defensive achievements
- Soul collection from defeated undead

---

## 👹 Enemy System

### **NEW: ScriptableObject-Based Architecture** ✅ 

**EnemyData System**: All enemy variations are now configured through ScriptableObject assets, making it easy to create and balance new enemy types without code changes.

**Key Features**:
- **Data-Driven Design**: Create enemy variants by configuring ScriptableObject assets
- **Designer-Friendly**: Non-programmers can create new enemies through Inspector
- **Runtime Efficiency**: Pre-configured data, no parsing needed
- **Modding Support**: External enemy packs possible through asset files

### Enemy Configuration Options

**Basic Properties**:
- Health, armor, movement speed, attack damage
- Detection range, attack range, stopping distance
- Turn speed, sprint multiplier, attack cooldown

**AI Behavior Types**:
- **Aggressive**: Attacks anything in range
- **Defensive**: Only attacks when attacked first
- **Coward**: Flees when health drops below 30%
- **Berserker**: Gets faster/stronger when damaged
- **Tactical**: Uses cover and positioning
- **Swarm**: Coordinated group attacks
- **Assassin**: Targets player specifically
- **Siege**: Focuses on destroying defenses

**Special Abilities** (Configurable with cooldowns):
- **Flying**: Hover above ground, ignore walls
- **Charge**: Rush attack with extra damage and speed
- **Disable Turret**: Temporarily shut down defensive structures
- **Heal**: Restore health to self or nearby allies
- **Teleport**: Instant movement to strategic positions
- **Stealth**: Become invisible for ambush attacks
- **EMP**: Disable electronic defenses
- **Explode**: Death explosion damaging nearby targets

**Resistances & Weaknesses**:
- Configurable damage type resistances (50% reduction)
- Damage type weaknesses (200% damage)
- Support for Physical, Fire, Ice, Lightning, Poison, Holy, Dark, Explosive

### Enemy Types & Behaviors

#### Basic Undead
- **Skeleton Warrior**: Basic melee attacker, weak to blunt weapons
- **Zombie**: Slow but tanky, spreads infection
- **Ghoul**: Fast erratic movement, vulnerable to fire

#### Advanced Threats  
- **Wraith**: Ranged magic attacks, hovers over walls, weak to arrows
- **Grave Fiend**: Heavy brute that breaks barricades, vulnerable to magic
- **Shadow Assassin**: Stealth attacks on player, bypasses some defenses

#### Boss Encounters
- **Necromancer**: Summons minions, curses defenses, requires focus fire
- **Bone Dragon**: Flying boss that attacks from above
- **Death Knight**: Elite warrior with heavy armor and magic resistance

### Advanced AI System

**State Machine**:
- **Idle**: Waiting, scanning for threats
- **Patrol**: Following predetermined paths
- **Chase**: Pursuing detected targets
- **Attack**: Engaging in combat
- **Flee**: Retreating from overwhelming threats
- **Ability**: Executing special powers

**Target Priority System** (Configurable per enemy):
- Player → Defenses → Sanctum → Wounded → Closest → Strongest
- Dynamic target switching based on threat assessment
- Group coordination for pack tactics

**Wave-Based Spawning**:
- **Dynamic Difficulty**: Health and damage scaling per wave
- **Threat Level System**: Gradually introduce stronger enemy types
- **Boss Waves**: Special encounters every 5th wave
- **Resource Management**: Configurable soul and material drops

---

## 🌅 Day/Night Cycle System

### Phase Transitions
- **Dawn**: Results screen, soul collection, upgrade opportunities
- **Day**: Build/repair phase with time pressure
- **Dusk**: Final preparation warning, automatic phase transition  
- **Night**: Wave-based survival combat
- **Midnight**: Peak difficulty with special enemy spawns

### Environmental Changes
- **Lighting**: Golden warm daytime → cold blue/violet night with volumetric fog
- **Weather**: Clear skies → ominous storms and mist
- **Audio**: Peaceful daytime ambiance → tense combat soundtrack
- **Visibility**: Full daylight → limited night vision with strategic lighting

---

## 🎨 Visual & Audio Design

### Art Style
- **Environment**: Dark fantasy graveyard setting with ruined chapel aesthetic
- **Architecture**: Gothic stone structures with magical enhancements
- **Atmosphere**: Eerie mist, supernatural lighting effects
- **Character Design**: Medieval fantasy with magical elements

### Audio Design
- **Daytime Music**: Lute and choir harmonies, peaceful and strategic
- **Nighttime Music**: Drums and low strings, building tension
- **Combat SFX**: Weapon impacts, spell effects, creature sounds
- **Ambient Audio**: Distant whispers, bone cracking, wind through ruins
- **Dynamic Audio**: Intensity scales with threat level and player actions

### User Interface
- **Day Phase UI**: Building menus, resource counters, time remaining
- **Night Phase UI**: Health/stamina bars, weapon selection, wave progress
- **Meta UI**: Upgrade trees, soul spending, progression tracking
- **Accessibility**: Colorblind-friendly, scalable text, audio cues

---

## 🚀 Technical Architecture

### Core Systems (Implemented)

#### Player Controller System
- **Movement**: First-person controller with sprinting and jumping
- **Camera**: Smooth mouse look with customizable sensitivity
- **Input**: New Unity Input System integration
- **Physics**: Rigidbody-based movement with collision detection

#### Weapon Framework
- **Data Architecture**: ScriptableObject-based weapon definitions
- **Damage System**: Tag-based targeting with safe validation
- **Animation Integration**: Weapon-specific attack patterns
- **Upgrade System**: Modular enhancement framework

#### Health & Stamina
- **Modular Components**: Reusable health and stamina systems
- **Event-Driven**: UnityEvent integration for UI updates
- **Configuration**: Inspector-friendly settings and testing tools
- **Debug Support**: Console logging and runtime monitoring

#### Documentation System
- **Comprehensive Docs**: Feature documentation with examples
- **Templates**: Standardized documentation format
- **Maintenance**: Update procedures for new features
- **Testing Guidelines**: QA procedures and validation tools

### Planned Systems

#### Building System
- **View Switching**: Seamless top-down/first-person transitions
- **Grid-Based Placement**: Snap-to-grid building with validation
- **Resource Management**: Material requirements and availability
- **Upgrade Paths**: Multi-tier enhancement system

#### AI & Enemy System  
- **Navigation**: NavMesh-based pathfinding with dynamic obstacles
- **State Machines**: Behavior trees for complex enemy AI
- **Wave Management**: Configurable enemy spawning and scaling
- **Performance**: LOD system and object pooling for large hordes

#### Progression System
- **Soul Economy**: Persistent currency with balanced earning/spending
- **Skill Trees**: Branching upgrade paths with meaningful choices
- **Save System**: Persistent progression data with backup validation
- **Achievement System**: Goal-driven progression incentives

---

## 📅 Development Roadmap

### Phase 1: Combat Core Foundation ✅ (Completed)
**Status**: Implemented and tested

**Completed Features**:
- ✅ First-person player controller with movement and input
- ✅ Health system with regeneration and damage handling
- ✅ Stamina system with configurable consumption and regeneration
- ✅ Complete weapon framework (melee, ranged, magic)
- ✅ Damage tag system for configurable targeting
- ✅ Debug and testing tools for all systems
- ✅ Comprehensive documentation system

**Technical Achievements**:
- Modular, reusable component architecture
- Event-driven system communication
- ScriptableObject-based data management
- Precision timing for smooth gameplay mechanics

### Phase 2: Base Building System (Next Priority)
**Estimated Duration**: 4-6 weeks

**Core Features**:
- [ ] Top-down camera mode with smooth transitions
- [ ] Grid-based building system with placement validation
- [ ] Basic defense structures (walls, turrets, traps)
- [ ] Resource collection and management UI
- [ ] Day/night cycle implementation with automatic transitions
- [ ] Simple enemy AI with pathfinding to Sanctum

**Technical Requirements**:
- Camera switching system with position/rotation interpolation
- Building grid overlay with visual feedback
- Resource inventory system with UI integration
- Basic enemy spawning and wave management

### Phase 3: Enemy AI & Combat Integration
**Estimated Duration**: 3-4 weeks

**Core Features**:
- [ ] Multiple enemy types with distinct behaviors
- [ ] NavMesh-based pathfinding with dynamic obstacle avoidance
- [ ] Wave-based enemy spawning with difficulty scaling  
- [ ] Enemy-player combat interactions with existing weapon systems
- [ ] Basic Sanctum health and lose condition
- [ ] Soul drop system with collection mechanics

**Technical Requirements**:
- Enemy AI state machines and behavior trees
- Performance optimization for multiple enemies
- Integration with existing damage and health systems
- Visual feedback for combat interactions

### Phase 4: Meta Progression & Balance
**Estimated Duration**: 2-3 weeks

**Core Features**:
- [ ] Soul-based upgrade shop with persistent progression
- [ ] Save/load system for meta-progression data
- [ ] Balance tuning for combat, enemies, and economy
- [ ] Player feedback integration and UI polish
- [ ] Basic tutorial and onboarding experience

**Technical Requirements**:
- Persistent data management with encryption
- Balance configuration system with easy tweaking
- Analytics integration for gameplay metrics
- Accessibility features and quality-of-life improvements

### Phase 5: Polish & Content Expansion
**Estimated Duration**: 3-4 weeks

**Core Features**:
- [ ] Advanced lighting and post-processing effects
- [ ] Complete audio implementation (music, SFX, ambient)
- [ ] Visual effects for spells, impacts, and atmosphere
- [ ] Additional weapon variants and enemy types
- [ ] Advanced building options and defensive strategies
- [ ] Steam integration and achievement system

**Technical Requirements**:
- Performance optimization and profiling
- Audio management system with dynamic mixing
- Particle effects and shader optimization
- Platform-specific builds and testing

---

## 🎯 Success Metrics & Goals

### Player Engagement
- **Session Length**: Target 30-45 minutes per run
- **Replayability**: Multiple viable strategies and upgrade paths
- **Progression Feel**: Meaningful advancement every 2-3 runs
- **Difficulty Curve**: Challenging but fair with clear improvement paths

### Technical Performance  
- **Frame Rate**: Consistent 60 FPS on target hardware
- **Loading Times**: Under 10 seconds for scene transitions
- **Memory Usage**: Efficient resource management for extended play
- **Platform Compatibility**: Stable performance across Windows configurations

### Content Depth
- **Weapon Variety**: 3+ weapons per category with distinct playstyles
- **Enemy Types**: 8+ enemy variants with unique behaviors
- **Defense Options**: 6+ building types with upgrade paths
- **Progression Unlocks**: 20+ meaningful upgrades in soul shop

---

## 🔮 Future Expansion Opportunities

### Advanced Features
- **Random Modifiers**: "Blood Moon", "Soul Storm" event nights
- **Procedural Content**: Randomized maps and wave compositions
- **Boss Encounters**: Special boss nights every 5th wave
- **Customization**: Sanctum appearance and functional modifications

### Multiplayer Expansion
- **Co-op Mode**: Two gravekeepers defending shared Sanctum
- **Asymmetric Roles**: Builder/Fighter role specialization
- **Shared Progression**: Team-based soul collection and upgrades
- **Communication Tools**: Built-in voice/text chat integration

### Platform Expansion
- **Console Ports**: Controller optimization and UI adaptation
- **Mobile Version**: Simplified controls and touch interface
- **VR Support**: Immersive first-person combat experience
- **Cross-Platform**: Unified progression across devices

---

## 📊 Risk Assessment & Mitigation

### Technical Risks
- **Performance**: Large enemy counts may impact frame rate
  - *Mitigation*: Object pooling, LOD system, performance profiling
- **Save Corruption**: Meta-progression data loss
  - *Mitigation*: Multiple save slots, cloud backup, validation checks
- **Balancing**: Difficulty spikes or trivial gameplay
  - *Mitigation*: Extensive playtesting, configurable difficulty, analytics

### Design Risks  
- **Complexity**: Too many systems overwhelming players
  - *Mitigation*: Gradual tutorial, clear UI design, optional complexity
- **Repetition**: Gameplay becoming stale after multiple runs
  - *Mitigation*: Random elements, multiple strategies, regular content updates
- **Accessibility**: Excluding players with disabilities
  - *Mitigation*: Colorblind support, audio cues, customizable controls

---

## 🏁 Conclusion

EFD represents an innovative fusion of strategic tower defense and visceral first-person combat. With its solid technical foundation already in place, the game is positioned to deliver a unique and engaging experience that combines thoughtful preparation with intense action.

The existing weapon systems, health/stamina management, and modular architecture provide a strong base for the planned building and enemy AI systems. The clear development roadmap and risk mitigation strategies ensure a path to successful completion.

**Next Steps**:
1. Begin Phase 2 development with camera switching and building systems
2. Implement basic enemy AI and pathfinding
3. Create day/night cycle with automatic transitions
4. Establish resource collection and management mechanics

The game's unique selling proposition—defending your base from within while managing strategic elements—creates compelling moment-to-moment gameplay that scales from tactical decisions to split-second combat reactions.

---

*This document represents the current vision for EFD and will be updated as development progresses and new insights emerge from playtesting and community feedback.*