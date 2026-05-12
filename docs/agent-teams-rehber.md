# Agent Teams Rehberi

> Kaynak: [Claude Code Docs - Agent Teams](https://code.claude.com/docs/en/agent-teams)
> Oluşturulma: 2026-05-11

---

## Nedir?

Agent Teams, birden fazla Claude Code oturumunun koordineli çalışmasını sağlar. Bir oturum **team lead** (takım lideri) olarak görev alır, diğerleri **teammate** (takım arkadaşı) olarak bağımsız çalışır. Subagent'lardan farkı: teammate'ler birbirleriyle doğrudan haberleşebilir, sadece lead'e rapor vermekle sınırlı değildir.

---

## Ne Zaman Kullanılır?

### Iyi Kullanim Senaryolari

| Senaryo | Neden Etkili? |
|---------|---------------|
| **Arastirma ve inceleme** | Farkli teammate'ler ayni problemin farkli yonlerini ayni anda inceleyip bulgularini paylasabilir |
| **Yeni modul/ozellik gelistirme** | Her teammate ayri bir parcanin sahibi olabilir, birbirine karismaz |
| **Rekabetci hipotezlerle debugging** | Farkli teoriler paralel test edilir, sonuca daha hizli ulasilir |
| **Cross-layer koordinasyon** | Frontend, backend ve test degisiklikleri farkli teammate'lerce yurutulur |

### Kullanilmamasi Gereken Durumlar

- Sıralı (sequential) gorevler
- Ayni dosyada duzenleme gerektiren isler
- Birbirine bagimli gorevler
- Rutin isler (tek oturum daha maliyet etkin)

---

## Subagent vs Agent Teams Karsilastirmasi

| Ozellik | Subagent | Agent Teams |
|---------|----------|-------------|
| **Baglam** | Kendi penceresi; sonuclar cagirana doner | Kendi penceresi; tam bagimsiz |
| **Iletisim** | Sadece ana agent'a rapor verir | Teammate'ler birbirine direkt mesaj atar |
| **Koordinasyon** | Ana agent tum isi yonetir | Paylasilmis gorev listesi, kendi kendini koordine |
| **En iyi** | Sadece sonucun onemli oldugu odakli isler | Tartisma ve isbirligi gerektiren karmasik isler |
| **Token maliyeti** | Dusuk | Yuksek (her teammate ayri Claude oturumu) |

---

## Aktiflestirme

`settings.json` veya ortam degiskeni ile:

```json
{
  "env": {
    "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS": "1"
  }
}
```

---

## Mimari

| Bilesen | Rol |
|---------|-----|
| **Team Lead** | Takimi olusturan, teammate'leri spawn eden, isi koordine eden ana oturum |
| **Teammates** | Atanan gorevlerde calisan ayri Claude Code ornekleri |
| **Task List** | Tum teammate'lerin gorebildigi, claim edip tamamladigi paylasimli gorev listesi |
| **Mailbox** | Agent'lar arasi mesajlasma sistemi |

### Dosya Konumlari

- Takim konfigürasyonu: `~/.claude/teams/{team-name}/config.json`
- Gorev listesi: `~/.claude/tasks/{team-name}/`

> **Onemli:** `config.json` dosyasini elle duzenlemeyin. Claude Code otomatik gunceller.

---

## Gorunum Modlari (Display Mode)

| Mod | Aciklama |
|-----|----------|
| **in-process** | Tum teammate'ler ana terminalde calisir. `Shift+Down` ile gecebilirsiniz. Her terminalde calisir. |
| **split-panes** | Her teammate ayri pane'e sahip. tmux veya iTerm2 gerektirir. |

**Varsayilan:** `"auto"` — tmux icindeyseniz split-panes, degilse in-process kullanir.

### Ayarlama

```json
// ~/.claude/settings.json
{
  "teammateMode": "in-process"
}
```

Tek oturum icin flag:
```bash
claude --teammate-mode in-process
```

### Split-pane Gereksinimleri

- **tmux:** Paket yoneticisiyle kurun
- **iTerm2:** `it2` CLI kurun + iTerm2 > Settings > General > Magic > Enable Python API

---

## Takim Olusturma Ornekleri

### Temel Ornek

```
CLI tool tasarliyorum, TODO comment'larini takip edecek. Bir agent team olustur:
- biri UX acisindan
- biri teknik mimari acisindan
- biri seytan savunucu (devil's advocate) olarak
```

### Kod Inceleme

```
PR #142'yi incelemek icin bir agent team olustur. 3 reviewer spawn et:
- biri guvenlik aciklari
- biri performans etkisi
- biri test coverage
```

### Rekabetci Hipotez Debugging

```
Kullanicilar uygulamanin bir mesajdan sonra kapandigini bildiriyor.
5 teammate spawn et, farkli hipotezleri arastirsin. Birbirlerinin
teorilerini cermezeye calissinlar, bilimsel bir tartisma gibi.
```

### Model ve Sayi Belirtme

```
4 teammate ile bir takim olustur, bu modulleri paralel refactor et.
Her teammate icin Sonnet kullan.
```

---

## Takim Kontrolu

### Gorev Atama ve Claim Etme

Gorevler uc duruma sahip: `pending`, `in_progress`, `completed`. Gorevler bagimliliklara sahip olabilir — bagimli gorevler, on bagimli gorevler tamamlanana kadar claim edilemez.

- **Lead atar:** "X gorevini Y teammate'e ver"
- **Kendiliginden claim:** Teammate gorev bitince siradaki atanmamis gorevu otomatik alir

> Dosya kilitleme (file locking) ile birden fazla teammate'in ayni gorevu claim etmesi onlenir.

### Plan Onayi Zorunlulugu

```
Auth modulunu refactor etmek icin bir mimar teammate spawn et.
Herhangi bir degisiklik yapmadan once plan onayi zorunlu olsun.
```

Teammeat plan modunda calisir, plan onay istegi gonderir. Lead onaylarsa implementasyona gecer, reddederse teammate plani revize eder.

### Dogrudan Teammate Iletisimi

- **in-process:** `Shift+Down` ile teammate'ler arasi gezin, yazarak mesaj gonderin
- **split-panes:** Pane'e tiklayarak dogrudan etkilesim kurun
- `Ctrl+T` ile gorev listesini goruntuleyin

### Teammate Kapatma

```
Arastirmaci teammate'e kapanmasini soyle
```

Lead shutdown request gonderir. Teammate onaylayip cikabilir veya reddedip aciklama yapabilir.

### Takim Temizligi

Is bitince lead'e temizleme talimati verin. Aktif teammate varsa temizleme basarisiz olur — once teammate'leri kapatın.

---

## Hooks ile Kalite Kontrolu

| Hook | Tetikleyici | Kullanim |
|------|-------------|----------|
| `TeammateIdle` | Teammate idle'a gececekken | Exit code 2 ile geri bildirim gonderip teammate'i calismaya devam ettir |
| `TaskCreated` | Gorev olusturulurken | Exit code 2 ile olusturmayi engelle |
| `TaskCompleted` | Gorev tamamlanirken | Exit code 2 ile tamamlamayi engelle |

---

## Subagent Tanimlarini Teammate Olarak Kullanma

`.claude/agents/` altinda tanimlanan subagent turlerini teammate olarak kullanabilirsiniz:

```
security-reviewer agent tipini kullanarak bir teammate spawn et, auth modulunu denetlesin.
```

Teammate, subagent taniminin `tools` ve `model` ayarlarina uyar. `SendMessage` ve gorev yonetim araclari her zaman kullanilabilir.

---

## En Iyi Pratikler

### 1. Teammate'lere Yeterli Baglam Verin

Teammate'ler lead'in konusma gecmisini almaz. Spawn prompt'unda gorefe ozel detaylari verin:

```
Bir guvenlik incelemesi teammate spawn et: "src/auth/ modulunu guvenlik aciklari icin incele.
Token yonetimi, oturum yonetimi ve input validation'a odaklan. Uygulama JWT token
kullaniyor, httpOnly cookie'lerde sakliyor. Sorunlari severity rating ile raporla."
```

### 2. Uygun Takim Buyuklugu Secin

| Durum | Oneri |
|-------|-------|
| Genel isler | 3-5 teammate |
| Her teammate icin | 5-6 gorev (verimli calisma, asiri context switch yok) |
| 15 bagimsiz gorev | 3 teammate baslangic icin iyi |

> **Kural:** 3 odaklanmis teammate, 5 daginik teammate'ten daha iyi performans gosterir.

### 3. Gorev Boyutunu Dogru Ayarlayın

- **Cok kucuk:** Koordinasyon maliyeti faydadan fazla
- **Cok buyuk:** Check-in olmadan uzun sure calisma, bosuna harcanan emek riski
- **Tam boyut:** Net deliverable ureten, kendi basina yeten birimler (bir fonksiyon, bir test dosyasi, bir inceleme)

### 4. Dosya Cakismasindan Kacinin

Iki teammate ayni dosyayi duzenlememeli. Her teammate farkli dosya setine sahip olmali.

### 5. Izleyin ve Yonlendirin

Teammate'lerin ilerlemesini duzenli kontrol edin, calismayan yaklasimlari yonlendirin. Takimi uzun sure gozetimsiz birakmak bos emek riskini artirir.

### 6. Arastirma ve Inceleme ile Baslayin

Agent Teams'e yeniyseniz, kod yazma gerektirmeyen gorevlerle baslayın: PR inceleme, kutuphane arastirmasi, bug arastirmasi.

---

## Sorun Giderme

| Sorun | Cozum |
|-------|-------|
| Teammate'ler gorunmuyor | `Shift+Down` ile gezin. Gorevin takim gerektirecek kadar karmasik oldugundan emin olun. |
| Cok fazla izin prompt'u | Yaygin islemleri permission settings'te onaylayin |
| Teammate hata verip duruyor | Teammate ciktilarini kontrol edin, direkt talimat verin veya yerine yeni teammate spawn edin |
| Lead is bitmeden kapaniyor | Lead'e devam etmesini soyleyin |
| Teammate gorev tamamlamiyor | Gorev durumunu manuel guncelleyin veya lead'e teammate'i uyarmasini soyleyin |
| Ortada kalan tmux oturumu | `tmux ls` + `tmux kill-session -t <isim>` |

---

## Sinirlamalar (Deneysel Ozellik)

- **in-process teammate'lerde session resume yok:** `/resume` ve `/rewind` teammate'leri geri yuklemez
- **Gorev durumu gecikebilir:** Teammate'ler bazen gorevleri `completed` isaretlemeyi unutur
- **Kapatma yavas olabilir:** Teammate mevcut istegi/bitir arac cagrisini bitirmeli
- **Ayni anda sadece bir takim:** Lead sadece bir takim yonetebilir
- **Ic ice takim yok:** Teammate'ler kendi takimlarini/teammate'lerini spawn edemez
- **Lead sabit:** Lead degistirilemez, devredilemez
- **Izinler spawn'da belirlenir:** Her teammate lead'in izin moduyla baslar
- **Split-panes tmux/iTerm2 gerektirir:** VS Code terminal, Windows Terminal veya Ghostty desteklenmez

---

## Hizli Referans - Komutlar

| Islem | Komut/Yol |
|-------|-----------|
| Teammate'ler arasi gezin | `Shift+Down` |
| Gorev listesini goster | `Ctrl+T` |
| Teammate oturumunu goruntule | `Enter` (in-process) |
| Teammate'i kes | `Escape` |
| Tek oturum icin mod sec | `claude --teammate-mode in-process` |
| tmux oturumlarini listele | `tmux ls` |
| tmux oturumunu kapat | `tmux kill-session -t <isim>` |

---

## Ilgili Yaklasimlar

- **Hafif delegation:** Subagent'lar oturum icinde arastirma/dogrulama icin spawn edilir, agent-arasi koordinasyon gerektirmez
- **Manuel paralel oturumlar:** Git worktree'ler ile birden fazla Claude Code oturumunu kendiniz yonetebilirsiniz
