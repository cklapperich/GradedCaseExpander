#!/usr/bin/env python3
"""
Extract and analyze graded card case assets from TCG Card Shop Simulator
"""

import UnityPy
import os
import sys
from pathlib import Path

def find_unity_files(game_path):
    """Find Unity asset files in the game directory"""
    unity_files = []
    game_dir = Path(game_path)

    if not game_dir.exists():
        print(f"Game directory not found: {game_path}")
        return []

    # Look for common Unity asset files
    for pattern in ["*.assets", "*.bundle", "*.unity3d", "*_Data/**/*.assets"]:
        unity_files.extend(game_dir.glob(pattern))

    # Focus on Card Shop Simulator_Data directory (the main Unity data folder)
    data_dir = game_dir / "Card Shop Simulator_Data"
    if data_dir.exists():
        print(f"Found data directory: {data_dir}")
        for pattern in ["*.assets", "*.bundle", "*.unity3d", "**/*.assets", "**/*.bundle"]:
            unity_files.extend(data_dir.glob(pattern))

    return unity_files

def extract_graded_case_assets(asset_file):
    """Extract graded card case related assets from a Unity file"""
    print(f"\nAnalyzing: {asset_file}")

    try:
        env = UnityPy.load(str(asset_file))
        graded_assets = []

        for obj in env.objects:
            try:
                # Look for textures with "graded" or "case" in the name
                if obj.type.name in ["Texture2D", "Sprite"]:
                    data = obj.read()
                    name = getattr(data, 'name', 'unnamed')

                    if any(keyword in name.lower() for keyword in ['graded', 'case', 'grade']):
                        print(f"Found {obj.type.name}: {name}")
                        graded_assets.append({
                            'type': obj.type.name,
                            'name': name,
                            'path_id': obj.path_id,
                            'data': data
                        })

                # Look for meshes
                elif obj.type.name == "Mesh":
                    data = obj.read()
                    name = getattr(data, 'name', 'unnamed')

                    if any(keyword in name.lower() for keyword in ['graded', 'case', 'grade']):
                        print(f"Found Mesh: {name}")
                        graded_assets.append({
                            'type': obj.type.name,
                            'name': name,
                            'path_id': obj.path_id,
                            'data': data
                        })

                # Look for GameObjects with graded/case in name
                elif obj.type.name == "GameObject":
                    data = obj.read()
                    name = getattr(data, 'name', 'unnamed')

                    if any(keyword in name.lower() for keyword in ['graded', 'case', 'grade']):
                        print(f"Found GameObject: {name}")
                        graded_assets.append({
                            'type': obj.type.name,
                            'name': name,
                            'path_id': obj.path_id,
                            'data': data
                        })

            except Exception as e:
                # Skip objects that can't be read
                continue

        return graded_assets

    except Exception as e:
        print(f"Error reading {asset_file}: {e}")
        return []

def save_textures(assets, output_dir):
    """Save texture assets to files"""
    output_path = Path(output_dir)
    output_path.mkdir(exist_ok=True)

    for asset in assets:
        if asset['type'] in ['Texture2D', 'Sprite']:
            try:
                data = asset['data']
                name = asset['name']

                if asset['type'] == 'Texture2D':
                    # Save as PNG
                    img = data.image
                    if img:
                        img.save(output_path / f"{name}_texture.png")
                        print(f"Saved texture: {name}_texture.png")

                elif asset['type'] == 'Sprite':
                    # Save sprite texture
                    if hasattr(data, 'texture') and data.texture:
                        texture_data = data.texture.read()
                        img = texture_data.image
                        if img:
                            img.save(output_path / f"{name}_sprite.png")
                            print(f"Saved sprite: {name}_sprite.png")

            except Exception as e:
                print(f"Error saving {asset['name']}: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    if len(sys.argv) > 1:
        game_path = sys.argv[1]

    print(f"Searching for Unity assets in: {game_path}")

    unity_files = find_unity_files(game_path)
    if not unity_files:
        print("No Unity asset files found!")
        return

    print(f"Found {len(unity_files)} Unity asset files")

    all_graded_assets = []

    for unity_file in unity_files:
        assets = extract_graded_case_assets(unity_file)
        all_graded_assets.extend(assets)

    if all_graded_assets:
        print(f"\n=== SUMMARY: Found {len(all_graded_assets)} graded card case assets ===")

        # Group by type
        by_type = {}
        for asset in all_graded_assets:
            asset_type = asset['type']
            if asset_type not in by_type:
                by_type[asset_type] = []
            by_type[asset_type].append(asset)

        for asset_type, assets in by_type.items():
            print(f"\n{asset_type} ({len(assets)}):")
            for asset in assets:
                print(f"  - {asset['name']} (path_id: {asset['path_id']})")

        # Save textures
        save_textures(all_graded_assets, "extracted_graded_assets")

    else:
        print("\nNo graded card case assets found.")
        print("Searching for any assets with 'card' in the name...")

        # Broader search
        for unity_file in unity_files[:3]:  # Check first few files
            try:
                env = UnityPy.load(str(unity_file))
                for obj in env.objects:
                    if obj.type.name in ["Texture2D", "Sprite", "GameObject"]:
                        try:
                            data = obj.read()
                            name = getattr(data, 'name', 'unnamed')
                            if 'card' in name.lower():
                                print(f"Found card-related {obj.type.name}: {name}")
                        except:
                            continue
            except:
                continue

if __name__ == "__main__":
    main()