# Graded Card Customization System - Complete Guide

## Overview

This guide documents the complete graded card customization system for Card Shop Simulator, enabling **per-grade custom label backgrounds** and **dynamic text styling**. The system supports grades 1-10 with fallback options and full transparency support.

**Final Capability**: Replace the black label backgrounds on graded cards with custom 615x160 pixel images and apply grade-specific text styling including colors, sizes, and positioning offsets.

## Key System Discoveries

### The Black Rectangle Mystery

**Problem**: When using transparent `GradedCardCase.png`, a black rectangle appeared instead of transparency.

**Root Cause**: The black rectangle was NOT from 3D meshes as initially assumed, but from **UI Image components**:
- `LabelImageBack` - The actual visible label background
- `LabelImage` - Hidden/unused label (misleading naming)

**Game Architecture Contradiction**: Despite the names, `LabelImageBack` is the main visible component, not `LabelImage`. Never trust variable naming in this game.

### Complete System Architecture

**3D Mesh Components** (Card3dUIGroup.m_GradedCardGrp):
- `CardBackMeshBlocker` - Renders card back texture that interferes with graded case display (must be disabled)
  - **Material**: `MAT_CardBackMesh (Instance)`
  - **Texture**: `T_CardBackMesh` (ID:1306 in sharedassets1.assets)
  - **Problem**: Causes card back texture to render on front of graded cards
- `Slab_BaseMesh` - Physical case base layer
  - **Material**: `trading_card 1 (Instance)`
  - **Texture**: `trading_card_transparent`
- `Slab_TopLayerMesh` - Physical case top layer
  - **Material**: `trading_card (Instance)`
  - **Texture**: `null`

**UI Components** (Canvas-based, UI Layer 5):
- `LabelImageBack` - **Main visible label background** (615x160 pixels stretched from 32x32 WhiteTile)
- `LabelImage` - Unused/hidden label component (misleading naming!)
- `LabelImageCompany` - Company branding area (red colored)

**Text Components** (TextMeshProUGUI):
- `m_GradeNumberText` - The grade number (1, 2, 3, etc.)
- `m_GradeDescriptionText` - Grade description ("Poor", "Fair", "Good", etc.)
- `m_GradeNameText` - Card name
- `m_GradeExpansionRarityText` - Expansion and rarity information

**Key Architecture Insight**: The system combines 3D mesh rendering (physical case structure) with Canvas UI overlays (text labels). The `Card3dUIGroup.EvaluateCardGrade()` method is the central control point for both systems.

## File Structure

### Required Files
```
/YourPluginDirectory/
├── 1.png, 2.png, ..., 10.png          # Grade-specific label backgrounds (615x160)
├── 1.txt, 2.txt, ..., 10.txt          # Grade-specific text configurations
├── GradedCardCase.png                  # Fallback label background (615x160)
├── GradedCardCase.txt                  # Fallback text configuration
└── GradedCardCaseExpander.dll          # The mod
```

### Image Specifications

**Label Background Images**:
- **Dimensions**: 615x160 pixels (ratio ~3.8:1)
- **Format**: PNG with transparency support
- **Usage**: Stretched to fit UI container using Unity's Simple image type

**Original Discovery**: The base `WhiteTile` sprite is only 32x32 pixels, but Unity stretches it to 615x160 in the UI system.

## Configuration Format

### Text Configuration Files (.txt)

```ini
# Comments start with #
[GradeNumberText]
Color=#FF0000          # Hex color (red)
FontSize=24           # Font size in points
OffsetX=50            # X offset from original position (positive = right)
OffsetY=-20           # Y offset from original position (positive = down)

[GradeDescriptionText]
Color=#00FF00         # Green
FontSize=18
OffsetX=0             # No offset = original position
OffsetY=0

[GradeNameText]
Color=#0000FF         # Blue
FontSize=16
OffsetX=-30
OffsetY=10

[GradeExpansionRarityText]
Color=#FFFF00         # Yellow
FontSize=14
OffsetX=20
OffsetY=5
```

### Available Text Elements

1. **GradeNumberText** - The grade number (1, 2, 3, etc.)
2. **GradeDescriptionText** - Grade description ("Poor", "Fair", "Good", etc.)
3. **GradeNameText** - Card name
4. **GradeExpansionRarityText** - Expansion and rarity information

### Configuration Properties

- **Color**: Hex format (`#RRGGBB`) or named colors
- **FontSize**: Numeric value for font size
- **OffsetX**: Horizontal offset from original position (pixels)
- **OffsetY**: Vertical offset from original position (pixels)

**Important**: Offsets are relative to the original game positioning. `OffsetX=0, OffsetY=0` maintains vanilla positioning.

## System Behavior

### Grade Detection
- System automatically detects `cardData.cardGrade` (1-10)
- Loads corresponding `{grade}.png` and `{grade}.txt`
- Falls back to `GradedCardCase.png` and `GradedCardCase.txt` if grade-specific files don't exist
- Disables label entirely if no assets are found

### Rendering Order
1. **CardBackMeshBlocker disabled** - Prevents card back texture interference
2. **Label background applied** - Custom PNG stretched to 615x160 area
3. **Text configuration applied** - Colors, sizes, and positions updated
4. **Original game text rendered** - With custom styling on top of custom background

## Technical Implementation

### Key Code Components

**Plugin.cs**:
- `LoadGradeSpecificAssets()` - Loads all PNG and TXT files on startup
- `GradeSprites` dictionary - Stores loaded sprites by grade (key 0 = fallback)
- `GradeConfigs` dictionary - Stores text configurations by grade

**Card3dUIGroupDisableCardBackPatch.cs**:
- Patches `Card3dUIGroup.EvaluateCardGrade()` method
- Disables `CardBackMeshBlocker` renderer
- Applies grade-specific sprite to `LabelImageBack`
- Applies text configuration with offset positioning

### Nullable Configuration System
- Uses `Color?`, `float?`, `Vector2?` for optional configuration
- Only applies explicitly configured values
- Preserves original game settings for unconfigured properties

## Troubleshooting

### Common Issues

**Black rectangle still visible**:
- Ensure `CardBackMeshBlocker` is being disabled
- Check that `LabelImageBack` is receiving the custom sprite
- Verify PNG has proper transparency

**Text not appearing with custom colors**:
- Check config file syntax (proper section headers `[SectionName]`)
- Verify hex color format (`#RRGGBB`)
- Ensure text configuration loading logs appear

**Positioning not working**:
- Use `OffsetX`/`OffsetY` instead of absolute positioning
- Remember: positive X = right, positive Y = down
- Check that offset values are being loaded in logs

**Images not loading**:
- Verify 615x160 pixel dimensions
- Ensure PNG format
- Check file permissions and paths

### Debug Logging

The system provides extensive logging:
- Asset loading: `"Loaded grade X sprite: 615x160"`
- Config parsing: `"Found section [GradeNumberText] in 10.txt, valid: True"`
- Color loading: `"Loaded color RGBA(1.000, 0.000, 0.000, 1.000)"`
- Application: `"Applied color RGBA(...) to GradeNumberText"`
- Positioning: `"Applied offset (50.00, -20.00) to GradeNumberText (original: (100.00, 200.00), new: (150.00, 180.00))"`

## Advanced Customization

### Per-Grade Customization
Create unique looks for each grade:
- Grade 1-3: Basic colors and positioning
- Grade 4-6: Enhanced styling with offsets
- Grade 7-9: Premium appearance with custom backgrounds
- Grade 10: Special gold/rainbow effects

### Fallback Strategy
1. Try grade-specific files (`10.png`, `10.txt`)
2. Fall back to `GradedCardCase.png`, `GradedCardCase.txt`
3. Disable label if no assets found

### Asset Organization
```
/ModDirectory/
├── grades/
│   ├── 1.png, 1.txt
│   ├── 2.png, 2.txt
│   └── ...
├── fallback/
│   ├── GradedCardCase.png
│   └── GradedCardCase.txt
└── GradedCardCaseExpander.dll
```

## Investigation History & Evolution

### Phase 1: Unity Asset Analysis (UnityPy)
**Target**: Replace static `GradedCardCase` texture (ID:133, 1024x1024) in sharedassets0.assets
**Approach**: Direct asset file modification
**Challenges**:
- Static replacement only
- No per-grade customization
- Complex asset dependency management

**Key Asset Discoveries**:
- `GradedCardCase` Texture2D (ID:133) and Sprite (ID:431) in sharedassets0.assets
- `T_CardBackMesh` texture (ID:1306) causing interference
- 936 card-related GameObjects analyzed across asset files
- Complete GameObject hierarchy mapped: `GradedCardCase` (ID:17644), `GradedCardTextureMask` (ID:8372), etc.

### Phase 2: Runtime Investigation
**Method**: Harmony patches and component inspection
**Key Findings**:
- `ShowGradedCardCase()` method controls visibility via `m_GradedCardCaseGrp.SetActive()`
- Canvas UI system (RectTransform) rather than pure 3D mesh rendering
- `Card3dUIGroup.EvaluateCardGrade()` identified as central control point

### Phase 3: The Black Rectangle Mystery
**Problem**: Transparent replacement resulted in black rectangle
**Initial Assumption**: 3D mesh materials causing issue
**Investigation Path**:
1. Tested disabling `Slab_BaseMesh` and `Slab_TopLayerMesh` - rectangle persisted
2. Analyzed shader systems - Standard vs Transparent shaders
3. **Breakthrough**: Discovered UI Image components were the actual source

**Root Cause Resolution**: `LabelImageBack` UI component (not 3D mesh) was rendering the black background using a 32x32 `WhiteTile` sprite stretched to 615x160 pixels.

### Phase 4: Dynamic Runtime System
**Evolution**: From static asset replacement to dynamic runtime customization
**Advantages**:
- Per-grade customization (1.png through 10.png)
- Dynamic text styling with offset positioning
- Fallback system (GradedCardCase.png/txt)
- Real-time configuration without asset rebuilding
- Proper transparency support

**Technical Implementation**:
- Nullable configuration system (`Color?`, `float?`, `Vector2?`)
- Offset-based positioning relative to original game layout
- Centralized asset loading and management
- Comprehensive logging and debugging support

### Naming Convention Pitfalls
**Critical Learning**: Never trust variable names in this codebase
- `LabelImageBack` = Main visible component
- `LabelImage` = Hidden/unused component
- `m_Show2DGradedCase` = Suggests 3D case exists separately
- `CardBackMeshBlocker` = Actually prevents card back from showing through

This investigation demonstrates how initial assumptions about system architecture can be misleading, and the importance of runtime inspection over static analysis when dealing with complex Unity systems.