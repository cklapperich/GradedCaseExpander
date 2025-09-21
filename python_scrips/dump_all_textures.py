#!/usr/bin/env python3
"""
Dump all texture names to a file for analysis
"""

import UnityPy
from pathlib import Path

def dump_all_texture_names(game_path, output_file="all_textures.txt"):
    """Dump all texture names to see what's available"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    if not data_dir.exists():
        print(f"Data directory not found: {data_dir}")
        return

    # Find Unity asset files
    unity_files = []
    for pattern in ["*.assets", "*.bundle"]:
        unity_files.extend(data_dir.glob(pattern))

    all_names = []

    for unity_file in unity_files:
        print(f"Processing: {unity_file.name}")

        try:
            env = UnityPy.load(str(unity_file))

            for obj in env.objects:
                try:
                    if obj.type.name in ["Texture2D", "Sprite"]:
                        data = obj.read()
                        name = getattr(data, 'name', 'unnamed')
                        all_names.append(f"{name} ({obj.type.name}) from {unity_file.name}")

                except Exception:
                    continue

        except Exception as e:
            print(f"Error reading {unity_file}: {e}")
            continue

    # Sort and save to file
    all_names.sort()

    with open(output_file, 'w') as f:
        for name in all_names:
            f.write(name + '\n')

    print(f"Saved {len(all_names)} texture names to {output_file}")

    # Look for potential matches
    potential_matches = []
    keywords = ['grade', 'case', 'mesh', 'back', 'border', 'ui']

    for name in all_names:
        name_lower = name.lower()
        for keyword in keywords:
            if keyword in name_lower:
                potential_matches.append(name)
                break

    if potential_matches:
        print(f"\nPotential matches found ({len(potential_matches)}):")
        for match in potential_matches[:20]:  # Show first 20
            print(f"  {match}")
        if len(potential_matches) > 20:
            print(f"  ... and {len(potential_matches) - 20} more")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"
    dump_all_texture_names(game_path)

if __name__ == "__main__":
    main()