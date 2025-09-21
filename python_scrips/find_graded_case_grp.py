#!/usr/bin/env python3
"""
Find the m_GradedCardCaseGrp GameObject specifically
"""

import UnityPy
from pathlib import Path

def find_graded_case_grp(game_path):
    """Search for m_GradedCardCaseGrp and related objects"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    asset_files = ["sharedassets0.assets", "sharedassets1.assets", "resources.assets"]

    print("=== SEARCHING FOR m_GradedCardCaseGrp ===\n")

    all_grp_objects = []
    exact_matches = []

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Searching in: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    if obj.type.name == "GameObject":
                        data = obj.read()
                        name = getattr(data, 'name', '') or getattr(data, 'm_Name', '') or f'unnamed_{obj.path_id}'

                        # Look for exact match
                        if name == "m_GradedCardCaseGrp":
                            print(f"*** EXACT MATCH FOUND: {name} ***")
                            exact_matches.append({
                                'name': name,
                                'path_id': obj.path_id,
                                'file': asset_file,
                                'data': data
                            })

                        # Look for any object with "Graded" and "Grp" in name
                        elif 'graded' in name.lower() and 'grp' in name.lower():
                            print(f"Graded Group Object: {name} (ID: {obj.path_id})")
                            all_grp_objects.append({
                                'name': name,
                                'path_id': obj.path_id,
                                'file': asset_file
                            })

                        # Look for any object with "CardCase" and "Grp"
                        elif 'cardcase' in name.lower() and 'grp' in name.lower():
                            print(f"CardCase Group Object: {name} (ID: {obj.path_id})")
                            all_grp_objects.append({
                                'name': name,
                                'path_id': obj.path_id,
                                'file': asset_file
                            })

                        # Look for objects with just "GradedCard" in name
                        elif 'gradedcard' in name.lower():
                            print(f"GradedCard Object: {name} (ID: {obj.path_id})")
                            all_grp_objects.append({
                                'name': name,
                                'path_id': obj.path_id,
                                'file': asset_file
                            })

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    return exact_matches, all_grp_objects

def analyze_grp_components(exact_matches, game_path):
    """Analyze components of found objects"""
    if not exact_matches:
        return

    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    print("\n=== ANALYZING m_GradedCardCaseGrp COMPONENTS ===")

    for match in exact_matches:
        print(f"\nAnalyzing: {match['name']} (ID: {match['path_id']}) from {match['file']}")

        try:
            # Get component information
            data = match['data']
            if hasattr(data, 'm_Component'):
                print(f"Components ({len(data.m_Component)}):")
                for i, component in enumerate(data.m_Component):
                    if hasattr(component, 'component'):
                        comp_ref = component.component
                        if hasattr(comp_ref, 'path_id'):
                            print(f"  Component {i}: ID {comp_ref.path_id}")

            # Try to get transform info
            if hasattr(data, 'm_Transform'):
                transform_ref = data.m_Transform
                if hasattr(transform_ref, 'path_id'):
                    print(f"Transform: ID {transform_ref.path_id}")

            # Try to get parent/child relationships
            if hasattr(data, 'm_Father'):
                father_ref = data.m_Father
                if hasattr(father_ref, 'path_id') and father_ref.path_id != 0:
                    print(f"Parent: ID {father_ref.path_id}")

        except Exception as e:
            print(f"Error analyzing components: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    exact_matches, grp_objects = find_graded_case_grp(game_path)

    if exact_matches:
        print(f"\n=== FOUND {len(exact_matches)} EXACT MATCHES ===")
        for match in exact_matches:
            print(f"✓ {match['name']} (ID: {match['path_id']}) in {match['file']}")

        analyze_grp_components(exact_matches, game_path)

    elif grp_objects:
        print(f"\n=== NO EXACT MATCH, BUT FOUND {len(grp_objects)} RELATED OBJECTS ===")
        for obj in grp_objects:
            print(f"- {obj['name']} (ID: {obj['path_id']}) in {obj['file']}")

    else:
        print("\n=== NO MATCHES FOUND ===")
        print("The m_GradedCardCaseGrp object might be:")
        print("1. Named differently than expected")
        print("2. In a different asset file")
        print("3. Generated at runtime")
        print("4. Part of a prefab or scene not loaded")

if __name__ == "__main__":
    main()