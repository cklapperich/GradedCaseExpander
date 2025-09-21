#!/usr/bin/env python3
"""
Find specific graded card case assets: T_CardBackMesh.png and GradedCardCase.png
"""

import UnityPy
import os
import sys
from pathlib import Path

def find_specific_assets(game_path, target_names):
    """Find specific assets by name"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    if not data_dir.exists():
        print(f"Data directory not found: {data_dir}")
        return []

    print(f"Searching in: {data_dir}")

    # Find Unity asset files
    unity_files = []
    for pattern in ["*.assets", "*.bundle", "**/*.assets", "**/*.bundle"]:
        unity_files.extend(data_dir.glob(pattern))

    print(f"Found {len(unity_files)} Unity files to search")

    found_assets = []

    for unity_file in unity_files:
        print(f"Checking: {unity_file.name}")

        try:
            env = UnityPy.load(str(unity_file))

            for obj in env.objects:
                try:
                    if obj.type.name in ["Texture2D", "Sprite"]:
                        data = obj.read()
                        name = getattr(data, 'name', 'unnamed')

                        # Check if this is one of our target assets
                        for target in target_names:
                            if target.lower() in name.lower() or name.lower() in target.lower():
                                print(f"*** FOUND TARGET: {name} (type: {obj.type.name}) ***")
                                found_assets.append({
                                    'name': name,
                                    'type': obj.type.name,
                                    'file': unity_file.name,
                                    'path_id': obj.path_id,
                                    'data': data
                                })

                        # Also report any assets with "card" and "case" or "grade"
                        if (('card' in name.lower() and ('case' in name.lower() or 'grade' in name.lower())) or
                            ('graded' in name.lower())):
                            print(f"Related asset: {name} (type: {obj.type.name})")

                except Exception:
                    continue

        except Exception as e:
            print(f"Error reading {unity_file}: {e}")
            continue

    return found_assets

def save_found_assets(assets, output_dir):
    """Save the found assets"""
    output_path = Path(output_dir)
    output_path.mkdir(exist_ok=True)

    for asset in assets:
        try:
            data = asset['data']
            name = asset['name']

            if asset['type'] == 'Texture2D':
                img = data.image
                if img:
                    filename = f"{name}_from_{asset['file']}.png"
                    img.save(output_path / filename)
                    print(f"Saved: {filename}")

            elif asset['type'] == 'Sprite':
                if hasattr(data, 'texture') and data.texture:
                    texture_data = data.texture.read()
                    img = texture_data.image
                    if img:
                        filename = f"{name}_sprite_from_{asset['file']}.png"
                        img.save(output_path / filename)
                        print(f"Saved: {filename}")

        except Exception as e:
            print(f"Error saving {asset['name']}: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    # Target asset names we're looking for
    target_names = ["T_CardBackMesh", "GradedCardCase"]

    print(f"Looking for assets: {target_names}")

    found_assets = find_specific_assets(game_path, target_names)

    if found_assets:
        print(f"\n=== FOUND {len(found_assets)} TARGET ASSETS ===")
        for asset in found_assets:
            print(f"- {asset['name']} ({asset['type']}) in {asset['file']}")

        save_found_assets(found_assets, "extracted_target_assets")
    else:
        print("\nTarget assets not found!")

if __name__ == "__main__":
    main()