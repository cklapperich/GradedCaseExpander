#!/usr/bin/env python3
"""
Analyze the GradedCardCase GameObject and related graded card objects
"""

import UnityPy
from pathlib import Path

def analyze_graded_card_objects(game_path):
    """Analyze the GradedCardCase and related objects"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"
    asset_path = data_dir / "sharedassets1.assets"

    # Target objects we found
    targets = [
        {'name': 'GradedCardTextureMask', 'id': 8372},
        {'name': 'GradedCardTexture', 'id': 15030},
        {'name': 'GradedCardFrontScaling', 'id': 17409},
        {'name': 'GradedCardCase', 'id': 17644}
    ]

    print("=== ANALYZING GRADED CARD GAMEOBJECTS ===\n")

    try:
        env = UnityPy.load(str(asset_path))

        for target in targets:
            print(f"Analyzing: {target['name']} (ID: {target['id']})")

            # Find the object
            for obj in env.objects:
                if obj.path_id == target['id'] and obj.type.name == "GameObject":
                    try:
                        data = obj.read()

                        # Basic info
                        name = getattr(data, 'name', 'unnamed')
                        print(f"  Name: {name}")
                        print(f"  Type: {obj.type.name}")

                        # Components
                        if hasattr(data, 'm_Component'):
                            print(f"  Components ({len(data.m_Component)}):")
                            for i, component in enumerate(data.m_Component):
                                if hasattr(component, 'component'):
                                    comp_ref = component.component
                                    if hasattr(comp_ref, 'path_id'):
                                        # Try to find what type this component is
                                        comp_obj = None
                                        for comp_search in env.objects:
                                            if comp_search.path_id == comp_ref.path_id:
                                                comp_obj = comp_search
                                                break

                                        comp_type = comp_obj.type.name if comp_obj else "Unknown"
                                        print(f"    {i}: {comp_type} (ID: {comp_ref.path_id})")

                        # Transform
                        if hasattr(data, 'm_Transform'):
                            transform_ref = data.m_Transform
                            if hasattr(transform_ref, 'path_id'):
                                print(f"  Transform: ID {transform_ref.path_id}")

                        # Parent
                        if hasattr(data, 'm_Father'):
                            father_ref = data.m_Father
                            if hasattr(father_ref, 'path_id') and father_ref.path_id != 0:
                                print(f"  Parent: ID {father_ref.path_id}")

                        # Active state
                        if hasattr(data, 'm_IsActive'):
                            print(f"  Active: {data.m_IsActive}")

                        # Layer
                        if hasattr(data, 'm_Layer'):
                            print(f"  Layer: {data.m_Layer}")

                        # Tag
                        if hasattr(data, 'm_Tag'):
                            print(f"  Tag: {data.m_Tag}")

                        print()  # Empty line between objects

                    except Exception as e:
                        print(f"  Error analyzing {target['name']}: {e}\n")
                    break

    except Exception as e:
        print(f"Error loading asset file: {e}")

def find_image_components(game_path):
    """Find Image components that might use our GradedCardCase texture"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"
    asset_path = data_dir / "sharedassets1.assets"

    print("=== SEARCHING FOR IMAGE COMPONENTS ===\n")

    try:
        env = UnityPy.load(str(asset_path))

        graded_case_sprite_id = 431  # From sharedassets0.assets

        for obj in env.objects:
            if obj.type.name == "Image":
                try:
                    data = obj.read()

                    # Check if this Image uses our GradedCardCase sprite
                    if hasattr(data, 'm_Sprite'):
                        sprite_ref = data.m_Sprite
                        if hasattr(sprite_ref, 'path_id'):
                            if sprite_ref.path_id == graded_case_sprite_id:
                                print(f"*** FOUND IMAGE USING GradedCardCase SPRITE ***")
                                print(f"Image Component ID: {obj.path_id}")

                                # Get more details
                                if hasattr(data, 'm_GameObject'):
                                    go_ref = data.m_GameObject
                                    if hasattr(go_ref, 'path_id'):
                                        print(f"GameObject ID: {go_ref.path_id}")

                                print()

                except Exception as e:
                    continue

    except Exception as e:
        print(f"Error searching for Image components: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    analyze_graded_card_objects(game_path)
    find_image_components(game_path)

if __name__ == "__main__":
    main()