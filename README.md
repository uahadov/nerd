# NƏRD. — Bir masa. Min rəqabət.

Android və iOS üçün Unity 6 / C# əsasında tam 2D nərd oyunu.
Online istiqamət: Nakama authoritative TypeScript və istehsal üçün CockroachDB.
P02.4 funksiyaları qəbul edilib. Yalnız P02.5 responsive matç/HUD yerləşimi işlənir; final polish deyil.
Qəbul edilmiş uzun/qısa mühərriklər, matç sistemi və testləri qorunur.
P00.3 Unity təməli qəbul edilib; fiziki Android testi açıqdır. Lokal iki insan prototipi var; bot və online hələ yoxdur.

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
| [Asset manifesti](docs/ASSET_MANIFEST.csv) | Asset mənbə və hüquqları; texniki qrafika və daxili referans |
| [P00.1 hesabatı](docs/reports/P00.1.md) | Əvvəlki auditin məhdudiyyətləri ilə saxlanan xülasəsi |
| [P00.2 hesabatı](docs/reports/P00.2.md) | Qəbul edilmiş təməlin tarixi nəticəsi |
| [P00.3 hesabatı](docs/reports/P00.3.md) | Qəbul edilmiş Unity/Android təməlinin tarixi sübutu |
| [P01.1 hesabatı](docs/reports/P01.1.md) | Qəbul edilmiş qayda sənədləri və nümunələr |
| [P01.2 hesabatı](docs/reports/P01.2.md) | Qəbul edilmiş uzun mühərrikin tarixi sübutu |
| [P01.3 hesabatı](docs/reports/P01.3.md) | Qəbul edilmiş qısa mühərrikin tarixi sübutu |
| [P01.4 hesabatı](docs/reports/P01.4.md) | Qəbul edilmiş matç/seriya müqaviləsinin tarixi sübutu |
| [P02.1 hesabatı](docs/reports/P02.1.md) | Texniki 2D taxta, cari testlər, screenshot və manual baxış |
| [P02.2 hesabatı](docs/reports/P02.2.md) | Cari seçim, marker, tap gedişi və real input/render yoxlamaları |
| [P02.3 hesabatı](docs/reports/P02.3.md) | Qəbul edilmiş zər/tam növbə və interaktiv sübutlar |
| [P02.4 hesabatı](docs/reports/P02.4.md) | Cari drag, sürətli tap və Undo sübutları |
| [P02.5 hesabatı](docs/reports/P02.5.md) | Cari responsive səhnə/HUD və real test sübutları |
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

## Cari qayda, matç, taxta, tap və növbə testləri — P02.5

```bash
python3 scripts/build/p012.py editmode --part P02.5
python3 scripts/build/p012.py playmode --part P02.5
python3 scripts/build/p012.py android --part P02.5
```

Unity 6000.6.7f1 lazımdır; başqa yerdədirsə --editor parametrini verin.
Runner build/P02.5/unity daxilində source-only lokal test surətini yaradır və onun
cache-indən istifadə edir. Açıq əsas Editor bağlanmır, əsas səhnə/UI dəyişdirilmir.
NUnit XML/log və APK build/P02.5 daxilindədir; insan üçün saxlanan nəticələr docs/reports-dadır.
Fixture-lər docs/rules/fixtures.json-dan oxunur. Uzun test faylı dəyişdirilmədiyi üçün onda 41 tarixi skip görünür; onların 30-u ShortFixture, 7-si MatchFixture adı ilə ayrıca icra olunur.
67 unikal nümunə ayrı adlarla PASS-dır: 26 uzun, 30 qısa, 7 matç, 4 Undo. Tarixi 41 skip saxlanır; hamısı başqa uyğun assembly-də ayrıca icra edilir. Skip özü PASS sayılmır.
PROVISIONAL_RULE seçimi ilkin standart olaraq qəbul edilib, müstəqil yerli oyunçu yoxlaması açıqdır.

## Texniki taxtaya baxmaq

Unity-də `NARD → P02.1 → Open technical board` seçib Play düyməsini basın.
Ayrıca səhnə unity/Assets/Game/Scenes/TechnicalBoard.unity-dir. Uzun nərd ilkin mövqeyi görünür.
P02.1 read-only baxışdır: daş seçimi və zər atışı hələ yoxdur; ekranda açıq yazılır.
Qısa/bar-off state-lərinin real Editor renderləri cari hesabatdadır. Son çayxana dizaynı deyil.
Android P02.1 APK bu taxtadan başlayır; köhnə Bootstrap scene və build settings qorunub.

Gələcək final taxtanın insan-seçilmiş əsas [vizual referansı və tətbiq sərhədi](docs/DECISIONS.md) D024-dədir.
Bu, cari mərhələni və növbəti part icazəsini dəyişmir.

## Əvvəlki texniki tap nümunəsi — P02.2

Unity-də `NARD → P02.2 → Open tap example`, sonra Play seçin. Ağır final art əlavə edilməyib.
Öz daşınıza, sonra bal rəngli hədəfə toxunun. Sabit 3–3 test atışı verilib; zər atışı və yeni növbə yoxdur.
Texniki sıfırlama yalnız həmin eyni nümunəni yenidən göstərir. Bu, undo/revanş deyil.
Əvvəlki P02.1 read-only scene və onun açılma menyusu ayrıca qorunur.

## Əvvəlki texniki lokal matç — P02.3

Unity-də `NARD → P02.3 → Open local long match` və ya `Open local short match`, sonra Play seçin.
İki tərəf eyni cihazda əl ilə oynanır; bot/online yoxdur. İlk atışdan sonra qaydaya uyğun zərlər alınır.
Markerə tap ilə gediş edin; qanuni hüquqlar bitəndə növbə avtomatik dəyişir. Sonra yeni oyunçu zər atsın.
İşlənmiş token xəttlənir, mümkün olmayan qalıq X olur. Final premium dizayn D024-də referans olaraq saxlanır.
Əvvəlki P02.1/P02.2 scene və menyuları ayrıca qalır; onların məhdudiyyətləri cari canlı dövrə aid deyil.

## Qəbul edilmiş sürükləmə və geri alma — P02.4

Unity-də `NARD → P02.4 → Open undo long match` və ya `Open undo short match`, sonra Play seçin.
İlk atışları edin. Daşı markerə sürükləyib buraxın; səhv hədəf və ekran xarici buraxma vəziyyəti dəyişmir.
Öz açıq növbənizdə yuxarıdakı “Geri al” son addımı və zər hüququnu bərpa edir.
Son qanuni addım avtomatik növbəni təsdiqləyir; sonra Undo mümkün deyil.
Seçilmiş daşın yalnız bir hədəfi varsa ona ikinci qısa toxunuş həmin gedişi edir; çox hədəfdə özünüz seçin.
İkinci barmaq və ləğv edilmiş toxunma dragı dayandırır. Son insan göstərişi D031 üzrə göndərilən qoz/krem/naxışlı taxta üslubu indi bu səhnəyə tətbiq olunub.
Əvvəlki texniki səhnələr saxlanır; digər ekranların art mərhələsi və iki responsive yerləşim hələ açılmayıb.
Fiziki Android, müstəqil yerli qayda və responsive orientation testləri tamamlanmış sayılmır.

## Cari matç yerləşimi — P02.5

Unity-də `NARD → P02.5 → Open responsive long match` və ya `Open responsive short match`, sonra Play seçin.
Game görünüşünü Free Aspect ilə dar/uzun, sonra enli edin: eyni matç davam etməlidir.
Portretdə sol/sağ yarılar bir xarici çərçivə və dar menteşəli bar daxilində altı sütunlu dik sahələrdir; taxta bütöv 90° çevrilmir. Landşaftın əvvəlki görünüşü qorunur.
Portret zərləri, statusu və geniş Undo/atış düymələri taxtanın altındakı bitişik idarə sahəsindədir; landşaft zərləri əvvəlki yerdə qalır. Böyük portret yığını beş görünən pillə və həqiqi say nişanı ilə göstərilir.
Son hüquq yenə avtomatik commit edir; ayrıca “Gedişi tamamla” düyməsi yoxdur. Böyük off zolağı kiçik yuvalarla əvəzlənib.
Bu yerləşim prototipidir: fiziki telefon/iOS, yerli qayda yoxlaması və final polish tamamlanmış sayılmır.
