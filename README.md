# NƏRD. — Bir masa. Min rəqabət.

Android və iOS üçün Unity 6 / C# əsasında tam 2D nərd oyunu.
Online istiqamət: Nakama authoritative TypeScript və istehsal üçün CockroachDB.
Hazırda yalnız sənədləşmə və Git təməli qurulur; oynanıla bilən oyun yoxdur.

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
| [P00.2 hesabatı](docs/reports/P00.2.md) | Cari nəticə və insan yoxlaması |
| [Hesabat şablonu](docs/reports/TEMPLATE.md) | Növbəti icazəli partda istifadə edilən format |
| [CHANGELOG](CHANGELOG.md) | Faktiki dəyişiklik tarixçəsi |

## Lokal yoxlama

Python 3.10+ və Git kifayətdir; əlavə paket, hesab və xidmət lazım deyil.
Bütün əmrlər repository kökündən icra olunur:

```bash
python3 scripts/validation/validate_project.py
python3 -m unittest discover -s scripts/validation -p 'test_*.py' -v
```

Qovluq daşındıqda istinadlar nisbi qalır. Mənbə sənədləri surətlənmir və adları dəyişdirilmir.
Master Plandakı GDD.md və docs/MASTER_PLAN.md adları bu repoda yuxarıdakı orijinallara uyğundur.
Gələcək qayda, Art Bible və release sənədləri yalnız müvafiq partda yaradılacaq;
P00.2-də boş Unity/Nakama/art layihələri yaradılmır.

Repository-nin uzaq nüsxəsi və yoxlanmış vəziyyəti STATE-də `repository` sahəsindədir.
Məhsul mənbə koduna açıq mənbə lisenziyası təyin edilməyib; pulsuz alət seçimi
məhsulu avtomatik açıq mənbə etmir.
