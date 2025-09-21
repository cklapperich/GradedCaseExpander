#!/usr/bin/env python3
"""
Broad search for any card-related assets to understand what's available
"""

import UnityPy
import os
import sys
from pathlib import Path

def search_all_assets(game_path):
    """Search for all texture/sprite assets to see what's available"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    if not data_dir.exists():
        print(f"Data directory not found: {data_dir}")
        return

    # Find Unity asset files
    unity_files = []
    for pattern in ["*.assets", "*.bundle"]:
        unity_files.extend(data_dir.glob(pattern))

    print(f"Searching {len(unity_files)} Unity files...")

    all_textures = []
    card_related = []
    graded_related = []

    for unity_file in unity_files:
        print(f"Checking: {unity_file.name}")

        try:
            env = UnityPy.load(str(unity_file))

            for obj in env.objects:
                try:
                    if obj.type.name in ["Texture2D", "Sprite"]:
                        data = obj.read()
                        name = getattr(data, 'name', 'unnamed')

                        all_textures.append(name)

                        # Check for card-related assets
                        if 'card' in name.lower():
                            card_related.append(f"{name} ({obj.type.name}) in {unity_file.name}")

                        # Check for graded/case related assets
                        if any(keyword in name.lower() for keyword in ['graded', 'case', 'grade', 'mesh']):
                            graded_related.append(f"{name} ({obj.type.name}) in {unity_file.name}")

                except Exception:
                    continue

        except Exception as e:
            print(f"Error reading {unity_file}: {e}")
            continue

    print(f"\n=== SUMMARY ===")
    print(f"Total textures/sprites found: {len(all_textures)}")
    print(f"Card-related assets: {len(card_related)}")
    print(f"Graded/case-related assets: {len(graded_related)}")

    if card_related:
        print(f"\n=== CARD-RELATED ASSETS ({len(card_related)}) ===")
        for asset in card_related:
            print(f"  {asset}")

    if graded_related:
        print(f"\n=== GRADED/CASE-RELATED ASSETS ({len(graded_related)}) ===")
        for asset in graded_related:
            print(f"  {asset}")

    # Look for partial matches to our targets
    print(f"\n=== SEARCHING FOR PARTIAL MATCHES ===")
    targets = ['cardback', 'mesh', 'graded', 'case']
    for target in targets:
        matches = [name for name in all_textures if target in name.lower()]
        if matches:
            print(f"Assets containing '{target}': {matches}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"
    search_all_assets(game_path)

if __name__ == "__main__":
    main()