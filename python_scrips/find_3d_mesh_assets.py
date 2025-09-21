#!/usr/bin/env python3
"""
Find the actual 3D mesh assets for graded card case geometry
"""

import UnityPy
from pathlib import Path

def find_mesh_assets(game_path):
    """Search for Mesh assets related to graded cards"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    asset_files = ["sharedassets0.assets", "sharedassets1.assets", "resources.assets"]

    print("=== SEARCHING FOR MESH ASSETS ===\n")

    all_meshes = []
    card_related_meshes = []

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Searching in: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    if obj.type.name == "Mesh":
                        data = obj.read()
                        name = getattr(data, 'name', '') or getattr(data, 'm_Name', '') or f'mesh_{obj.path_id}'

                        mesh_info = {
                            'name': name,
                            'path_id': obj.path_id,
                            'file': asset_file,
                            'vertex_count': getattr(data, 'm_VertexCount', 0),
                            'triangle_count': len(getattr(data, 'm_Triangles', [])) // 3 if hasattr(data, 'm_Triangles') else 0
                        }

                        all_meshes.append(mesh_info)

                        # Look for card/graded/case related meshes
                        search_terms = ['card', 'graded', 'case', 'grade']
                        if any(term in name.lower() for term in search_terms):
                            print(f"*** CARD-RELATED MESH: {name} (ID: {obj.path_id}) ***")
                            print(f"    Vertices: {mesh_info['vertex_count']}, Triangles: {mesh_info['triangle_count']}")
                            card_related_meshes.append(mesh_info)

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    print(f"\n=== MESH SEARCH RESULTS ===")
    print(f"Total meshes found: {len(all_meshes)}")
    print(f"Card-related meshes: {len(card_related_meshes)}")

    if card_related_meshes:
        print(f"\n=== CARD-RELATED MESHES ===")
        for mesh in card_related_meshes:
            print(f"- {mesh['name']} (ID: {mesh['path_id']}) in {mesh['file']}")
            print(f"  Vertices: {mesh['vertex_count']}, Triangles: {mesh['triangle_count']}")

    # Look for complex meshes that might be cases (high vertex count)
    complex_meshes = [m for m in all_meshes if m['vertex_count'] > 100]
    complex_meshes.sort(key=lambda x: x['vertex_count'], reverse=True)

    print(f"\n=== COMPLEX MESHES (>100 vertices, might include case meshes) ===")
    for mesh in complex_meshes[:20]:  # Show top 20 most complex
        print(f"- {mesh['name']} (ID: {mesh['path_id']}) - {mesh['vertex_count']} vertices")

    return card_related_meshes, all_meshes

def find_mesh_renderers_and_materials(game_path):
    """Find MeshRenderer components that might use our card textures"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    asset_files = ["sharedassets0.assets", "sharedassets1.assets"]

    print(f"\n=== SEARCHING FOR MESH RENDERERS ===")

    graded_case_texture_id = 133  # Our GradedCardCase texture
    card_back_mesh_id = 1306     # Our T_CardBackMesh texture

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Checking: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            # Look for MeshRenderer components
            for obj in env.objects:
                try:
                    if obj.type.name == "MeshRenderer":
                        data = obj.read()

                        # Check materials
                        if hasattr(data, 'm_Materials'):
                            for material_ref in data.m_Materials:
                                if hasattr(material_ref, 'path_id'):
                                    print(f"MeshRenderer (ID: {obj.path_id}) uses Material (ID: {material_ref.path_id})")

                    elif obj.type.name == "Material":
                        data = obj.read()
                        name = getattr(data, 'name', f'material_{obj.path_id}')

                        # Check if material uses our target textures
                        if hasattr(data, 'm_SavedProperties'):
                            props = data.m_SavedProperties
                            if hasattr(props, 'm_TexEnvs'):
                                for tex_env in props.m_TexEnvs:
                                    if hasattr(tex_env, 'second') and hasattr(tex_env.second, 'm_Texture'):
                                        tex_ref = tex_env.second.m_Texture
                                        if hasattr(tex_ref, 'path_id'):
                                            tex_id = tex_ref.path_id
                                            if tex_id in [graded_case_texture_id, card_back_mesh_id]:
                                                tex_name = "GradedCardCase" if tex_id == graded_case_texture_id else "T_CardBackMesh"
                                                prop_name = getattr(tex_env, 'first', 'unknown')
                                                print(f"*** MATERIAL USES TARGET TEXTURE: {name} (ID: {obj.path_id}) ***")
                                                print(f"    Texture: {tex_name} (ID: {tex_id}) as property '{prop_name}'")

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    card_meshes, all_meshes = find_mesh_assets(game_path)
    find_mesh_renderers_and_materials(game_path)

if __name__ == "__main__":
    main()