#!/usr/bin/env python3
"""Generate architectural layers for Koala.Yedpa"""
import json, os
from pathlib import Path

PROJECT_ROOT = r'D:\cSource\repos\Koala.Yedpa_cursor'
UA_DIR = os.path.join(PROJECT_ROOT, '.ua')

with open(os.path.join(UA_DIR, 'intermediate', 'assembled-graph.json'), 'r', encoding='utf-8') as f:
    graph = json.load(f)

nodes = graph['nodes']

# Define layers based on project structure
layers = []

def layer(id_suffix, name, description, path_patterns):
    node_ids = []
    for n in nodes:
        fp = n.get('filePath', '')
        for pattern in path_patterns:
            if pattern in fp:
                node_ids.append(n['id'])
                break
    return {
        'id': f'layer:{id_suffix}',
        'name': name,
        'description': description,
        'nodeIds': node_ids
    }

layers = [
    layer('cozum-kok-dosyalari', 'Cozum Kok Dosyalari',
          'Solution (.sln), proje talimatlari (CLAUDE.md), yapilandirma ve dokumantasyon dosyalari.',
          ['Koala.Yedpa.sln', 'CLAUDE.md', 'handoff.md', '.editorconfig', '.gitignore', 'KoalaYedpa_Api_Dokumantasyon', 'check_', 'build_output']),

    layer('claude-code-ajanlar', 'Claude Code Ajan Tanimlari',
          'Claude Code takim ajani tanimlari ve davranis kurallari (.claude/agents/).',
          ['.claude/agents/']),

    layer('claude-code-yetenekler', 'Claude Code Yetenekler (Skills)',
          'Claude Code skill tanimlari ve sablonlari (.claude/skills/).',
          ['.claude/skills/']),

    layer('dotnet-core-katmani', '.NET Core Katmani',
          'Entity, DTO, Interface, Enum ve temel is mantigi tanimlari. Projenin en temel katmani.',
          ['Koala.Yedpa.Core/']),

    layer('veritabani-katmani', 'Veritabani ve Repository Katmani',
          'EF Core DbContext, repository implementasyonlari, migration dosyalari.',
          ['Koala.Yedpa.Repositories/']),

    layer('servis-katmani', 'Servis Katmani',
          'Is mantigi servisleri, DTO donusumleri, AutoMapper, hesaplamalar ve harici API entegrasyonlari.',
          ['Koala.Yedpa.Service/']),

    layer('webapi-katmani', 'Web API Katmani',
          'REST API endpointleri, JSON serializasyonu ve harici sistem entegrasyon noktalari.',
          ['Koala.Yedpa.WebApi/']),

    layer('webui-katmani', 'Web Arayuz Katmani (MVC)',
          'ASP.NET Core MVC controllerlari, Razor Viewlar, Metronic 7 temasi ve Bootstrap 4 tabanli kullanici arayuzu.',
          ['Koala.Yedpa.WebUI/']),

    layer('test-katmani', 'Test Katmani',
          'Unit test ve entegrasyon testleri (xUnit/NUnit).',
          ['Koala.Yedpa.Service.Tests/', 'Koala.Yedpa.Repositories.Tests/']),

    layer('dokumantasyon-dosyalari', 'Dokumantasyon Dosyalari',
          'n8n is akisi dokumantasyonlari ve diger referans belgeler.',
          ['docs/']),

    layer('coverage-raporlari', 'Kod Kapsama Raporlari',
          'Test coverage HTML raporlari ve ilgili dosyalar.',
          ['CoverageReport/']),

    layer('message34-entegrasyon', 'Message34 Entegrasyonu',
          'Message34 e-posta servisi yapilandirmasi.',
          ['.Message34/']),

    layer('dotnet-yapilandirma', '.NET Yapilandirma Dosyalari',
          'appsettings.json, dotnet-tools.json, publish profilleri ve proje (.csproj) dosyalari.',
          ['appsettings.json', 'dotnet-tools.json', '.csproj', 'Properties/PublishProfiles', '.pubxml']),

    layer('statik-varliklar', 'Statik Web Varliklari',
          'CSS, JavaScript, resim ve font dosyalari (wwwroot altinda).',
          ['/wwwroot/']),
]

# Count coverage
all_assigned = set()
for layer_def in layers:
    all_assigned.update(layer_def['nodeIds'])

unassigned = [n['id'] for n in nodes if n['id'] not in all_assigned]
if unassigned:
    print(f'WARNING: {len(unassigned)} nodes not assigned to any layer')
    for uid in unassigned[:20]:
        print(f'  {uid}')

# Validate no duplicates across layers
from collections import Counter
all_ids = []
for l in layers:
    all_ids.extend(l['nodeIds'])
dupes = [id for id, count in Counter(all_ids).items() if count > 1]
if dupes:
    print(f'WARNING: {len(dupes)} nodes assigned to multiple layers')

output_path = os.path.join(UA_DIR, 'intermediate', 'layers.json')
with open(output_path, 'w', encoding='utf-8') as f:
    json.dump(layers, f, ensure_ascii=False, indent=2)

print(f'Saved {len(layers)} layers to {output_path}')
for l in layers:
    print(f'  {l["name"]}: {len(l["nodeIds"])} dosya')
print(f'Unassigned: {len(unassigned)}')
