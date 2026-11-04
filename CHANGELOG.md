# Dəyişikliklər

## 2026-11-03 — P00.2, insan təsdiqi gözlənilir

- P00.1-in açıq insan qəbulu və yalnız P00.2 icazəsi saxlanıldı.
- Dəyişməyən mənbələrə əsaslanan agent müqaviləsi, inventar, modul sərhədləri,
  status/progress, əməliyyat və test sənədləri hazırlandı.
- Git ignore/atributları, standart Python yoxlayıcısı və mənfi sınaqlar əlavə edildi.
- Unity layihəsi, Android alətləri və oyun modulları yaradılmadı.
- Real yoxlama nəticələri və uzaq repo vəziyyəti [part hesabatındadır](docs/reports/P00.2.md).

### P00.2 yekun auditi — 2026-11-03

- Mövcud origin-ə push qadağası və eyni partda audit icazəsi dəqiqləşdirildi.
- Validatorun növbəti parta keçid və boş scope boşluqları bağlandı; test sübutları yoxlaması əlavə edildi.
- Arxitektura məsuliyyətlərinin ayrıca paket/framework tələbi olmadığı açıqlandı.
- Məhsul qərarları və mənbələr dəyişdirilmədi; yekun nəticələr cari part hesabatındadır.

## 2026-11-03 — P00.3, insan təsdiqi gözlənilir

- P00.2 qəbulu saxlandı və əvvəlki audit commitinə lokal qəbul tag-ı qoyuldu.
- Unity 6000.6.7f1 minimal 2D bootstrap, ayrıca Runtime/Editor/test assembly-ləri yaradıldı.
- Rəsmi Android Build Support/SDK/NDK/OpenJDK quraşdırıldı və ARM64/IL2CPP development APK hazırlandı.
- 7 EditMode və 1 PlayMode testi keçdi; real GUI screenshot və imza/ZIP yoxlaması alındı.
- Preview startup/domain-reload problemləri düzəldildi; recovery nüsxələri lokal qorundu.
- Raw build/cache Git-dən kənardadır; fiziki Android testi hələ aparılmayıb.
- Real nəticələr [P00.3 hesabatında](docs/reports/P00.3.md) və icra qeydindədir.

## P01.1 — 2026-11-03 — AWAITING_HUMAN_APPROVAL

- P00.3 insan qəbulu və Game View krem fon təsdiqi qeydə alındı; fiziki Android NOT_RUN saxlanıldı.
- Uzun/qısa qayda sənədləri, ortaq koordinat/variant müqaviləsi və 67 JSON nümunə əlavə edildi.
- Fixture sxem/invariant/matris yoxlaması və mənfi testlər əlavə edildi; Unity kodu dəyişmədi.
- Lokal yoxlamaların sübutları P01.1 hesabatındadır. P01.2 icazəsi yoxdur; push edilməyib.

## P01.2 — 2026-11-03 — AWAITING_HUMAN_APPROVAL

- P01.1 qəbulu və altı qaydanın ilkin standart seçimi qeyd edildi; müstəqil yerli baxış NOT_RUN/provisional saxlanıldı.
- Immutable LongPosition və Unity/RNG-dən müstəqil LongNardeRules əlavə edildi; yalnız uzun nərd.
- 26 mövcud fixture və 57 əlavə qayda testi, ayrıca hesablamaya qarşı 72 başlanğıc/1000 sintetik mövqe yoxlaması əlavə edildi.
- EditMode 90 PASS/41 aid-part skip, PlayMode 1 PASS, Android bootstrap build uğurlu; fiziki Android NOT_RUN.
- Lokal runner və saxta/missing evidence mənfi testləri; A01 statusu IN_PROGRESS, tam oyun deyil.
- Sübutlar docs/reports/P01.2.* daxilindədir. UI/bot/server/qısa mühərrik əlavə edilmədi; push yoxdur.

## P01.3 — 2026-11-03 — AWAITING_HUMAN_APPROVAL

- P01.2 qəbulu qeydə alındı; yalnız ayrıca pure C# qısa mühərrik əlavə edildi. Uzun runtime/testləri bayt səviyyəsində qorundu.
- Vurma/bar, maksimum/yüksək zər, qoşa, evdən çıxarma və adi/mars/koks; UI/bot/backend/seriya yoxdur.
- 30 mövcud qısa fixture və 81 əlavə hal: 72 başlanğıc kombinasiya, 1000 sintetik mövqe, 102 növbə/38 vurma sınağı.
- EditMode 201 PASS/41 tarixi skip, PlayMode 1 PASS, Python 82 PASS; Android bootstrap build uğurlu.
- 56 unikal fixture real mühərriklərdə keçdi; 11 undo/seriya/reytinq gələcək partlarda qaldı. A02 IN_PROGRESS.
- Fiziki Android və müstəqil yerli qayda yoxlaması NOT_RUN, provisional qeydlər saxlanılır. GitHub push yoxdur; P01.4 icazəsizdir.

## 2026-11-03 — P01.4, insan təsdiqi gözlənilir

- P01.3 açıq insan qəbulu və yalnız P01.4 icazəsi saxlanıldı.
- Pure C# Nard.Match: immutable matç fazaları, engine nəticələri, ayrı WinKind/Points,
  1/Best of 3/5 seriyası, dəqiq RuleSetVersion və rating eligibility predikatı əlavə edildi.
- Uzun/qısa runtime və testləri dəyişdirilmədən qorundu; 62 yeni C# və 9 Python testi əlavə edildi.
- Real nəticə: 263 EditMode PASS, 1 PlayMode PASS, 91 Python PASS, Android build PASS.
- 67 canonical nümunədən 63 PASS, 4 undo NOT_RUN (P02.4); 41 tarixi skip ayrıca saxlandı.
- UI/bot/backend və P02 hazırlanmadı; fiziki cihaz/yerli oyunçu NOT_RUN, push edilmədi.
- Sübutlar və manual baxış: [P01.4 hesabatı](docs/reports/P01.4.md).

## 2026-11-03 — P02.1, insan təsdiqi gözlənilir

- P01.4 qəbulu və yalnız P02.1 icazəsi qeyd edildi; əvvəlki mühərrik/matç/testlər qorundu.
- Ayrı TechnicalBoard scene, read-only BoardSnapshot/BoardGeometry/BoardView və boş zər yerləri əlavə edildi.
- 38 yeni EditMode və 1 PlayMode scene/render testi; 301 EditMode/2 PlayMode PASS, Android build PASS.
- 98 Python testi və 15 struktur yoxlaması; 63 canonical PASS, 4 undo NOT_RUN saxlanır.
- Real Editor camera və GUI Game View screenshot-ları saxlandı; font/rim kontrastı düzəldildi.
- Əvvəldən açıq Editor-un native Analytics shutdown qəzası açıq qeyd edildi; dump lokal qorundu.
- Gameplay input, zər atışı, bot, online və P02.2 yoxdur. Cihaz/yerli yoxlama NOT_RUN, push edilmədi.
- Sübut və manual addımlar: [P02.1 hesabatı](docs/reports/P02.1.md).

### P02.1 gözləməsində vizual referans qeydi — 2026-11-03

- İnsanın təqdim etdiyi əsas taxta referansı orijinal PNG və D024 qeydi ilə saxlandı.
- Üfüqi final taxta, isti/premium Azərbaycan üslubu və şaquli matç dəstəyi qeyd edildi.
- Art kodlaşdırılmadı, Unity və mərhələ dəyişmədi; ayrıca art partına icazə verilmədi.
- Yalnız dəqiq referans yol/hash üçün ölçü istisnası və iki mənfi validator testi əlavə edildi.

## 2026-11-03 — P02.2, insan təsdiqi gözlənilir

- P02.1 qəbulu qeyd edildi; yalnız P02.2 seçim/marker/tap və rədd feedback-i hazırlandı.
- Pure TapSelection yalnız mövcud engine LegalTurns prefikslərini qəbul edir; qaydalar təkrar yazılmadı.
- Sabit 3–3 TapBoard nümunəsi və explicit texniki reset əlavə edildi; RNG/növbə dövrü/drag/undo yoxdur.
- 356 EditMode, 7 PlayMode, 105 Python testi PASS; Android build və imza PASS.
- Əvvəlki 301/2 Unity testi və bütün mühərrik/matç kodu qorundu; 63 canonical PASS/4 undo NOT_RUN.
- Real Input System hadisələri, camera render və GUI şəkilləri saxlandı; fiziki touch/device NOT_RUN.
- Köhnə native Analytics bağlanış qəzası açıq riskdir; P02.2-də təkrarlanmadı, həll iddia edilmir.
- D024 final vizual referansı dəyişmədi və kodlaşdırılmadı. P02.3 açılmadı, GitHub push edilmədi.
- Sübut və manual addımlar: [P02.2 hesabatı](docs/reports/P02.2.md).

## 2026-11-03 — P02.3, insan təsdiqi gözlənilir

- P02.2 qəbulu və yalnız P02.3 icazəsi qeyd edildi.
- LocalTurnLoop + ayrıca IDiceSource/LocalDiceSource; ilkin/adi atış, token Used/Unavailable ayrılığı,
  bütöv lokal commit və avtomatik növbə keçidi mövcud mühərriklər üzərində quruldu.
- İki zərin pipləri və 2/4 hüquq state-dən çəkilir; render RNG çağırmır. Lokal iki insan sınağıdır.
- 524 EditMode, 12 PlayMode, 111 Python testi PASS; Android build və APK imzası PASS.
- 8 test oyunu tamamlandı; əvvəlki 356/7 Unity testi və Rules/Match/TapSelection dəyişmədən qorundu.
- 63 canonical PASS/4 undo NOT_RUN; fiziki cihaz və yerli oyunçu NOT_RUN; native qəza açıq riskdir.
- Final premium referans dəyişmədi və tətbiq edilmədi. Drag/undo/bot/online və P02.4 yoxdur; push edilmədi.
- Sübut və manual addımlar: [P02.3 hesabatı](docs/reports/P02.3.md).

## 2026-11-04 — P02.4, insan təsdiqi gözlənilir

- P02.3 insan qəbulu və yalnız P02.4 icazəsi D029-da saxlanıldı; p02.3-approved lokal tag-ı yaradıldı.
- Pure C# UndoPreview açıq növbənin qanuni prefiksini bərpa edir; LocalTurnLoop commit-dən sonra tarixçəni bağlayır.
- Drag, birhədəfli seçilmiş daş üçün sürətli tap, yanlış drop/cancel/multitouch qoruması və Undo düyməsi əlavə edildi.
- Son insan göstərişi D031 ilə göndərilmiş referans üslubunda boş taxta/daş assetləri hazırlandı və UndoBoard-a tətbiq edildi.
- Qayda, zər, matç mühərrikləri və əvvəlki C# testləri qorunur. 67 canonical nümunənin hamısı ayrıca icra edilir.
- 749 EditMode / 17 PlayMode, Android build/imza və real Editor Game View/input sübutları saxlanıldı.
- 115 Python testi və 15/15 struktur PASS; açıq risklər [P02.4 hesabatında](docs/reports/P02.4.md).
- Fiziki Android/yerli oyunçu NOT_RUN, PROVISIONAL_RULE və köhnə native qəza riski saxlanır. P02.5/push icazəsi yoxdur.

### P02.4 insan rəyi — 2026-11-04

- Səhnə/HUD görünüşünün rədd edilməsi və konkret yerləşim brifi D032-də saxlanıldı.
- P02.4 qəbul edilmiş sayılmır; aktiv part/status və növbəti part qapısı dəyişmir.
- Cari səhnə düzəlişi ilə P02.5 başlanğıcı arasında əhatə cavabı istənilib. Unity kodu/asset/APK dəyişmir.

## 2026-11-04 — P02.5, insan təsdiqi gözlənilir

- İnsanın P02.4 funksional qəbulu və yalnız P02.5 icazəsi D033-də qeyd edildi; əvvəl rədd edilmiş layout qəbul sayılmadı.
- İki responsive yerləşim, safe-area həndəsəsi, board-focus HUD, board üzərində zərlər və kiçik off yuvaları hazırlandı.
- Böyük krem HUD panelləri, debug mətnləri və saxta “Gedişi tamamla” düyməsi çıxarıldı.
- Çevrilmə eyni loop/matç/Undo tarixçəsini saxlayır; həmin kadrın yarımçıq dragı ləğv edilir. P02.6 save yoxdur.
- Əvvəlki mühərriklər, Interaction və C# testləri qorunur. 777 EditMode, 22 PlayMode, 118 Python, 15/15 struktur və 67/67 canonical nümunə PASS.
- Real render/input sübutları, Android ARM64 APK və imza yoxlaması [cari hesabatda](docs/reports/P02.5.md).
- Fiziki Android/iOS/yerli oyunçu NOT_RUN; native Editor qəzası həll edilmiş sayılmır. Final polish, P02.6 və GitHub push yoxdur.

## 2026-11-04 — P02.5 D034 yerləşim düzəlişi, insan təsdiqi gözlənilir

- Portret bütöv 90° fırlatma əvəzinə 24 ID-ni saxlayan iki dik altı-sütunlu tray kimi quruldu.
- Daşlar böyüdü, qonşularla örtüşməyən toxunma sahələri, aydın seçim/marker və stack sayı əlavə edildi.
- İki lokal tərəf, aktiv növbə, real hüquq sayı və işlək Undo/atış vəziyyəti HUD-da göstərilir.
- Əvvəlki şəkillər müqayisə üçün saxlanır; yekun test/build və native Editor sübutları P02.5 hesabatında yenilənir.
- Rules/Match/Interaction və əvvəlki part testləri dəyişmir. Push/P02.6 yoxdur; cihaz/yerli review NOT_RUN.

## 2026-11-04 — P02.5 D035, yalnız portrait UX düzəlişi

- Ayrı çərçivəli hissələr vahid qoz çərçivə və dar menteşəli barla birləşdirildi.
- Zərlər/status və geniş Undo/atış düymələri taxtanın altına toplandı; HUD mətnləri böyüdü.
- Sıx yığın beş aralı görünən pillə və həqiqi sayla göstərilir; mənbə bütün xanadan seçilir.
- Landscape, mühərrik/Undo/zər/növbə məntiqi dəyişmir; yeni portrait testləri və regressiya sübutları cari hesabatdadır.
- AWAITING_HUMAN_APPROVAL; fiziki Android/yerli review NOT_RUN, native Editor riski açıq, push/P02.6 yoxdur.

## 2026-11-04 — P02.5 D036, portrait ölçü/yığın düzəlişi

- Daşın tam sprite qutusu real xana eninin 80%-inə bağlandı; üçbucaq, kənar payı və sıra eyni xana ölçüsündən hesablanır.
- Beş görünən pillə limiti ləğv edildi: bütün 1–15 daş ayrıca, bərabər aralı mərkəzlə yerləşir; toxunma sahəsi geniş qalır.
- Portrait HUD qısaldıldı və oyun sahəsi böyüdü; BAR/çıxarılan daşlar da ölçülü qutulara sığır.
- 1/2/5/10/15 × dörd telefon ölçüsü üçün faktiki render və sərhəd məsafələri, bütün xana/bar/off həndəsəsi yoxlanır.
- Qaydalar/landscape qorunur; yalnız lokal commit. AWAITING_HUMAN_APPROVAL; P02.6/push yoxdur.
