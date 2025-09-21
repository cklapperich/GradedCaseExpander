#!/usr/bin/env python3
"""
Analyze GameObject hierarchies, meshes, and relationships to graded card assets
"""

import UnityPy
from pathlib import Path

def analyze_graded_card_objects(game_path):
    """Analyze GameObjects, Meshes, and other objects related to graded cards"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    asset_files = ["sharedassets0.assets", "sharedassets1.assets", "resources.assets"]

    print("=== SEARCHING FOR GRADED CARD RELATED OBJECTS ===\n")

    all_findings = {
        'gameobjects': [],
        'meshes': [],
        'materials': [],
        'scriptable_objects': [],
        'other_components': []
    }

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Analyzing: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    obj_type = obj.type.name

                    # Skip textures and sprites - we already found those
                    if obj_type in ["Texture2D", "Sprite"]:
                        continue

                    data = obj.read()
                    name = getattr(data, 'name', '') or getattr(data, 'm_Name', '') or f'unnamed_{obj.path_id}'

                    # Search for card/graded/case related objects
                    search_terms = ['card', 'graded', 'case', 'grade', 'ui']
                    name_lower = name.lower()

                    if any(term in name_lower for term in search_terms):
                        finding = {
                            'name': name,
                            'type': obj_type,
                            'path_id': obj.path_id,
                            'file': asset_file
                        }

                        if obj_type == "GameObject":
                            # Try to get component information
                            components = []
                            if hasattr(data, 'm_Component'):
                                for component in data.m_Component:
                                    if hasattr(component, 'component'):
                                        comp_ref = component.component
                                        if hasattr(comp_ref, 'path_id'):
                                            components.append(comp_ref.path_id)
                            finding['components'] = components
                            all_findings['gameobjects'].append(finding)

                        elif obj_type == "Mesh":
                            # Get mesh details
                            if hasattr(data, 'm_VertexCount'):
                                finding['vertex_count'] = data.m_VertexCount
                            all_findings['meshes'].append(finding)

                        elif obj_type == "Material":
                            # Get material properties
                            if hasattr(data, 'm_SavedProperties'):
                                # Try to find texture references
                                textures = []
                                props = data.m_SavedProperties
                                if hasattr(props, 'm_TexEnvs'):
                                    for tex_env in props.m_TexEnvs:
                                        if hasattr(tex_env, 'second') and hasattr(tex_env.second, 'm_Texture'):
                                            tex_ref = tex_env.second.m_Texture
                                            if hasattr(tex_ref, 'path_id'):
                                                textures.append(tex_ref.path_id)
                                finding['textures'] = textures
                            all_findings['materials'].append(finding)

                        elif "ScriptableObject" in obj_type:
                            all_findings['scriptable_objects'].append(finding)

                        else:
                            all_findings['other_components'].append(finding)

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    return all_findings

def analyze_specific_texture_references(game_path):
    """Find what objects reference our specific target textures"""
    data_dir = Path(game_path) / "Card Shop Simulator_Data"

    # Our target texture IDs
    graded_case_texture_id = 133  # sharedassets0.assets
    card_back_mesh_id = 1306     # sharedassets1.assets

    print("=== SEARCHING FOR REFERENCES TO TARGET TEXTURES ===\n")

    references = {
        'graded_case_refs': [],
        'card_back_mesh_refs': []
    }

    asset_files = ["sharedassets0.assets", "sharedassets1.assets"]

    for asset_file in asset_files:
        asset_path = data_dir / asset_file
        if not asset_path.exists():
            continue

        print(f"Checking references in: {asset_file}")

        try:
            env = UnityPy.load(str(asset_path))

            for obj in env.objects:
                try:
                    data = obj.read()

                    # Check if this object references our target textures
                    if hasattr(data, 'm_Texture') and hasattr(data.m_Texture, 'path_id'):
                        tex_id = data.m_Texture.path_id
                        if tex_id == graded_case_texture_id:
                            name = getattr(data, 'name', f'unnamed_{obj.path_id}')
                            references['graded_case_refs'].append({
                                'name': name,
                                'type': obj.type.name,
                                'path_id': obj.path_id,
                                'file': asset_file
                            })
                        elif tex_id == card_back_mesh_id:
                            name = getattr(data, 'name', f'unnamed_{obj.path_id}')
                            references['card_back_mesh_refs'].append({
                                'name': name,
                                'type': obj.type.name,
                                'path_id': obj.path_id,
                                'file': asset_file
                            })

                    # Check materials for texture references
                    if obj.type.name == "Material" and hasattr(data, 'm_SavedProperties'):
                        props = data.m_SavedProperties
                        if hasattr(props, 'm_TexEnvs'):
                            for tex_env in props.m_TexEnvs:
                                if hasattr(tex_env, 'second') and hasattr(tex_env.second, 'm_Texture'):
                                    tex_ref = tex_env.second.m_Texture
                                    if hasattr(tex_ref, 'path_id'):
                                        tex_id = tex_ref.path_id
                                        if tex_id in [graded_case_texture_id, card_back_mesh_id]:
                                            name = getattr(data, 'name', f'material_{obj.path_id}')
                                            target = 'graded_case_refs' if tex_id == graded_case_texture_id else 'card_back_mesh_refs'
                                            references[target].append({
                                                'name': name,
                                                'type': obj.type.name,
                                                'path_id': obj.path_id,
                                                'file': asset_file,
                                                'texture_property': getattr(tex_env, 'first', 'unknown')
                                            })

                except Exception as e:
                    continue

        except Exception as e:
            print(f"Error reading {asset_file}: {e}")

    return references

def main():
    game_path = "/home/klappec/.steam/debian-installation/steamapps/common/TCG Card Shop Simulator/"

    # Analyze all graded card related objects
    findings = analyze_graded_card_objects(game_path)

    print("\n=== ANALYSIS RESULTS ===\n")

    for category, items in findings.items():
        if items:
            print(f"{category.upper()} ({len(items)}):")
            for item in items:
                extra_info = ""
                if 'components' in item:
                    extra_info += f" [components: {len(item['components'])}]"
                if 'vertex_count' in item:
                    extra_info += f" [vertices: {item['vertex_count']}]"
                if 'textures' in item:
                    extra_info += f" [textures: {item['textures']}]"

                print(f"  - {item['name']} ({item['type']}) ID:{item['path_id']} from {item['file']}{extra_info}")
            print()

    # Analyze specific texture references
    references = analyze_specific_texture_references(game_path)

    print("=== TEXTURE REFERENCE ANALYSIS ===\n")

    if references['graded_case_refs']:
        print("Objects referencing GradedCardCase texture:")
        for ref in references['graded_case_refs']:
            extra = f" [property: {ref['texture_property']}]" if 'texture_property' in ref else ""
            print(f"  - {ref['name']} ({ref['type']}) ID:{ref['path_id']} from {ref['file']}{extra}")
        print()

    if references['card_back_mesh_refs']:
        print("Objects referencing T_CardBackMesh texture:")
        for ref in references['card_back_mesh_refs']:
            extra = f" [property: {ref['texture_property']}]" if 'texture_property' in ref else ""
            print(f"  - {ref['name']} ({ref['type']}) ID:{ref['path_id']} from {ref['file']}{extra}")
        print()

if __name__ == "__main__":
    main()