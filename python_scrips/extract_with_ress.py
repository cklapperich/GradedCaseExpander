#!/usr/bin/env python3
"""
Extract assets including .resS files which often contain texture data
"""

import UnityPy
from pathlib import Path

def extract_with_ress_files(game_path):
    """Extract assets including .resS files for texture data"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    if not data_dir.exists():
        print(f"Data directory not found: {data_dir}")
        return

    # Try different asset file combinations
    asset_files = [
        "sharedassets0.assets",
        "sharedassets1.assets",
        "resources.assets",
        "level1"
    ]

    found_assets = []

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            print(f"Skipping missing file: {asset_file}")
            continue

        print(f"\nProcessing: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            # Look for all objects
            texture_count = 0
            sprite_count = 0

            for obj in env.objects:
                try:
                    if obj.type.name == "Texture2D":
                        data = obj.read()
                        name = getattr(data, 'name', f'texture_{obj.path_id}')
                        texture_count += 1

                        # Check for our target patterns
                        if any(keyword in name.lower() for keyword in ['graded', 'case', 'card', 'mesh', 't_']):
                            print(f"*** POTENTIAL MATCH: {name} (Texture2D) ***")
                            found_assets.append({
                                'name': name,
                                'type': 'Texture2D',
                                'file': asset_file,
                                'path_id': obj.path_id,
                                'data': data
                            })

                    elif obj.type.name == "Sprite":
                        data = obj.read()
                        name = getattr(data, 'name', f'sprite_{obj.path_id}')
                        sprite_count += 1

                        # Check for our target patterns
                        if any(keyword in name.lower() for keyword in ['graded', 'case', 'card', 'mesh']):
                            print(f"*** POTENTIAL MATCH: {name} (Sprite) ***")
                            found_assets.append({
                                'name': name,
                                'type': 'Sprite',
                                'file': asset_file,
                                'path_id': obj.path_id,
                                'data': data
                            })

                except Exception as e:
                    continue

            print(f"  Found {texture_count} textures, {sprite_count} sprites")

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    return found_assets

def save_found_textures(assets, output_dir="extracted_textures"):
    """Save found textures to files"""
    output_path = Path(output_dir)
    output_path.mkdir(exist_ok=True)

    for asset in assets:
        try:
            data = asset['data']
            name = asset['name']

            if asset['type'] == 'Texture2D':
                img = data.image
                if img:
                    filename = f"{name}_{asset['path_id']}.png"
                    img.save(output_path / filename)
                    print(f"Saved: {filename}")

            elif asset['type'] == 'Sprite':
                if hasattr(data, 'texture') and data.texture:
                    texture_data = data.texture.read()
                    img = texture_data.image
                    if img:
                        filename = f"{name}_sprite_{asset['path_id']}.png"
                        img.save(output_path / filename)
                        print(f"Saved: {filename}")

        except Exception as e:
            print(f"Error saving {asset['name']}: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    found_assets = extract_with_ress_files(game_path)

    if found_assets:
        print(f"\n=== FOUND {len(found_assets)} POTENTIAL ASSETS ===")
        for asset in found_assets:
            print(f"- {asset['name']} ({asset['type']}) in {asset['file']}")

        save_found_textures(found_assets)
    else:
        print("\nNo matching assets found.")

if __name__ == "__main__":
    main()