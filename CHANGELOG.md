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
