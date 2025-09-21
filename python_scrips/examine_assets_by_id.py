#!/usr/bin/env python3
"""
Examine assets by dumping all names with path_ids to find patterns
"""

import UnityPy
from pathlib import Path

def examine_all_assets(game_path):
    """Examine all assets with their path IDs and actual names"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    asset_files = ["sharedassets0.assets", "sharedassets1.assets", "resources.assets"]

    all_assets = []

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Processing: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    if obj.type.name in ["Texture2D", "Sprite"]:
                        data = obj.read()

                        # Get all available attributes
                        name = getattr(data, 'name', '')

                        # Sometimes the name is empty but we can get it from m_Name
                        if not name and hasattr(data, 'm_Name'):
                            name = data.m_Name

                        # Create a fallback name if still empty
                        if not name:
                            name = f"unnamed_{obj.path_id}"

                        all_assets.append({
                            'name': name,
                            'type': obj.type.name,
                            'file': asset_file,
                            'path_id': obj.path_id,
                            'data': data
                        })

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    # Sort by name and save to file
    all_assets.sort(key=lambda x: x['name'])

    with open('all_assets_with_ids.txt', 'w') as f:
        for asset in all_assets:
            f.write(f"{asset['name']} ({asset['type']}) ID:{asset['path_id']} from {asset['file']}\n")

    print(f"Saved {len(all_assets)} assets to all_assets_with_ids.txt")

    # Look for any asset that might be our targets
    potential_matches = []
    search_terms = ['grade', 'case', 'card', 'mesh', 'back', 'border', 'ui']

    for asset in all_assets:
        name_lower = asset['name'].lower()
        for term in search_terms:
            if term in name_lower:
                potential_matches.append(asset)
                break

    if potential_matches:
        print(f"\nFound {len(potential_matches)} potential matches:")
        for match in potential_matches[:20]:
            print(f"  {match['name']} ({match['type']}) in {match['file']}")
    else:
        print("\nNo obvious matches found. Checking first 20 named assets:")
        named_assets = [a for a in all_assets if not a['name'].startswith('unnamed_')]
        for asset in named_assets[:20]:
            print(f"  {asset['name']} ({asset['type']}) in {asset['file']}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"
    examine_all_assets(game_path)

if __name__ == "__main__":
    main()