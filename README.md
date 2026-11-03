# NƏRD. — Bir masa. Min rəqabət.

Android və iOS üçün Unity 6 / C# əsasında tam 2D nərd oyunu.
Online istiqamət: Nakama authoritative TypeScript və istehsal üçün CockroachDB.
Hazırda P01.1 üzrə uzun/qısa qayda müqaviləsi və fixture nümunələri yoxlanır.
P00.3 Unity təməli qəbul edilib; fiziki Android testi açıqdır. Oynanıla bilən nərd hələ yoxdur.

## Yeni agent üçün giriş

[AGENTS.md](AGENTS.md) → [STATE.json](docs/STATE.json) → [PROGRESS.md](PROGRESS.md)
→ statusdakı cari hesabat → iki tam mənbə → aidiyyəti sənədlər.
Söhbət tarixçəsi tələb olunmur. Skill və Antigravity/Codex Gravity MCP istifadəsi qadağandır.

| Sənəd | Vahid məsuliyyət |
|---|---|
| [Final GDD](NARD_Final_GDD_v1.0.md) | Məhsulun dəyişdirilməmiş mənbə qərarları |
| [Master İcra Planı](NARD_Master_Icra_Plani_v2.md) | Partların ardıcıllığı və qəbul qapıları |
| [AGENTS](AGENTS.md) | Bütün agentlər üçün icra müqaviləsi |
| [STATE](docs/STATE.json) | Cari part, icazə, təsdiq və açıq məsələlərin əsas statusu |
| [PROGRESS](PROGRESS.md) | İnsan üçün status xülasəsi və ötürmə nöqtəsi |
| [DECISIONS](docs/DECISIONS.md) | Mənbə ziddiyyətləri, son göstərişlər və fərziyyələr |
| [APPROVED](docs/APPROVED.md), [REJECTED](docs/REJECTED.md) | Mənbəyə uyğun qərar inventarı və ilkin part əlaqələri |
| [ARCHITECTURE](docs/ARCHITECTURE.md) | Gələcək modulların sərhədləri; implementasiya deyil |
| [TEST_MATRIX](docs/TEST_MATRIX.md) | Test qaydası və təkrar icra əmrləri |
| [OPERATIONS](docs/OPERATIONS.md) | Git, təsdiq, təhlükəsizlik, xərc və asset qaydaları |
| [Asset manifesti](docs/ASSET_MANIFEST.csv) | Asset mənbə və hüquqları; hazırda boşdur |
| [P00.1 hesabatı](docs/reports/P00.1.md) | Əvvəlki auditin məhdudiyyətləri ilə saxlanan xülasəsi |
| [P00.2 hesabatı](docs/reports/P00.2.md) | Qəbul edilmiş təməlin tarixi nəticəsi |
| [P00.3 hesabatı](docs/reports/P00.3.md) | Qəbul edilmiş Unity/Android təməlinin tarixi sübutu |
| [P01.1 hesabatı](docs/reports/P01.1.md) | Cari qayda sənədləri və real avtomatik yoxlamalar |
| [Qayda müqaviləsi](docs/RULESET_VARIANTS.md) | Ortaq koordinatlar, qayda ID-ləri və provisional variantlar |
| [Uzun qaydalar](docs/RULES_LONG.md), [qısa qaydalar](docs/RULES_SHORT.md) | Rejimlər üzrə sənəd tələbləri |
| [Qayda matrisi](docs/RULE_MATRIX.md) | JSON nümunələrinin insan üçün oxunaqlı görünüşü |
| [Hesabat şablonu](docs/reports/TEMPLATE.md) | Növbəti icazəli partda istifadə edilən format |
| [CHANGELOG](CHANGELOG.md) | Faktiki dəyişiklik tarixçəsi |

## Lokal yoxlama

Python 3.10+ və Git kifayətdir; əlavə paket, hesab və xidmət lazım deyil.
Bütün əmrlər repository kökündən icra olunur:

```bash
python3 scripts/validation/validate_project.py
python3 scripts/validation/validate_rule_fixtures.py
python3 -m unittest discover -s scripts/validation -p 'test_*.py' -v
```

Qovluq daşındıqda istinadlar nisbi qalır. Mənbə sənədləri surətlənmir və adları dəyişdirilmir.
Master Plandakı GDD.md və docs/MASTER_PLAN.md adları bu repoda yuxarıdakı orijinallara uyğundur.
Art Bible və release sənədləri yalnız müvafiq gələcək partda yaradılacaq.
P00.3-də unity/ daxilində yalnız texniki başlanğıc mövcuddur; Nakama/art modulları yaradılmır.

Repository-nin uzaq nüsxəsi və yoxlanmış vəziyyəti STATE-də `repository` sahəsindədir.
Məhsul mənbə koduna açıq mənbə lisenziyası təyin edilməyib; pulsuz alət seçimi
məhsulu avtomatik açıq mənbə etmir.

## Unity / Android yoxlamasını təkrarlamaq

Layihə unity/ qovluğundadır, Editor versiyası 6000.6.7f1-dir. Editor bağlı olanda:

```bash
python3 scripts/build/p003.py configure
python3 scripts/build/p003.py editmode
python3 scripts/build/p003.py playmode
python3 scripts/build/p003.py android
```

Runner əlavə xidmət qurmur. Başqa kompüterdə --editor ilə Editor yolu verilir;
Android build üçün uyğun rəsmi Android Build Support/SDK/NDK/OpenJDK lazımdır.
Raw loglar, NUnit XML və APK build/P00.3/ daxilində lokal saxlanır və Git-ə daxil edilmir.
Eyni layihəni açıq Editor ilə batch prosesində paralel işlətməyin.
