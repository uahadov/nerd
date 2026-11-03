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
