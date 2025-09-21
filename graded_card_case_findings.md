# Graded Card Case Texture System Findings

## Modding Target
**Replace graded case textures with custom artwork** while maintaining game functionality.

## Core Issue for Modding
**Card back texture renders on the FRONT of graded cards** - appears over the graded case mesh but beneath the graded case texture, affecting visual appearance.

## Key Components

### CardUI.cs
- `ShowGradedCardCase(bool isShow)` - Controls case visibility
- `m_GradedCardCaseGrp` - Main container GameObject
- Shows when `m_CardData.cardGrade > 0`
- **Weird Behavior**: Our logs showed `isShow=false` consistently even when viewing graded cards, suggesting the method may be called differently than expected or cases display through other means

### MonsterData_ScriptableObject.cs
- `GetGradedCardScratchTexture(int cardGrade)` - Returns scratch overlay textures
- `GetCardBackSprite(ECardExpansionType)` - Returns card back sprites
- **Key**: Scratch texture ≠ main case texture

## Structure Discovered
- Graded case = UI Image component
- Uses "GradedCardCase" sprite (1024x1024)
- Hierarchy: Container → Image component with sprite

## Rendering Layers (Front of Card)
```
Bottom to top:
1. Graded case mesh
2. Card back texture ← Interferes with case appearance
3. Graded case texture
```

## Modding Approach
Need to intercept and replace the "GradedCardCase" sprite while handling card back texture interference.

## Unity Asset Discovery (UnityPy Analysis)

### Target Assets Confirmed Found
**GradedCardCase**
- **Texture2D**: ID:133 in sharedassets0.assets
- **Sprite**: ID:431 in sharedassets0.assets
- Size: Expected 1024x1024 (confirmed from earlier findings)

**T_CardBackMesh**
- **Texture2D**: ID:1306 in sharedassets1.assets
- This is the texture that renders on the front of graded cards causing visual interference

### Related Card Assets Discovered
**Card Back Textures (by expansion):**
- `CardBack` (base) - ID:109 in sharedassets0.assets
- `CardBackCatJob` - ID:799 in sharedassets1.assets
- `CardBackFantasyRPG` - ID:982 in sharedassets1.assets
- `CardBackMegabot` - ID:948 in sharedassets1.assets

**Card Background Textures:**
- `CardBG_Air`, `CardBG_Champion`, `CardBG_Destiny`, `CardBG_Earth` - sharedassets0.assets
- `CardBG_CatJob`, `CardBG_FantasyRPG` - sharedassets1.assets

**Card Enhancement Assets:**
- `T_CardLegendGlow`, `T_CardLegendGlow2`, `T_CardLegendGlowAlpha2` - sharedassets0.assets
- Various card pack textures: `T_CardPack[Type]` - sharedassets1.assets
- Card sleeve textures: `T_CardSleeve[Type]` - sharedassets1.assets

### Asset File Structure
- **sharedassets0.assets**: Contains core card UI elements (GradedCardCase, basic card backs, legend glows)
- **sharedassets1.assets**: Contains expansion-specific content (T_CardBackMesh, expansion card backs, packs, sleeves)
- **resources.assets**: Contains additional game resources
- Total texture/sprite assets: 2,749 across all files

### Key GameObject Discoveries
**Critical Graded Card GameObjects Found:**
- `GradedCardTextureMask` - ID:8372 in sharedassets1.assets [4 components]
- `CardBackMesh` - ID:9406 in sharedassets1.assets [3 components]
- `GhostCardGrp` - ID:10034 in sharedassets1.assets [2 components]

**Related Card UI GameObjects:**
- `CardBorder` - ID:8687, ID:12332 in sharedassets1.assets [3-4 components]
- `CardBorderMaskFoil` - ID:4594 in sharedassets1.assets [4 components]
- `CardFront` - ID:6941, ID:10036, ID:11522 in sharedassets1.assets
- `CardBack` - ID:10051 in sharedassets1.assets [3 components]

**Card Group Containers (the 'grp' references):**
- `GhostCardGrp` - Ghost card container system
- `CardGrp` (multiple variants 1-7) - General card grouping containers
- `Card3dUIGroup` - 3D card UI container
- `CardHandPivotGrp` - Card hand positioning system
- `Card_Ctrl_Grp` - Card control system groups

### Asset Analysis Summary
- **Total GameObjects analyzed**: 936 card-related objects
- **Mesh assets**: Multiple card-related meshes found
- **Material assets**: Various card materials discovered
- **Component structure**: Most card objects have 1-5 components

### Critical Discovery: The Graded Card System GameObject Hierarchy

**Found the actual `m_GradedCardCaseGrp` equivalent!**

The system uses `GradedCardCase` GameObject (ID:17644) in sharedassets1.assets, which is likely the container referenced in CardUI.cs as `m_GradedCardCaseGrp`.

**Complete Graded Card GameObject System:**
- `GradedCardCase` - ID:17644 [3 components] **← Main container (likely m_GradedCardCaseGrp)**
  - RectTransform (ID:51913)
  - CanvasRenderer (ID:51000)
  - MonoBehaviour (ID:52816) **← Likely Image component with GradedCardCase sprite**
  - **Status**: Active: False (hidden by default)
  - **Layer**: 5 (UI layer)

- `GradedCardTextureMask` - ID:8372 [4 components]
  - RectTransform, CanvasRenderer, 2x MonoBehaviour components
  - **Purpose**: Texture masking for graded cards

- `GradedCardTexture` - ID:15030 [3 components]
  - RectTransform, CanvasRenderer, MonoBehaviour
  - **Purpose**: Main texture display component

- `GradedCardFrontScaling` - ID:17409 [1 component]
  - Only RectTransform
  - **Purpose**: Scaling/positioning for front display

### Rendering System Analysis
**Layer Structure (UI Layer 5):**
1. `GradedCardCase` (container) - contains the case texture
2. `GradedCardTextureMask` - masks/clips the content
3. `GradedCardTexture` - the actual card texture
4. `GradedCardFrontScaling` - scaling container

**Key Insight**: All objects use **RectTransform** (UI system) rather than regular Transform, confirming this is a **Canvas-based UI system**, not 3D mesh rendering.

### Modding Implications
- The `GradedCardCase` GameObject contains the Image component that renders the case texture
- MonoBehaviour ID:52816 is likely the Image component that references the GradedCardCase sprite (ID:431)
- The case starts as `Active: False` and gets activated when `ShowGradedCardCase(true)` is called
- All components are on UI Layer 5, making this a Canvas UI overlay system

### BREAKTHROUGH: CardUI.cs Code Analysis Combined with Unity Assets

**Confirmed Variable Mappings:**
- **Line 590**: `public GameObject m_GradedCardCaseGrp;` ← This IS our GameObject ID:17644!
- **Line 593**: `public bool m_Show2DGradedCase;` ← **KEY**: This suggests there's also a **3D** graded case!
- **Line 596**: `public Transform m_GradedCardFrontScaling;` ← GameObject ID:17409 for scaling
- **Line 656**: `public Image m_GradedCardTextureImage;` ← The scratch texture overlay

**ShowGradedCardCase Method Analysis (Lines 124-143):**
```csharp
public void ShowGradedCardCase(bool isShow)
{
    if (this.m_CardData.cardGrade <= 0) { isShow = false; }
    this.m_GradedCardCaseGrp.SetActive(isShow);  // ← Activates entire graded case system

    if (isShow) {
        // Sets grade info text, positions card front
        this.m_CardFront.transform.localPosition = this.m_GradedCardFrontScaling.transform.localPosition;
        this.m_CardFront.transform.localScale = this.m_GradedCardFrontScaling.transform.localScale;
    }
}
```

**What We Know for Certain**:
- When `cardGrade > 0`, the entire graded case system activates via `SetActive(isShow)`
- The `m_GradedCardCaseGrp` (GameObject ID:17644) contains both UI elements and likely 3D geometry
- Ungraded cards (`cardGrade <= 0`) have no case at all - just the card
- When graded, the 3D case geometry and textures appear together as one system
- The case has physical ridges and depth (3D mesh) plus the flat case texture

**SetCardUI Method (Line 432-433):**
```csharp
this.m_GradedCardTextureImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetGradedCardScratchTexture(this.m_CardData.cardGrade);
this.ShowGradedCardCase(this.m_Show2DGradedCase);
```

**System Architecture (Confirmed):**
1. `m_GradedCardCaseGrp` (GameObject ID:17644) - **Main container** - SetActive controls entire system
2. Contains child objects:
   - `GradedCardTextureMask` (ID:8372) - Masking component
   - `GradedCardTexture` (ID:15030) - Texture display
   - `GradedCardFrontScaling` (ID:17409) - Scaling reference
   - **Unknown 3D mesh component(s)** - Physical case geometry
3. `m_GradedCardTextureImage` - Scratch texture overlay
4. **GradedCardCase texture** (ID:133) - The case surface texture

**Missing Piece**: The 3D mesh with ridges and geometry hasn't been located in our asset analysis yet, but it must exist within this GameObject group since the entire system activates/deactivates together.

**Modding Strategy**:
- **Target**: The GradedCardCase texture (ID:133) used somewhere in this GameObject group
- **Challenge**: Need to find how the 3D mesh references this texture (material system)
- **Activation**: Entire system controlled by `ShowGradedCardCase()` and `cardGrade > 0`

## Investigation Results (2025-01-19)

### Confirmed Findings

**Problem Reproduction:**
- GradedCardCase.png transparency replacement successful - problem is now clearly visible
- Card back texture is stretching awkwardly across graded cards (confirmed visual bug)
- Issue occurs on the front face of graded cards, interfering with case display

**What We've Ruled Out:**
- NOT normal card back UI components (`m_CardBack`, `m_CardBackImage`) - disabling had no effect
- NOT UI Image components within graded card case group - no card back textures found
- NOT 3D Renderer components in CardUI hierarchy - 0 Renderer components found
- NOT within `m_GradedCardCaseGrp` - sometimes NULL, doesn't contain the problem source

**System Architecture Confirmed:**
- Game uses Canvas UI system (RectTransform, Image components) for card display
- `GradedCardCase` GameObject often `Active: False` when problem occurs
- Graded case system controlled by `ShowGradedCardCase()` method

### Root Cause Identified

The card back texture is being rendered by components outside the CardUI hierarchy.

**Key Evidence:**
1. 0 Renderer components found in CardUI during inspection
2. CardBackMesh GameObject (ID:9406) exists separately with 3 components
3. T_CardBackMesh texture (ID:1306) confirmed in game assets
4. Rendering happens at higher hierarchy level than CardUI

### Current Status

**Completed Patches:**
- `GradedCardBackRemovalPatch.cs` - Full CardUI hierarchy inspection (confirmed 0 renderers)
- Transparent GradedCardCase.png replacement (successful visual confirmation)

**Next Required Action:**
- Patch at higher hierarchy level to find CardBackMesh GameObject
- Search entire scene for objects using T_CardBackMesh texture
- Disable CardBackMesh renderer when graded cards are displayed

**Solution Approach:**
The fix requires finding and disabling the `CardBackMesh` GameObject or its renderer components, which exist outside the CardUI component hierarchy and are likely siblings or parents to the CardUI system.

## Card3DUIGroup System Analysis (EvaluateCardGrade Patch)

### Discovered Renderer Components in Graded Card System

**Patch Target:** `Card3dUIGroup.EvaluateCardGrade()` method
**GameObject Container:** `m_GradedCardGrp` (3D graded card group)

### Complete Renderer Inventory

**CardBackMeshBlocker**
- **GameObject Name:** `CardBackMeshBlocker`
- **Status:** Always enabled initially, gets disabled by patch
- **Material:** `MAT_CardBackMesh (Instance)`
- **Texture:** `T_CardBackMesh`
- **Purpose:** Renders card back texture that interferes with graded case display
- **Action:** **Target for disabling** - this is the problematic renderer

**Slab_BaseMesh**
- **GameObject Name:** `Slab_BaseMesh`
- **Status:** Enabled (not affected by patch)
- **Material:** `trading_card 1 (Instance)`
- **Texture:** `trading_card_transparent`
- **Purpose:** Base layer of graded card case structure
- **Action:** Leave enabled

**Slab_TopLayerMesh**
- **GameObject Name:** `Slab_TopLayerMesh`
- **Status:** Enabled (not affected by patch)
- **Material:** `trading_card (Instance)`
- **Texture:** `null` (no main texture assigned)
- **Purpose:** Top layer of graded card case structure
- **Action:** Leave enabled

### System Behavior

**Execution Pattern:**
- `EvaluateCardGrade()` called repeatedly (multiple cards rendering)
- Each call processes exactly **3 renderers**
- Each call finds **3 materials** total (1 per renderer)
- **1 renderer** disabled per execution (`CardBackMeshBlocker`)
- Processing time: **7-9ms** per execution

**Key Insight:**
- The patch works correctly - disabling `CardBackMeshBlocker` prevents card back texture interference
- Each execution appears to be processing a different card instance
- The `CardBackMeshBlocker` always shows as `Enabled: True` at start because each card has its own instance

### Material & Texture Details

**MAT_CardBackMesh (Instance)**
- Applied to: `CardBackMeshBlocker`
- Main Texture: `T_CardBackMesh`
- **This is the interfering texture** that renders on graded card fronts

**trading_card 1 (Instance)**
- Applied to: `Slab_BaseMesh`
- Main Texture: `trading_card_transparent`
- Part of legitimate graded case structure

**trading_card (Instance)**
- Applied to: `Slab_TopLayerMesh`
- Main Texture: `null`
- Part of legitimate graded case structure

### Architecture Confirmed

The graded card 3D system (`Card3dUIGroup`) contains:
1. **Physical graded case mesh** (`Slab_BaseMesh`, `Slab_TopLayerMesh`)
2. **Card back blocking mesh** (`CardBackMeshBlocker`) ← **Problem source**
3. **Material system** using `trading_card` materials for case, `MAT_CardBackMesh` for back

### Solution Status

**Current Patch:** `Card3dUIGroupDisableCardBackPatch.cs`
- **Function:** Disables `CardBackMeshBlocker` renderer to prevent interference
- **Result:** Successfully removes card back texture from graded card fronts
- **Status:** Working correctly

## Files Modified
- `GradedCardCaseInspectorPatch.cs` - Created for inspection (removed)
- `GradedCardBackRemovalPatch.cs` - Full hierarchy inspection patch
- `Card3dUIGroupDisableCardBackPatch.cs` - **Working solution** for CardBackMeshBlocker interference
- `/objects_textures/GradedCardCase.png` - Replaced with transparent version (backup created)