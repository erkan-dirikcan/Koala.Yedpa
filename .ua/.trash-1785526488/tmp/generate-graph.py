#!/usr/bin/env python3
"""Deterministic knowledge graph generator for Koala.Yedpa"""
import json, os, re
from pathlib import Path

PROJECT_ROOT = r'D:\cSource\repos\Koala.Yedpa_cursor'
UA_DIR = os.path.join(PROJECT_ROOT, '.ua')

# Load scan result
with open(os.path.join(UA_DIR, 'intermediate', 'scan-result.json'), 'r', encoding='utf-8') as f:
    scan = json.load(f)

files = scan['files']

CATEGORY_TYPE_MAP = {
    'code': 'file', 'config': 'config', 'docs': 'document',
    'markup': 'file', 'data': 'file', 'script': 'file', 'infra': 'service',
}

def generate_summary(file_path, language, category):
    path_lower = file_path.lower()
    parts = Path(file_path).parts
    fname = Path(file_path).stem
    ext = Path(file_path).suffix.lower()

    if 'readme' in path_lower:
        return 'Proje dokumantasyonu ve genel bilgiler.'
    if 'claude.md' in path_lower:
        return 'Claude Code icin proje talimatlari ve yapilandirmasi.'
    if '.claude/agents/' in path_lower:
        return f'{fname.capitalize()} takim ajani tanimi ve davranis kurallari.'
    if '.claude/skills/' in path_lower and '/assets/controllers/' in path_lower:
        return f'ASP.NET Identity {fname} controller sablonu.'
    if '.claude/skills/' in path_lower and '/assets/views/' in path_lower:
        return f'ASP.NET Identity view sablonu: {fname}.'
    if '.claude/skills/' in path_lower and '/references/' in path_lower:
        ref_topics = {'entities': 'Entity yapilari', 'patterns': 'Tasarim desenleri', 'workflows': 'Is akislari'}
        topic = ref_topics.get(fname, fname)
        return f'ASP.NET Identity {topic} referans dokumani.'
    if '.claude/skills/' in path_lower and 'skill.md' in path_lower:
        return f'Claude Code yetenek tanimi: {parts[-2] if len(parts) > 1 else fname}.'
    if 'migration' in path_lower:
        return f'Veritabani migration: {fname}.'
    if '.csproj' in path_lower:
        return f'.NET proje dosyasi: {fname}.'
    if 'appsettings' in path_lower:
        return f'Uygulama yapilandirma ayarlari: {fname}.'
    if 'program.cs' in path_lower:
        return 'ASP.NET Core uygulama baslangic noktasi.'
    if 'startup' in path_lower:
        return 'Uygulama baslangic yapilandirmasi.'
    if 'controller' in path_lower:
        return f'MVC Controller: {fname}.'
    if 'dto' in path_lower or '/dtos/' in path_lower:
        return f'DTO tanimlari: {fname}.'
    if 'entity' in path_lower or '/entities/' in path_lower:
        return f'Entity tanimi: {fname}.'
    if 'interface' in path_lower or '/interfaces/' in path_lower:
        return f'Arayuz tanimi: {fname}.'
    if 'service' in path_lower:
        return f'Servis implementasyonu: {fname}.'
    if 'repository' in path_lower:
        return f'Repository implementasyonu: {fname}.'
    if 'view' in path_lower or 'cshtml' in ext:
        return f'MVC View: {fname}.'
    if '.js' in ext and '/custom/' in path_lower:
        return f'Sayfa JavaScript modulu: {fname}.'
    if '.js' in ext:
        return f'JavaScript dosyasi: {fname}.'
    if '.css' in ext:
        return f'CSS stil dosyasi: {fname}.'
    if '/tests/' in path_lower or 'test' in path_lower:
        return f'Test dosyasi: {fname}.'
    if '.sql' in ext:
        return f'SQL sorgu dosyasi: {fname}.'
    if 'docker' in path_lower:
        return f'Docker yapilandirmasi: {fname}.'
    if 'nginx' in path_lower:
        return f'Nginx yapilandirmasi: {fname}.'
    if 'handoff' in path_lower:
        return 'Gelistirme devir teslim notlari.'

    if category == 'config':
        return f'Yapilandirma dosyasi: {fname}.'
    if category == 'docs':
        return f'Dokuman dosyasi: {fname}.'
    if category == 'markup':
        return f'Arayuz dosyasi: {fname}.'
    return f'{fname} kaynak dosyasi.'


def generate_tags(file_path, language, category):
    path_lower = file_path.lower()
    tags = []
    if 'controller' in path_lower: tags.append('controller')
    if 'dto' in path_lower or '/dtos/' in path_lower: tags.append('dto')
    if 'entity' in path_lower or '/entities/' in path_lower: tags.append('entity')
    if 'service' in path_lower: tags.append('servis')
    if 'repository' in path_lower: tags.append('repository')
    if 'view' in path_lower or 'cshtml' in path_lower: tags.append('view')
    if 'migration' in path_lower: tags.append('migration')
    if 'test' in path_lower: tags.append('test')
    if 'interface' in path_lower: tags.append('arayuz')
    if 'model' in path_lower: tags.append('model')
    if 'api' in path_lower: tags.append('api')
    if 'wwwroot' in path_lower: tags.append('statik-dosya')
    if '.claude/' in path_lower: tags.append('claude-code')
    if '.sql' in path_lower: tags.append('veritabani')
    if language == 'csharp': tags.append('csharp')
    if language == 'javascript': tags.append('javascript')
    if language == 'css': tags.append('css')
    if language == 'sql': tags.append('sql')
    if 'mail' in path_lower.lower(): tags.append('eposta')
    if 'invoice' in path_lower.lower(): tags.append('fatura')
    if 'budget' in path_lower.lower(): tags.append('butce')
    if 'ariza' in path_lower.lower(): tags.append('ariza')
    if 'otopark' in path_lower.lower(): tags.append('otopark')
    if not tags:
        tags.append(category)
    return tags


def complexity(size_lines, language):
    if size_lines > 500: return 'complex'
    if size_lines > 150: return 'moderate'
    return 'simple'


def generate_name(file_path):
    path = file_path
    fname = Path(path).stem

    if path.endswith('Program.cs'):
        return 'Uygulama Baslangic Noktasi'
    elif path.endswith('Startup.cs'):
        return 'Baslangic Yapilandirmasi'
    elif path.endswith('CLAUDE.md'):
        return 'Proje Talimatlari'
    elif path.endswith('.csproj'):
        return f'{Path(path).stem} Proje Dosyasi'
    elif path.endswith('.sln'):
        return 'Cozum Dosyasi'
    elif path.endswith('appsettings.json'):
        return 'Uygulama Ayarlari'
    elif 'handoff' in path.lower():
        return 'Devir Teslim Notlari'

    # Convert filename to readable Turkish title
    title = fname.replace('-', ' ').replace('_', ' ').strip()
    return title


print(f'Processing {len(files)} files...')

nodes = []
node_ids = set()

for f in files:
    path = f['path']
    lang = f['language']
    cat = f.get('fileCategory', 'code')
    size = f.get('sizeLines', 0)

    ntype = CATEGORY_TYPE_MAP.get(cat, 'file')
    nid = f'{ntype}:{path}'

    node = {
        'id': nid,
        'type': ntype,
        'name': generate_name(path),
        'filePath': path,
        'summary': generate_summary(path, lang, cat),
        'tags': generate_tags(path, lang, cat),
        'complexity': complexity(size, lang),
        'language': lang
    }
    nodes.append(node)
    node_ids.add(nid)

print(f'Generated {len(nodes)} nodes')

# --- Generate edges ---
edges = []
edge_set = set()
edge_counter = [0]

def add_edge(source, target, etype, weight=0.5):
    key = (source, target, etype)
    if key not in edge_set and source in node_ids and target in node_ids and source != target:
        edge_set.add(key)
        edges.append({
            'source': source,
            'target': target,
            'type': etype,
            'weight': weight
        })
        edge_counter[0] += 1

# 1. Directory-based edges
dir_groups = {}
for n in nodes:
    d = str(Path(n['filePath']).parent)
    if d not in dir_groups:
        dir_groups[d] = []
    dir_groups[d].append(n['id'])

for d, group in dir_groups.items():
    if 1 < len(group) <= 25:
        for i in range(len(group)):
            for j in range(i+1, len(group)):
                add_edge(group[i], group[j], 'related', 0.3)

print(f'After dir edges: {edge_counter[0]}')

# 2. csproj references
csproj_nodes = [n for n in nodes if n['filePath'].endswith('.csproj')]
for i in range(len(csproj_nodes)):
    for j in range(i+1, len(csproj_nodes)):
        add_edge(csproj_nodes[i]['id'], csproj_nodes[j]['id'], 'related', 0.6)

# Read actual project references from csproj files
for csproj_node in csproj_nodes:
    csproj_path = os.path.join(PROJECT_ROOT, csproj_node['filePath'])
    if os.path.exists(csproj_path):
        try:
            with open(csproj_path, 'r', encoding='utf-8') as fh:
                content = fh.read()
            refs = re.findall(r'<ProjectReference\s+Include=\"([^\"]+)\"', content)
            for ref in refs:
                ref_name = Path(ref).stem
                for n in nodes:
                    if ref_name in n['filePath'] and n['type'] in ('file', 'config'):
                        add_edge(csproj_node['id'], n['id'], 'depends_on', 0.7)
        except Exception:
            pass

print(f'After csproj edges: {edge_counter[0]}')

# 3. Test → source edges (tested_by)
test_nodes = [n for n in nodes if 'test' in str(Path(n['filePath']).parent).lower()
              or 'tests' in str(Path(n['filePath']).parent).lower()
              or 'Tests' in n['filePath']]
for test_node in test_nodes:
    test_stem = Path(test_node['filePath']).stem
    # Remove common test suffixes
    test_base = test_stem.replace('Tests', '').replace('Test', '').replace('_test', '').replace('Test', '')
    for src_node in nodes:
        src_stem = Path(src_node['filePath']).stem
        if src_node['id'] != test_node['id']:
            if test_base and src_stem and (test_base == src_stem or test_base in src_stem or src_stem in test_base):
                add_edge(test_node['id'], src_node['id'], 'tested_by', 0.5)
                break

print(f'After test edges: {edge_counter[0]}')

# 4. Controller → View edges
for n in nodes:
    path_lower = n['filePath'].lower()
    if 'controller' in path_lower and n['language'] == 'csharp':
        ctrl_name = Path(path_lower).stem.lower().replace('controller', '')
        if ctrl_name:
            for vn in nodes:
                vpath_lower = vn['filePath'].lower()
                if 'cshtml' in vpath_lower and (ctrl_name in vpath_lower or ctrl_name in str(Path(vpath_lower).parent).lower()):
                    add_edge(n['id'], vn['id'], 'serves', 0.6)

print(f'After controller edges: {edge_counter[0]}')

# 5. Docs → code edges
doc_nodes = [n for n in nodes if n['type'] == 'document' and n['filePath'].endswith('.md')]
for doc in doc_nodes:
    doc_stem = Path(doc['filePath']).stem.lower()
    if len(doc_stem) < 5:
        continue
    for src_node in nodes:
        src_path_lower = src_node['filePath'].lower()
        src_stem = Path(src_path_lower).stem.lower()
        # Check if doc mentions a project directory or filename
        if doc_stem in src_path_lower and doc['id'] != src_node['id'] and len(src_stem) > 3:
            add_edge(doc['id'], src_node['id'], 'documents', 0.5)
            break

print(f'After doc edges: {edge_counter[0]}')

# 6. Core library → consumers
core_nodes = [n for n in nodes if 'Koala.Yedpa.Core' in n['filePath']]
service_nodes = [n for n in nodes if 'Koala.Yedpa.Service' in n['filePath']]
webui_nodes = [n for n in nodes if 'Koala.Yedpa.WebUI' in n['filePath']]
webapi_nodes = [n for n in nodes if 'Koala.Yedpa.WebApi' in n['filePath']]

# Core entities → Service consumers
for core_n in core_nodes[:50]:  # Limit
    for svc_n in service_nodes[:30]:
        cname = Path(core_n['filePath']).stem
        sname = Path(svc_n['filePath']).stem
        if cname in sname:
            add_edge(svc_n['id'], core_n['id'], 'depends_on', 0.7)

print(f'After core→svc edges: {edge_counter[0]}')

# Save assembled graph
assembled = {
    'version': '1.0.0',
    'project': {
        'name': 'Koala.Yedpa',
        'languages': ['csharp', 'javascript', 'html', 'css', 'sql', 'json', 'xml', 'markdown'],
        'frameworks': ['ASP.NET Core 10.0', 'Entity Framework Core', 'Metronic 7', 'Bootstrap 4', 'Hangfire', 'RabbitMQ', 'N8N'],
        'description': 'Kurumsal yonetim yazilimi — aidat tahsilati, butce yonetimi, fatura, ariza takibi, otopark yonetimi ve finansal operasyonlar',
        'analyzedAt': '2026-07-31T12:00:00Z',
        'gitCommitHash': 'da90f9c705ee72cd4f5aac673d1f5e273b395773'
    },
    'nodes': nodes,
    'edges': edges
}

output_path = os.path.join(UA_DIR, 'intermediate', 'assembled-graph.json')
with open(output_path, 'w', encoding='utf-8') as fh:
    json.dump(assembled, fh, ensure_ascii=False, indent=2)
print(f'Saved: {output_path}')
print(f'Total: {len(nodes)} nodes, {len(edges)} edges')
