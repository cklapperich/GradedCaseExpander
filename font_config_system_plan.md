# Font Configuration System Plan

## Overview
Implement a system where font configurations are stored in separate files and referenced by grade configuration sections. This allows for reusable font styling across different grades and text elements.

## File Structure

### Font Config Files
- **Naming**: `font_xxx.txt` (e.g., `font_luxury_gold.txt`, `font_standard.txt`)
- **Location**: Plugin root directory alongside existing config files
- **Format**: Simple key=value pairs, no sections

Example `font_luxury_gold.txt`:
```
Font=wingdings
Color=#FFD700
FontSize=24.0
OffsetX=0.0
OffsetY=5.0
OutlineColor=#000000
OutlineWidth=2.0
```

### Grade Config Files
- **Files**: `1.txt` to `10.txt`, `DefaultLabel.txt` (existing structure)
- **Sections**: Keep existing sections (`[GradeNumberText]`, `[GradeDescriptionText]`, etc.)
- **New property**: `FontConfig` to reference font config files

Example `1.txt`:
```
[GradeNumberText]
FontConfig=luxury_gold
Text=★ {grade} ★

[GradeDescriptionText]
FontConfig=standard
Text=MINT CONDITION

[GradeNameText]
FontConfig=company_branding
```

## Reference System
- Grade configs can reference font configs by:
  - Short name: `FontConfig=luxury_gold`
  - Full name: `FontConfig=font_luxury_gold.txt`
- System will try both formats when looking up font configs

## Implementation Components

### 1. FontConfig Class
```csharp
public class FontConfig
{
    public string FontName { get; set; } = null;
    public Color? Color { get; set; } = null;
    public float? FontSize { get; set; } = null;
    public Vector2? Position { get; set; } = null;
    public Color? OutlineColor { get; set; } = null;
    public float? OutlineWidth { get; set; } = null;
}
```

### 2. Font Config Loading
- Load all `font_*.txt` files during plugin initialization
- Cache them in `Dictionary<string, FontConfig>`
- Support lookup by both short name and full filename

### 3. Grade Config Processing
- When processing grade config sections, check for `FontConfig` property
- Load referenced font config and apply its values to `GradedCardTextConfig`
- Allow section-specific properties to override font config values

## Loading Order
1. Load custom fonts (.ttf files)
2. Load font config files (`font_*.txt`)
3. Load grade-specific configs (`1.txt` to `10.txt`, `DefaultLabel.txt`)
4. Apply font configs when processing grade config sections

## Backward Compatibility
- Existing grade config files continue to work unchanged
- Font config system is purely additive
- If `FontConfig` property not specified, use inline font properties as before

## Benefits
- **Reusability**: Multiple grades can share the same font styling
- **Maintainability**: Font styles managed in dedicated files
- **Flexibility**: Can mix font configs with section-specific overrides
- **Clear separation**: Font styling separate from grade-specific properties like text content