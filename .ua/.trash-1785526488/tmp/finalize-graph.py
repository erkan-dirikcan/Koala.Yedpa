#!/usr/bin/env python3
"""Finalize the knowledge graph - layers, tour, validation"""
import json, os
from pathlib import Path
from datetime import datetime, timezone

PROJECT_ROOT = r'D:\cSource\repos\Koala.Yedpa_cursor'
UA_DIR = os.path.join(PROJECT_ROOT, '.ua')

with open(os.path.join(UA_DIR, 'intermediate', 'assembled-graph.json'), 'r', encoding='utf-8') as f:
    graph = json.load(f)

nodes = graph['nodes']
node_ids = {n['id'] for n in nodes}

# ===== LAYERS (exclusive - first match wins) =====
layer_defs = [
    ('cozum-kok', 'Cozum Kok Dosyalari', 'Solution (.sln), proje talimatlari (CLAUDE.md) ve kok duzey dokumantasyon.',
     lambda fp: any(x in fp for x in ['Koala.Yedpa.sln', 'CLAUDE.md', 'handoff.md', 'KoalaYedpa_Api_Dokumantasyon',
                                        'KoalaYedpa_Smmury_v01.md', 'LogoClCardAPI_Documentation.md', 'Todo.md'])),

    ('claude-ajanlar', 'Claude Code Ajan Tanimlari', 'Claude Code takim ajani tanimlari.',
     lambda fp: '.claude/agents/' in fp),

    ('claude-skills', 'Claude Code Yetenekler', 'Claude Code skill tanimlari ve sablonlari.',
     lambda fp: '.claude/skills/' in fp),

    ('dotnet-config', '.NET Yapilandirma', 'appsettings.json, .csproj, dotnet-tools.json ve publish profilleri.',
     lambda fp: any(x in fp for x in ['appsettings.json', 'dotnet-tools.json', '.csproj', '.pubxml',
                                        '.claude/settings', '.editorconfig', '.gitignore', 'Properties/PublishProfiles'])),

    ('ci-cd-config', 'CI/CD ve Harici Yapilandirma', 'Message34, n8n is akislari, code-workspace.',
     lambda fp: any(x in fp for x in ['.Message34/', 'docs/n8n/', '.code-workspace'])),

    ('dotnet-core', '.NET Core Katmani', 'Entity, DTO, Interface, Enum. Projenin temel domain katmani.',
     lambda fp: 'Koala.Yedpa.Core/' in fp and '.csproj' not in fp),

    ('veritabani-repo', 'Veritabani ve Repository Katmani', 'EF Core DbContext, repository, migration.',
     lambda fp: 'Koala.Yedpa.Repositories/' in fp and 'Tests' not in fp and '.csproj' not in fp),

    ('servis-katmani', 'Servis Katmani', 'Is mantigi servisleri, DTO donusumleri, hesaplamalar.',
     lambda fp: 'Koala.Yedpa.Service/' in fp and 'Tests' not in fp and '.csproj' not in fp),

    ('webapi', 'Web API Katmani', 'REST API endpointleri, harici sistem entegrasyonu.',
     lambda fp: 'Koala.Yedpa.WebApi/' in fp and '.csproj' not in fp),

    ('webui-controller', 'Web Arayuz - Controller', 'ASP.NET Core MVC controllerlari.',
     lambda fp: 'Koala.Yedpa.WebUI/Controllers/' in fp),

    ('webui-views', 'Web Arayuz - Viewlar', 'Razor View (.cshtml) dosyalari.',
     lambda fp: 'Koala.Yedpa.WebUI/Views/' in fp),

    ('webui-js', 'Web Arayuz - JavaScript', 'Sayfaya ozel JavaScript modulleri.',
     lambda fp: ('/js/' in fp or '/assets/js/' in fp) and fp.endswith('.js')),

    ('webui-css', 'Web Arayuz - CSS', 'Metronic 7 tema ve ozel CSS dosyalari.',
     lambda fp: ('/css/' in fp or '/assets/css/' in fp) and fp.endswith('.css')),

    ('webui-statik', 'Web Arayuz - Statik Varliklar', 'Font, resim, medya ve diger statik dosyalar.',
     lambda fp: ('/wwwroot/' in fp) and not any(ext in fp for ext in ['.js', '.css', '.cshtml']) and 'Controllers/' not in fp),

    ('webui-diger', 'Web Arayuz - Diger', 'WebUI altindaki Models, Helpers, Extensions gibi yardimci dosyalar.',
     lambda fp: 'Koala.Yedpa.WebUI/' in fp and '/wwwroot/' not in fp and '/Controllers/' not in fp and '/Views/' not in fp),

    ('test-katmani', 'Test Katmani', 'Unit ve entegrasyon testleri.',
     lambda fp: 'Tests' in fp or 'Test' in fp.replace('Tests.cs', '')),

    ('dokuman-diger', 'Diger Dokumantasyon', 'Markdown dokumanlar ve referans belgeler.',
     lambda fp: fp.endswith('.md') or fp.endswith('.txt')),

    ('sql-sorgular', 'SQL Sorgulari', 'Veritabani kontrol ve sorgu scriptleri.',
     lambda fp: fp.endswith('.sql')),

    ('coverage', 'Kod Kapsama Raporlari', 'Test coverage HTML raporlari.',
     lambda fp: 'CoverageReport/' in fp),

    ('diger', 'Diger Dosyalar', 'Yukaridaki kategorilere girmeyen dosyalar.',
     lambda fp: True),  # catch-all
]

# Assign each node to exactly one layer (first match)
layers_map = {ld[0]: {'id': f'layer:{ld[0]}', 'name': ld[1], 'description': ld[2], 'nodeIds': []}
              for ld in layer_defs}

for n in nodes:
    fp = n.get('filePath', '')
    for lkey, lname, ldesc, lfunc in layer_defs:
        if lfunc(fp):
            layers_map[lkey]['nodeIds'].append(n['id'])
            break

layers = [v for v in layers_map.values() if v['nodeIds']]

# Check coverage
assigned = set()
for l in layers:
    assigned.update(l['nodeIds'])
unassigned = [n['id'] for n in nodes if n['id'] not in assigned]
print(f'Layers: {len(layers)}')
for l in layers:
    print(f'  {l["name"]}: {len(l["nodeIds"])} dosya')
print(f'Unassigned: {len(unassigned)}')

# ===== TOUR =====
tour = [
    {
        'order': 1,
        'title': 'Projeye Genel Bakis',
        'description': 'Cozum dosyasi ve proje talimatlariyla baslayarak Koala.Yedpa yaziliminin mimarisini ve amacini anlayin.',
        'nodeIds': ['file:CLAUDE.md', 'config:Koala.Yedpa.sln']
    },
    {
        'order': 2,
        'title': 'Domain Katmani (.NET Core)',
        'description': 'Entity, DTO ve Interface tanimlarini iceren temel katman. Is mantigi bu katman uzerine insa edilir.',
        'nodeIds': [n['id'] for n in nodes if 'Koala.Yedpa.Core/' in n['filePath'] and 'Entities' in n['filePath']][:5]
    },
    {
        'order': 3,
        'title': 'Veritabani ve Repository Katmani',
        'description': 'EF Core DbContext, repository implementasyonlari ve veritabani migrationlari. Veri erisim mantigi burada toplanir.',
        'nodeIds': [n['id'] for n in nodes if 'Koala.Yedpa.Repositories/' in n['filePath'] and 'AppDbContext' in n['filePath']][:3]
    },
    {
        'order': 4,
        'title': 'Servis Katmani',
        'description': 'Is mantigi servisleri, hesaplamalar, DTO donusumleri ve harici API entegrasyonlari. Uygulamanin ana is mantigi bu katmanda calisir.',
        'nodeIds': [n['id'] for n in nodes if 'Koala.Yedpa.Service/' in n['filePath'] and 'Service' in n['filePath'] and 'I' not in Path(n['filePath']).stem][:5]
    },
    {
        'order': 5,
        'title': 'Uygulama Baslangic Noktasi',
        'description': 'ASP.NET Core uygulamasinin baslangic noktasi. DI konteyner, middleware pipeline ve servis kayitlari burada yapilandirilir.',
        'nodeIds': ['file:Koala.Yedpa.WebUI/Program.cs']
    },
    {
        'order': 6,
        'title': 'MVC Controllerlar',
        'description': 'Kullanici isteklerini karsilayan ve viewlari yonlendiren controller siniflari.',
        'nodeIds': [n['id'] for n in nodes if '/Controllers/' in n['filePath'] and n['language'] == 'csharp'][:5]
    },
    {
        'order': 7,
        'title': 'Kullanici Arayuzu - Viewlar',
        'description': 'Metronic 7 temasiyla olusturulmus Razor Viewlar. Bootstrap 4 tabanli responsive tasarim ve DataTables entegrasyonu.',
        'nodeIds': [n['id'] for n in nodes if '/Views/' in n['filePath'] and n['filePath'].endswith('.cshtml')][:5]
    },
    {
        'order': 8,
        'title': 'JavaScript ve Client-Side Mantik',
        'description': 'Sayfaya ozel JavaScript modulleri, AJAX cagrilari, DataTables ve Chart.js yapilandirmalari.',
        'nodeIds': [n['id'] for n in nodes if n['language'] == 'javascript' and '/custom/' in n['filePath']][:5]
    },
    {
        'order': 9,
        'title': 'Web API ve Harici Entegrasyon',
        'description': 'REST API endpointleri ve Logo/Message34 gibi harici sistem entegrasyon noktalari.',
        'nodeIds': [n['id'] for n in nodes if 'Koala.Yedpa.WebApi/' in n['filePath'] and n['language'] == 'csharp'][:5]
    },
    {
        'order': 10,
        'title': 'Test Katmani',
        'description': 'Unit test ve entegrasyon testleri. Is mantiginin dogrulugu bu testlerle guvence altina alinir.',
        'nodeIds': [n['id'] for n in nodes if 'Tests' in n['filePath'] or 'Test' in n['filePath']][:5]
    },
    {
        'order': 11,
        'title': 'Dokumantasyon ve Is Akislari',
        'description': 'n8n is akisi dokumantasyonlari, API referanslari ve gelistirme notlari.',
        'nodeIds': [n['id'] for n in nodes if n['type'] == 'document' and ('docs/' in n['filePath'] or n['filePath'].endswith('.md'))][:5]
    }
]

# Filter steps with empty nodeIds
tour = [t for t in tour if t['nodeIds']]

print(f'\nTour: {len(tour)} steps')

# ===== FINAL GRAPH =====
final_graph = {
    'version': '1.0.0',
    'project': graph.get('project', {
        'name': 'Koala.Yedpa',
        'languages': ['csharp', 'javascript', 'html', 'css', 'sql', 'json', 'xml', 'markdown'],
        'frameworks': ['ASP.NET Core 10.0', 'Entity Framework Core', 'Metronic 7', 'Bootstrap 4', 'Hangfire', 'RabbitMQ', 'N8N'],
        'description': 'Kurumsal yonetim yazilimi',
        'analyzedAt': datetime.now(timezone.utc).isoformat(),
        'gitCommitHash': 'da90f9c705ee72cd4f5aac673d1f5e273b395773'
    }),
    'nodes': nodes,
    'edges': graph.get('edges', []),
    'layers': layers,
    'tour': tour
}

output_path = os.path.join(UA_DIR, 'knowledge-graph.json')
with open(output_path, 'w', encoding='utf-8') as f:
    json.dump(final_graph, f, ensure_ascii=False, indent=2)

print(f'\nSaved final graph to {output_path}')
print(f'Stats: {len(nodes)} nodes, {len(graph.get("edges",[]))} edges, {len(layers)} layers, {len(tour)} tour steps')
