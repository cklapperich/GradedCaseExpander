#!/usr/bin/env python3
"""
Analyze materials to find which ones use the GradedCardCase texture
"""

import UnityPy
from pathlib import Path

def analyze_materials(game_path):
    """Find materials that use our target textures"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    # Our target texture IDs
    graded_case_texture_id = 133  # GradedCardCase texture
    graded_case_sprite_id = 431   # GradedCardCase sprite
    card_back_mesh_id = 1306      # T_CardBackMesh texture

    print("=== ANALYZING MATERIALS FOR GRADED CARD CASE ===\n")

    asset_files = ["sharedassets0.assets", "sharedassets1.assets"]

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Checking materials in: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    if obj.type.name == "Material":
                        data = obj.read()
                        name = getattr(data, 'name', f'material_{obj.path_id}')

                        # Check if material uses our target textures
                        found_target = False
                        texture_properties = []

                        if hasattr(data, 'm_SavedProperties'):
                            props = data.m_SavedProperties
                            if hasattr(props, 'm_TexEnvs'):
                                for tex_env in props.m_TexEnvs:
                                    if hasattr(tex_env, 'second') and hasattr(tex_env.second, 'm_Texture'):
                                        tex_ref = tex_env.second.m_Texture
                                        if hasattr(tex_ref, 'path_id'):
                                            tex_id = tex_ref.path_id
                                            prop_name = getattr(tex_env, 'first', 'unknown')

                                            if tex_id == graded_case_texture_id:
                                                found_target = True
                                                texture_properties.append(f"GradedCardCase texture as '{prop_name}'")
                                            elif tex_id == card_back_mesh_id:
                                                found_target = True
                                                texture_properties.append(f"T_CardBackMesh texture as '{prop_name}'")

                        if found_target:
                            print(f"\n*** MATERIAL USES TARGET TEXTURE: {name} (ID: {obj.path_id}) ***")
                            for prop in texture_properties:
                                print(f"    {prop}")

                            # Try to find what uses this material
                            find_material_users(env, obj.path_id, name)

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

def find_material_users(env, material_id, material_name):
    """Find MeshRenderers that use this material"""
    print(f"    Looking for objects using material '{material_name}' (ID: {material_id})...")

    users = []

    for obj in env.objects:
        try:
            if obj.type.name == "MeshRenderer":
                data = obj.read()

                if hasattr(data, 'm_Materials'):
                    for material_ref in data.m_Materials:
                        if hasattr(material_ref, 'path_id') and material_ref.path_id == material_id:
                            # Found a MeshRenderer using this material
                            # Try to find the associated GameObject
                            if hasattr(data, 'm_GameObject'):
                                go_ref = data.m_GameObject
                                if hasattr(go_ref, 'path_id'):
                                    go_name = find_gameobject_name(env, go_ref.path_id)
                                    users.append(f"MeshRenderer (ID: {obj.path_id}) on GameObject '{go_name}' (ID: {go_ref.path_id})")

        except Exception as e:
            continue

    if users:
        print(f"    Used by:")
        for user in users:
            print(f"      {user}")
    else:
        print(f"    No MeshRenderer users found")

def find_gameobject_name(env, go_id):
    """Find the name of a GameObject by ID"""
    for obj in env.objects:
        if obj.path_id == go_id and obj.type.name == "GameObject":
            try:
                data = obj.read()
                return getattr(data, 'name', f'unnamed_{go_id}')
            except:
                return f'gameobject_{go_id}'
    return f'unknown_{go_id}'

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"
    analyze_materials(game_path)

if __name__ == "__main__":
    main()