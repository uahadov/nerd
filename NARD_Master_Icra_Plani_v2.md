# NƏRD. — MASTER İCRA PLANI

**Versiya:** 2.0 · **Tarix:** 3 noyabr 2026 · **Status:** İnkişafa başlamaq üçün əməliyyat planı, kod icra olunmayıb  
**Əsas mənbə:** `NARD_Final_GDD_v1.0.md` və bu söhbətdə qəbul edilmiş qərarlar.  
**Hədəf:** yalnız Android və iOS, Unity 6, tam 2D, Azərbaycan çayxanası estetikası, AI-agent vasitəsilə mərhələli icra.  
**Əsas iş qaydası:** **BİR KİÇİK HİSSƏ → AI İCRA → AI TEST → AI HESABAT → İNSAN TESTİ → AÇIQ TƏSDİQ → NÖVBƏTİ HİSSƏ.**

> **Sənədin mənası:** Bu, “bütün oyunu bir promptda yarat” göstərişi deyil. 87 qəbul edilmiş xüsusiyyət uzunmüddətli backlog kimi qalır; hər icra biletinin həcmi kiçikdir. Agent növbəti biletə istifadəçinin açıq razılığı olmadan keçmir. Fakt, fərziyyə, risk və qeyri-müəyyənlik fərqləndirilir.

---

## I. DƏYİŞMƏZ MƏHSUL MÜQAVİLƏSİ

### I.1. Məhsulun məqsədi

**NƏRD. — Bir masa. Min rəqabət.** Azərbaycana xas vizual və sosial hisslərlə hazırlanmış, klassik uzun və qısa nərdi düzgün oynadan, dostlarla və digər oyunçularla rəqabət aparmağa imkan verən mobil 2D oyundur. İstifadəçi sadəcə düymələrlə dolu proqram açmır, yaşanan, səmimi, isti bir oyun məkanına daxil olur. Nərd qaydaları dekorasiyadan üstündür; oynanış hissi animasiya nümayişindən üstündür.

### I.2. İstifadəçinin birbaşa söylədikləri — qəti məhdudiyyətlər

1. **Yalnız Android və iPhone**; ayrıca brauzer və ya kompüter versiyası üçün vaxt sərf olunmur.
2. **3D yox, 2D**: 3D çətinliyini və istehsal xərcini artırmaq istəmir.
3. **Unity seçilib**; Godot-a keçid gündəmdə deyil; Google Antigravity və Unity-nin rəsmi agent plugin/CLI imkanları prioritetdir.
4. Proqramlaşdırma, qrafiklər, səhnələr, animasiyalar, sınaqlar, düzəlişlər və sənədləşmə **mümkün qədər AI tərəfindən** həyata keçirilməlidir.
5. İstifadəçi heç bir game engine-i əvvəl istifadə etməyib; ona skript qurmaq, Unity Inspector-da çoxlu əl işi aparmaq kimi tapşırıqlar verilməməlidir.
6. Oyun **əsl mobil oyun hissi** verməlidir; biznes tətbiqi/dashboard və yeni domino oyununda hiss etdiyi “oyun yox, proqram” təəssüratı qəti istənilmir.
7. Qara-korporativ, Apple-demo kimi steril, soyuq “luxury”, neon, plastik, standart AI slop, təsadüfi illüstrasiya qarışığı və ağır ornament bolluğu **olmayacaq**.
8. İstiqamət: **müasir Azərbaycan çayxanası** — gün işığı, taxta masa, armudu çay, milli üslubun ölçülü izi, rahat səmimiyyət, rəng, təbii tekstura.
9. **Hər kiçik partdan sonra dayan**; AI təkbaşına növbəti hissəyə keçmir. İstifadəçi gözlə görür, telefonda oynayır, razılıq verir və ya konkret düzəliş istəyir.
10. **AI öz işini birinci özü test edir**: kod, build, davranış, ekran görüntüsü, toxunuş, səhv, regressiya; sonra insan testi gəlir.
11. **Pulsuz-first**: lisenziyalı pulsuz proqramlar, açıq mənbə, lokal test, mövcud 1 GB serverdən (uyğun gəlirsə) istifadə; qeyri-mümkün xərcləri açıq göstər.
12. **87 təsdiqlənmiş xüsusiyyət** roadmap-da qalır; **20 rədd edilmiş** özbaşına yenidən əlavə edilmir.
13. Əvvəl Android daxili testləri və qapalı beta, ictimai yayımlama Android və iOS-da birlikdə; **ilk ictimai buraxılış məcburi və könüllü reklam olmadan**.
14. İdeya çox gözəl görünsə belə, real oyunçu dəyəri və testə yararlılığı yoxdursa sırf effekt üçün əlavə edilməməlidir.

### I.3. Sözün dəqiq mənası: “AI hər şeyi etsin”

Agentin öhdəliyindədir: rəsmi sənədləşməni oxumaq, uyğun paketləri seçmək, layihəni qurmaq, kod yazmaq, Unity scene/prefab yaratmaq, asset yaratmaq və ya tapmaq, animasiya və audio inteqrasiya etmək, avtomat test hazırlamaq, build çıxarmaq, log/ekran görüntüsünü yoxlamaq, dəyişiklikləri izləmək, texniki sənədləri yeniləmək. Agentin **öz-özünə görə bilməyəcəyi və ya real cihaz tələb edən** hallar: əlçatmaz GUI-ni real klikləyib sınaqdan keçirmək, iPhone-da fiziki test, Apple/Google hesabının hüquqi qeydiyyatı, mağaza ödənişi, xarici server üçün admin icazəsi, son “bəyəndim” qərarı. Bunları yerinə yetirmiş kimi iddia edə bilməz.

### I.4. Aşkar edilən zövq nümunələri

- **Seçib:** uzun/qısa nərd, onlayn/offline, real rəqabət, dostluq, reytinq, liqalar, tarixçə, təkrar, fərdiləşdirmə, yüngül gündəlik mükafatlar, isti canlı məkanlar, stikerlər, özünəməxsus səs/animasiya.
- **Rədd edib:** səsli/yazılı chat, klublar, qalibiyyət kartı, AI gediş təhlili, mövsümlük sıfırlama və battle pass, həddindən artıq rəqabət alt-sistemləri, mərc, tamaşaçı reaksiyası, NPC hekayəsi/dialoqları, maskot, öz foto avatarı, şəhər reytinqi, həftəlik açıq turnir, iki ayrıca sadə/mürəkkəb interfeys rejimi.
- **Yəni:** zəngin OYUN təcrübəsi istəyir, amma sadəcə “funksiya olsun” deyə təkrarlanan sistem, sosial şəbəkə və süni monetizasiya istəmir.

### I.5. Uğur meyarı

Birinci uğur: iki nərd rejimi ən kritik qayda testlərini keçir, bir matç başdan sona oynanır, mobil ekranda rahat idarə olunur. İkinci uğur: ana ekran və matç yüksək keyfiyyətli, ardıcıl Azərbaycan ruhlu 2D art-direction-a uyğundur. Üçüncü uğur: iki real müştəri arasında saxtakarlığa davamlı, bərpa olunan onlayn matç işləyir. Nəhayət: real insanların təkrar oynamaq istədiyi məhsul. Kod sətirlərinin sayı uğur göstəricisi deyil.

---

## II. QƏRARLARIN VƏ TƏKLİFLƏRİN STATUSU

**Qəbul edilmiş:** GDD-də A01–A87, buraxılışın 7 qərarı, əsas nərd prinsiplərinin 6 qərarı, variant qərarlarının 6 bəndi, bot qərarları. **Rədd edilmiş:** R01–R20. Yeni bu planın özü, əvvəl təsdiqlənməmiş yeni oyun rejimi və ya hüquqi öhdəlik yaratmır. Bu sənəddə göstərilən dəqiq rəqəmlər (məsələn, ilkin taymer saniyəsi, asset ölçüsü, beta hədəfi) **dizayn fərziyyəsi** kimi işarələnir, beta mərhələsində ölçülüb dəyişdirilir.

**Düzəliş tələb edən köhnə fərziyyə:** Nakama + CockroachDB GDD-də istehsal üçün seçilib. Nakama-nın aktual **rəsmi Docker Compose tutorial-ı PostgreSQL ilə lokal inkişaf nümunəsi** göstərir, rəsmi Linux təlimatı isə CockroachDB-ni əsas dəstəklənən istehsal variantı, PostgreSQL-i inkişaf üçün qeyri-rəsmi dəstək kimi təsvir edir. Buna görə “PostgreSQL tam istehsal üçün təsdiqlənmişdir” demək olmaz; “1 GB server CockroachDB + Nakama üçün yetəcək” də hələ sübut olunmayıb. Yalnız ölçmə ilə qərar verilir.

---

## III. İCRA ÜÇÜN TƏK SƏRHƏDLİ AGENT PROTOKOLU

### III.1. Agentin əvvəlcə oxuduğu sənədlər

1. `GDD.md` (əvvəlki final GDD, məhsulun mənbə qərarları)
2. `docs/MASTER_PLAN.md` (bu plan)
3. `AGENTS.md` (agentin işləmə müqaviləsi)
4. `docs/APPROVED.md` və `docs/REJECTED.md` (A01–A87 və R01–R20)
5. `docs/STATE.json` (hazırda icazə verilmiş part)
6. `docs/DECISIONS.md` (yeni fərziyyə və qərar qeydləri)
7. `docs/ART_BIBLE.md`, `docs/RULES_LONG.md`, `docs/RULES_SHORT.md`, `docs/TEST_MATRIX.md` (müvafiq part yetişəndə).

### III.2. Hər iş dövrü

**0. Oxu və sərhədi yoxla:** İstifadəçinin son icazəsi yalnız hansı partadır? Əvvəlki təsdiq sənəddə qeyd olunubmu? `STATE.json` açıqdırmı? Yoxdursa işi başlatma; yeni feature-a keçmə.  
**1. Planla:** 1 kiçik nəticə, nəyi dəyişəcəksən, nəyi dəyişməyəcəksən, qəbul meyarları, risk.  
**2. İcra et:** Unity və kod/API/asset işlərini mümkün qədər özün bitir. Lazımsız əlavə funksionallıq yaratma.  
**3. Testləri yaz və işə sal:** sadəcə test faylı yaratmaq kifayət deyil; həqiqi nəticə, exit code, XML/log çıxar.  
**4. Vizual yoxla:** Unity Game View və ya build aç; screenshot/video, ölçü, font, daş/zər görünüşü, touch hədəfləri. GUI-yə real çıxış yoxdursa “görmədim” qeyd et.  
**5. Regressiya:** əvvəl işləyən rejimləri yenidən test et.  
**6. Hesabat + sübut:** nə edilib, hansı fayllar dəyişib, test nəticələri, nəyi yoxlaya bilməyib, screenshot/build yerləşməsi, insanın 3–5 test addımı.  
**7. Təhlükəsiz commit:** kiçik, adlandırılmış Git commit; uğurlu mərhələ üçün tag və keçid. Commit icazəsi özlüyündə növbəti mərhələ icazəsi deyil.  
**8. DAYAN:** `WAITING_FOR_HUMAN_REVIEW`; başqa mərhələyə, mağaza/deploy/ödənişə keçmə.  
**9. İnsan rəyi:** “QƏBUL”, “DÜZƏLT”, “GERİ QAYTAR”, “SUAL” nəticələrindən biri. “DÜZƏLT” olduqda yalnız cari part üzrə səhvləri düzəlt və yenidən test et.  
**10. Sonrakı part:** yalnız cari part açıq şəkildə təsdiqlənib yeni partın başlanmasına icazə verilibsə.

### III.3. Bir partın ölçü limiti

- **1 əsas görsənən nəticə** (məsələn, zər animasiyası) və yalnız onun işləməsi üçün lazım olan ən az texniki dəyişiklik.
- Bir part 2–4 bir-birindən müstəqil xüsusiyyət yığmamalıdır. Həcm böyüyərsə A/B alt-partlara bölünür.
- Daha çox vaxt keçirmək və daha çox kod yazmaq üçün scope genişləndirmə.
- Bir partın bağlanması üçün əsas testlər, sınaq addımları və nəticə sübutu lazımdır.
- Başqa sistemə toxunmaq məcburiyyətində qalırsa əvvəl scope-impact göstər; təsdiqsiz böyük refactor etmə.

### III.4. Qadağan olunan gizli davranışlar

Agent istifadəçi icazəsi olmadan: növbəti partı başlatmır; yeni oyun sistemi əlavə etmir; təsdiqlənmiş dizaynı dəyişmir; serveri internetə açmır; pul xərcləmir; pullu hesab açmır; şəxsi məlumat göndərmir; real mağazaya yayımlamır; Git tarixini məhv edən əməliyyat aparmır; prod database-i silmir; işləməyən testi silib “hamısı yaşıl” yazmır; asset lisenziyasını uydurmur; real iPhone testi aparılmadığı halda “iOS işləyir” demir.

### III.5. Bilet sonu hesabat forması

```
PART: P03.2 — Portret matçda toxunma
STATUS: WAITING_FOR_HUMAN_REVIEW
İCRA: 1 cümlə ilə yekun
DEYİŞƏN FAYLLAR: ...
AVTOMATİK TESTLƏR: 47 keçdi / 0 uğursuz / 1 bloklandı
BUILD: platforma, fayl, build ID və ya "hazır deyil"
VİZUAL SÜBUT: screenshot/video yolu
YARADILAN ASSETLƏR: mənbə, generator, istifadə hüququ
MƏHDUDİYYƏT: yoxlanmamış hissələr
İNSAN TESTİ: 3–5 konkret əməl + gözlənilən nəticə
SİZDƏN LAZIM OLAN: QƏBUL / DÜZƏLT / GERİ QAYTAR
NÖVBƏTİ PART: YALNIZ TƏSDİQDƏN SONRA P03.3
```

### III.6. Status qaydası

`PLANNED → IN_PROGRESS → SELF_TEST_FAILED` və ya `SELF_TEST_PASSED → WAITING_FOR_HUMAN_REVIEW → REWORK_REQUIRED` və ya `HUMAN_APPROVED → LOCKED`. `SELF_TEST_PASSED` öz-özlüyündə `HUMAN_APPROVED` demək deyil. Əlavə hazır feature-a toxunmaq yalnız regresiya testi ilə mümkündür.

### III.7. İstifadəçinin verə biləcəyi təbii mesajlar

“Bu partı təsdiqləyirəm, növbəti parta keç” — açıq icazə.  
“Bu düymə işləmir, düzəlt” — cari part yenidən işlənir, irəliləyiş yox.  
“Görünüş xoşuma gəlmir, taxta çox plastikdir” — art-direction düzəlişi, digər sistemlər toxunulmaz.  
“Birini də əlavə et” — GDD-lə uyğunluq və scope göstərilmədən avtomatik backlog artımı yox.  
“Hələ yoxlamağa imkanım olmayıb” — gözləmə, yeni part yoxdur.

---

## IV. PULSUZ-FIRST TEXNİKİ VƏ MALİYYƏ STRATEGİYASI

### IV.1. Tövsiyə olunan stack

| Təyinat | Seçim | Xərc və qeyd |
|---|---|---|
| Unity Editor | Unity 6 Personal, C# | Uygun gəlir/maliyyələşmə həddinə qədər pulsuz; Unity splash screen məcburiyyəti yoxdur |
| Agent | Google Antigravity + Unity-nin rəsmi agent plugini | Plugin/CLI dəstəyi var; istifadə edilən AI modelinin planı/kvotası ayrıdır |
| Unit və play test | Unity Test Framework + CLI | Editor daxilində, əlavə xidmət şərt deyil |
| Kod saxlama | Git + mövcud GitHub hesabında PRIVATE repo | Uyğun pulsuz planla başlamaq |
| Asset yaradılması | AI-nin icazəli image/audio alətləri; Unity/SVG/kod | Mövcud alətlərlə et; ayrıca bahalı xidmət açma |
| Multiplayer | Nakama OSS | Server idarəsi özündədir; POC vacibdir |
| İstehsal bazası | Nakama-nın versiya üzrə rəsmi dəstəklədiyi CockroachDB | 1 GB serverə sığması **qəti deyil** |
| Lokal POC bazası | Nakama-nın cari Docker nümunəsinə uyğun PostgreSQL | Yalnız development; istehsal uyğunluğu ayrıca təsdiqlənir |
| Mobil test | Android real cihaz + ADB | APK quraşdırması pulsuz |
| iOS build | Uyğun macOS/Xcode mühiti və ya Unity Build Automation | İmzalama/ödəniş/kvota real olaraq yoxlanmalı; sıfır xərc zəmanəti yoxdur |
| TLS | Etibarlı, pulsuz sertifikat (məs. Let's Encrypt) | Server/şəbəkə konfiqurasiyası lazımdır |
| CI | Lokal test skriptləri; lazım gələrsə pulsuz CI kvotası | CI istifadə etmədən də başlamaq mümkündür |

### IV.2. Sənin 1 GB serverin haqqında konkret qərar

**İlkin mərhələdə serverə heç nə quraşdırmırıq.** Unity + qaydalar + bot + art laptopda hazırlanır. Nakama lazım olan gün əvvəl lokal Docker-də yoxlanılır. 1 GB VPS yalnız ölçmədən sonra gündəmə gəlir. Tək VPS-də DB + Nakama + sistem servisinin RAM yükü, saxlanma və ehtiyat nüsxəsi ciddi problemdir; “işə düşdü” istehsal üçün təhlükəsizdir demək deyil.

Server haqqında AI yalnız bu *oxuma/ölçmə* məlumatlarını toplayır: RAM, boş disk, CPU, swap, OS, açıq portlar, mövcud servislər, fərdi serverdə başqa layihələrin olub-olmadığı. Quraşdırma, port açma, firewall və sistem dəyişiklikləri ayrı təsdiq tələb edir. Şifrələr, şəxsi açarlar və tokenlər söhbətə və Git-ə yazılmır.

Qapalı POC zamanı boş yaddaş, yük altında istifadə, restart, bağlantı, data bərpası, şifrəli trafik ölçülür. Ölçmə müsbətdirsə server yalnız aşağı yüklü sınaq üçün istifadə oluna bilər; yoxdursa lokal test və başqa legitim pulsuz/ucuz host variantı barədə müqayisə təqdim edilir. **Təsdiqsiz ödənişli server alınmır.** CockroachDB üçün “1 GB mütləq yetər” fərziyyəsi yoxdur.

### IV.3. Xərcləri dürüst bölmək

**İndi sıfır əlavə ödənişlə mümkündür:** qaydaların yazılması/testi, Unity Personal ilə prototip, Android APK, Git, lokal offline bot, lokal server POC və assetlərin lisenziyalı/pulsuz üsullarla hazırlanması (mövcud AI kvotaları daxilində).  
**Gələcəkdə ödəniş tələb edə bilər:** iOS cloud build, Apple Developer üzvlüyü, Google Play developer hesabı, daha güclü VPS, DNS/domen, bəzi musiqi/grafika xidmətləri, AI kvotalarını aşmaq.  
**İctimai mağaza yayımlaması sıfır xərcli hesab olunmur:** Google Play developer qeydiyyatı $25 birdəfəlik, Apple Developer Program standart $99/il (regiondan asılı variasiya var); 18 yaş/region üzrə hüquqi yetkinlik tələbləri ayrıca rol oynayır. Yetkin, hüquqi uyğun hesab sahibi ilə əvvəlcədən plan qurmaq lazımdır. Qeydiyyat hüququnu saxtalaşdırmaq olmaz. Yeni şəxsi Google Play hesablarında 12 tester / 14 gün qapalı test tələbi nəzərə alınır.

### IV.4. Xərci artıran şeylərə qarşı sərt qayda

AI “iş asan olsun” deyə pullu Firebase, Photon, reklamsız versiyada reklam SDK-sı, pullu asset pack, baha hosting, ayrıca generativ API, ikinci backend, başqa oyun mühərriki əlavə etmir. Alternativ təqdim edərsə **problem / pulsuz həll / ödənişli həll / niyə pulsuz alınmır / ilkin risk** ardıcıllığı ilə sübut edir. Uydurma benchmark, uydurma qiymət və gəlir proqnozu yoxdur.

---

## V. ART DIRECTION: “GÖZƏL OYUN”, “AI SLOP” DEYİL

### V.1. Vizual şəxsiyyət

- **İsti krem** `#F6EBD5`: əsas işıqlı mühit; **terrakota** `#C96F4C`: hərəkətlər; **firuzə** `#4D9995`: kiçik vurğu; **bal sarısı** `#E8B85D`: mükafat; **qoz** `#79563D`: taxta və tipografik vurğu.
- Məkan günəşli və ya yumşaq axşam işığı ilə təqdim edilir, ekranın böyük sahələri korporativ qaraya çevrilmir.
- **2D əl işi / redaktə edilmiş illüstrasiya üslubu**, lakin nərd daşları və zərlər toxunanda real material hissi verməlidir. Bir ekran fotorealizm, o biri ekran generik 3D cartoon olmamalıdır.
- Xalça ornamenti dekorativ kənarlarda və xüsusi kolleksiyada istifadə ediləcək; əsas oyun xanası üzərində gedişləri gizlətməyəcək.
- Düymələr oyun komponenti kimi hiss olunacaq, standart mobil dashboard tabları və 12 eyni böyük kart olmayacaq.
- Yeni UI çoxlu ekranlarla deyil, **bir ekrandakı real görünüş və interaksiya** ilə təsdiqlənəcək.
- İstifadəçinin azərbaycanca bütün hərfləri (ə, ğ, ı, İ, ö, ş, ç, ü) yoxlanır; pulsuz kommersiya istifadəsinə icazəli uyğun font seçilir.

### V.2. 2D art asset istehsal zənciri

**Hər asset üçün ayrıca `Asset Request` yaradılır:** ID; konkret yer; lazım olan fayl növü (PNG/SVG və s.); ölçü/nisbət; şəffaflıq; qatlara bölünmə; rənglər; perspektiv; işıq mənbəyi; mənfi tələblər; oyunda test yeri; lisenziya; versiya. AI əvvəl ehtiyacın **Unity daxilində sadə vector/UI/sprite yolu ilə** yaradılıb-yaradıla bilmədiyini yoxlayır. Quru iş üçün ayrıca böyük şəkil generatoruna müraciət olunmur.

**Prioritet sırası:**
1. Özünün kodla çəkə biləcəyi obyekt (sadə ikon, gediş markerləri, responsiv UI) — əlavə şəkil yox.
2. AI agentində mövcud olan icazəli görüntü yaratma imkanı — prompt, nəticə, hüquq qeydi, keyfiyyət yoxlaması ilə yarat.
3. Rəsmi/faydalı pulsuz mənbədən **lisenziyası yoxlanmış** asset — autor, mənbə URL-i, lisenziya, dəyişiklik hüququ ilə daxil et.
4. Agentin image generation alətinə çıxışı yoxdursa və ya nəticə tutarlı deyilsə: həmin **bir asset** üçün tam hazır prompt və mənə təqdim edilməli fayl tələblərini ver, `WAITING_FOR_ASSET`; başqa sistemi qurmağa keçmə. İstifadəçi şəkli yaradıb göndərdikdən sonra uyğunlaşdır, test et və cari partı yekunlaşdır.

**Qəti olmaz:** watermarklı foto, başqa mobil oyunun hazır UI screenshot-u, copyright-lı mascot, lisenziyası bilinməyən internet şəkli, minlərlə təsadüfi AI şəkli, şəkil üzərində AI ilə generasiya olunmuş anlaşılmaz Azərbaycan dilində yazılar. **Mətn Unity daxilində fontla yazılır**, şəkillər əsasən mətnsiz olmalıdır.

### V.3. Stil ardıcıllığı üzrə qəbul meyarları

Eyni: işıq istiqaməti, kölgə yumşaqlığı, perspektiv, obyekt konturu, tekstura sıxlığı, material palitrası, kamera bucağı, kənar boşluqları, UI tipografiyası, ikon xətt qalınlığı. “Bu ayrıca yaxşı şəkildir” kifayət deyil; **digər ekranlarla eyni oyuna məxsus olmalıdır**. Mübahisəli asset avtomatik qəbul edilmir.

### V.4. Ana ekranda istənilən oyun hissi

Ana ekran **canlı nərd masası** kimi qurulur: arxa plan diqqəti oğurlamayan rahat çayxana; mərkəzdə nərd masası; masa/vizual Oyna interaksiyası; oyunçu avatarı və əsas status, oyuna mane olmayan kolleksiya/xəritə/dost ikonları. Çay buxarı və yüngül işıq hərəkəti mümkündür; animasiyalar telefon batareyasını yormamalı və məcburi gözlətməməlidir. İlk matça keçid 1–2 toxunuş olmalıdır.

### V.5. Matç ekranında prioritet

24 xana aydın görünür, 15 daş hər tərəfdə rahat ayırd edilir, zərlər yanlış vizual siqnal vermir, sürüşdürmə rahatdır, toxunma hədəfləri barmaq üçün yetərlidir. Portret və landşaft **ayrı yerləşim** istəyə bilər, amma game state eynidir. Qrafika qaydalar oxunaqlığını zəiflətməyəcək. Uzun intro və parlaq qələbə effekti sürətli matçlarda təkrar əsəbiləşdirməməlidir. Animasiya sürət parametri saxlanır.

### V.6. Audio və motion

Zər masaya düşdükdə yüngül material səsi; daş hərəkətində səthi hiss etdirən klik; məkanın sakit səsi; razılıq/qələbədə zövqlü və qısa accent. Əsas qayda: audio mənbə və lisenziyası məlum, səs parametrləri müstəqil, sıfır səsdə oyun tam anlaşılandır. Məkan musiqiləri orijinal və ya uyğun lisenziyalıdır, müasir mahnılardan icazəsiz istifadəyə yol verilmir.

---
## VI. ADDIM-ADDIM İCRA BACKLOG-U — HƏR SƏTR BİR İNSAN TƏSDİQ QAPISIDIR

> **Əsas tələb:** Aşağıdakı bölmələr “AI bunların hamısını indi etsin” mənasını vermir. **Yalnız bir bilet aktiv olur.** Bilet bitəndə agent test nəticəsini göstərir və dayanır. Sonrakı bilet üçün yeni istifadəçi razılığı vacibdir. Bilet texniki baxımdan böyük çıxarsa eyni ID altında `.1a/.1b` kimi parçalanır.

### FAZA 0 — Təhlükəsiz başlanğıc və layihənin təməli

**P00.1 — Mövcud mühiti yalnız oxu.**  
**AI edir:** Garuda Linux, disk, RAM, Unity Hub/Editor, Android Build Support, JDK/SDK/NDK, Antigravity, Unity plugin/CLI, Git versiyalarını yoxlayır. Hansı alətin artıq qurulduğunu tapır, həll yollarını rəsmi sənədləşmədən seçir. Kompüterdəki başqa layihələrə toxunmur.  
**Avtomat test:** alətlər açılır? Unity versiyası uyğundur? Disk ehtiyatı kifayətdir? Plugin Antigravity tərəfindən görünür?  
**İnsan yoxlaması:** “Bəli, bunlar mənim mühitimdir; heç nə pozulmayıb”.  
**Bu partda yoxdur:** heç bir oyun funksiyası, server, pullu xidmət, paketin kor-koranə yüksəldilməsi.

**P00.2 — Git, sənədlər və vəziyyət faylları.**  
**AI edir:** private repo; Unity üçün düzgün `.gitignore`; `GDD.md`, `MASTER_PLAN.md`, `AGENTS.md`, `STATE.json`, təsdiq/rədd inventarı, decisions, test template, asset registry. Repo `Library`, build cache, parol və token saxlamır.  
**Avtomat test:** project root oxunaqlıdır, GDD siyahıları A01–A87/R01–R20 dəqiq sayılır, status `WAITING_FOR_HUMAN_REVIEW`; Git-ə böyük cache və secret daxil deyil.  
**İnsan:** repo adı/fayl strukturu görünür, digər şəxsi repolar toxunulmazdır.  
**Yoxdur:** animasiya, UI, Nakama.

**P00.3 — Boş Unity 2D layihəsi və minimal build.**  
**AI edir:** Unity 6-nın dəstəklənən sabit versiyasını sabitləyir; 2D layihə, ortaq render/input/safe area quruluşu, Android build support; bir sadə ekran, açılış və bağlanış.  
**Avtomat:** Unity compile `0 errors`, batch/EditMode ilk test, Android APK çıxması.  
**İnsan:** Android telefonda açılma/bağlanma, çöküb-çökməmə.  
**Yoxdur:** hazır oyuna bənzəyən yalan dekor və online login.

### FAZA 1 — Nərd qaydalarının səhvsiz özəyi

**P01.1 — Tam qayda matrisi, yalnız sənəd və test ssenariləri.**  
**AI edir:** 24 xana, 15 daş, iki hərəkət istiqaməti, başlanğıc düzülüşləri, zar seçim qaydaları, uzun nərddə baş/altılıq blok, qısa nərddə bar/vurma, evə toplama/çıxarma, mars/koks və Best of 3/5 vəziyyətlərini nümunələrə çevirir. GDD-də variant kimi seçilən və yerli oyunçularla doğrulanmalı 3–3/4–4/6–6 ilk növbə istisnasını və başlanğıc atışını ayrıca `PROVISIONAL_RULE` kimi işarələyir.  
**Avtomat:** fixtures JSON schema, qayda mətnində ziddiyyət və undefined hal yoxlaması.  
**İnsan:** qaydaların məntiqi nümunələrini oxumaq, mümkün olduqda təcrübəli nərd oyunçusu ilə 5 əsas mövqeyi müqayisə etmək.  
**Yoxdur:** Unity UI, bot, server. **Məcburi qərar:** uyğun gəlməyən yerli variantlar `RulesetVersion` ilə saxlanılır, səssiz dəyişdirilmir.

**P01.2 — Uzun nərd qanuni gediş motoru.**  
**AI edir:** `LongNardeRules` pure C# modul; ilkin düzülüş, zər, başdan çıxış, blok/altılıq, mümkün maksimum zər istifadə, qeyri-qanuni gedişin rəddi, daşların evə toplanması.  
**Avtomat:** normal/qoşa zər; bloklanmış hal; iki daş istisnası; zərlərin sıradan asılılığı; bütün qanuni gedişlərin generasiyası; oyun vəziyyətinin dəyişməməsi (`immutability` və ya nəzarətli state). Min 30 konkret fixture ilkin hədəf.  
**İnsan:** 5 izahlı mövqedə gözlənilən gedişləri təsdiqləmək.  
**Yoxdur:** qısa nərd, 2D sprite, audio.

**P01.3 — Qısa nərd qanuni gediş motoru.**  
**AI edir:** `ShortNardeRules` pure C# modul; 2/5/3/5 başlanğıc düzülüşü, vurma, bar-dan məcburi geri giriş, evə toplama, overshoot, bloklama, qoşa zər.  
**Avtomat:** bar boşalmadan başqa daşı hərəkət etdirməmək; yüksək zər prioriteti; tam/yarım bloklanma; mars/koks nəticəsi; min 30 fixture ilkin hədəf.  
**İnsan:** göstərilən qısa nərd nümunələrini yoxlamaq.  
**Yoxdur:** backend və bot.

**P01.4 — Qələbə, partiyalar və qayda müqaviləsi.**  
**AI edir:** match state machine (`Created`, `Rolling`, `Moving`, `TurnCommitted`, `Finished`), adi/mars/koks, Best of 3/5, `RuleSetVersion`, qalibiyyət növü və xalların ayrı hesablanması.  
**Avtomat:** 1 matç, iki/üç qələbə seriyası, qoşa dəyərin vizual yox xalı, bot və özəl matçın reytinq artırmaması, hər rejim üzrə fixture.  
**İnsan:** seriyanın qələbəni düzgün elan etdiyini görmək.  
**Yoxdur:** liderlər cədvəli/mağaza.

### FAZA 2 — Tam oynanıla bilən sadə 2D prototip

**P02.1 — Texniki nərd taxtası.**  
**AI edir:** sadə, oxunaqlı 2D 24 xana, 15+15 daş, iki rəng, zər yerləri; bütün daşlar data state-ə bağlıdır. Müvəqqəti qrafika olduğu açıq qeyd olunur, amma oynana bilən görünüş olmalıdır.  
**Avtomat:** 24 xana düzgün indekslənir, state render eynidir, heç bir daş gizli qalmır.  
**İnsan:** Android-də taxtaya baxıb bütün zonaların aydın olduğunu yoxlayır.  
**Yoxdur:** çayxana fonu və avatarlar.

**P02.2 — İlk toxunma ilə gediş.**  
**AI edir:** daşı seçmək, mümkün gediş markerləri, seçilən xanaya toxunaraq hərəkət; mümkün olmayan xananı səssiz qəbul etməmə və qısa geri bildirim.  
**Avtomat:** sadə gediş, blok, yanlış növbə, iki yerə klik və sürətli ardıcıl toxunma, state/render sinxronluğu.  
**İnsan:** 5 dəfə daşı hərəkət etdirir, ən azı bir qadağan hərəkəti yoxlayır.  
**Yoxdur:** drag, animasiya, online.

**P02.3 — Zər atmaq və tam növbə.**  
**AI edir:** iki zərin state ilə vizual uyğunluğu, növbənin başlaması/bitməsi, hər zərin istehlakı, qoşa zər və növbə dəyişmə; zəri vizual atmaq nəticəyə təsir etməyəcək.  
**Avtomat:** bütün qoşa/sadə kombinasiyalar, avtomatik növbə, eyni zərin iki dəfə istifadə olunmaması.  
**İnsan:** bir neçə növbə zər atıb oynayır, zər/daş uyğunluğunu hiss edir.  
**Yoxdur:** effektli zər animasiyası.

**P02.4 — Drag-and-drop, sürətli gediş və undo.**  
**AI edir:** daşı sürüşdürmək, uyğun hallarda sürətli toxunma; yanlış dragi əvvəlki yerə qaytarmaq; növbə təsdiqlənmədən geri almaq.  
**Avtomat:** ekran sərhədi, barmaq sürüşməsi, iki eyni nöqtə, undo sonrası zər hüququnun bərpası, committed move-un geri alınmaması.  
**İnsan:** qısa və uzun daş hərəkəti, səhv sürüşdürmə, undo.  
**Yoxdur:** gözəl kölgə əlavə etmək adıyla davranış dəyişmək.

**P02.5 — Şaquli və üfüqi matç ekranları.**  
**AI edir:** iki ayrı responsive layout; taxta hər iki istiqamətdə aydın, daşlar sıxışmır, təhlükəsiz sahələr və fərqli ölçülər qorunur, ekran çevriləndə state itmir.  
**Avtomat:** fərqli aspekt nisbətlərində screenshot/UI test, state-in eyni qalması, notch/safe area, daş üst-üstə qalma yoxlaması.  
**İnsan:** telefonunu çevirib eyni matçın davam etdiyini yoxlayır; mümkün olsa kiçik və böyük ekran.  
**Yoxdur:** ayrıca desktop ekranı.

**P02.6 — Offline matçın saxlanması və bərpası.**  
**AI edir:** tətbiq arxa plana gedəndə, qapananda və ya restart olanda oyunun qaldığı yer, bot növbəsi, zərlər, seriya vəziyyəti lokal saxlanır. Versiyalı save format, zədələnmiş fayl üçün təhlükəsiz fallback.  
**Avtomat:** kill/restart, tətbiqi 2 dəfə açmaq, köhnə save, pozulmuş save, yarımçıq növbə.  
**İnsan:** matçda çıxıb geri açır, davam edir.  
**Yoxdur:** bulud hesabı.

### FAZA 3 — Offline rəqib və əsl matç dövrü

**P03.1 — Asan uzun nərd botu.**  
**AI edir:** yalnız qayda motorunun verdiyi gedişlərdən qanuni, sadə qərar seçən offline bot; avatar hələ lazım deyil.  
**Avtomat:** 100+ simulyasiya üçün crash/illegal move `0`, nəticələrin qərəzsiz RNG-dən asılılığı.  
**İnsan:** botla tam uzun nərd matçı oynayır.  
**Yoxdur:** başqa bot səviyyəsi.

**P03.2 — Asan qısa nərd botu.**  
**AI edir:** bar/vurma/evə toplanma üçün qısa nərdə uyğun sadə strategiya; offline.  
**Avtomat:** bot illegal move `0`, yüzlərlə vəziyyətdə hərəkət tapır və matçı bitirir.  
**İnsan:** botla qısa nərd matçını tamamlayır.  
**Yoxdur:** botu saxta insan kimi təqdim etmək.

**P03.3 — Orta çətinlik.**  
**AI edir:** risk, bloklama, daşların inkişafı və cavab mövqelərini nəzərə alan qiymətləndirmə.  
**Avtomat:** asan botla yüzlərlə eyni başlanğıc/seed şəraitli matçda statistik fərq, növbəyə cavab gecikməsi. Rəqəmlər ölçülür, uydurulmur.  
**İnsan:** asan və orta botun fərqli hiss etdirdiyini görür.  
**Yoxdur:** zəri qəsdən botun xeyrinə dəyişmək.

**P03.4 — Usta çətinliyi.**  
**AI edir:** daha geniş mövqe qiymətləndirməsi və axtarış; mobil cihazda kadrların donmamasına diqqət. Hər iki qayda variantına uyğun ayrı bot strategiyası.  
**Avtomat:** orta botla sınaq, CPU/memory ölçmə, hər bot növbəsində max gecikmə hədəfini faktiki cihazda təsdiqləmə.  
**İnsan:** Usta real çətin, amma “aldadır” hissi yaratmır.  
**Yoxdur:** ödənişli AI API.

**P03.5 — Sürətli/klassik/Best of 3/5 və nəticə.**  
**AI edir:** sürətli/klassik taymerin ilkin fərziyyə parametrləri, partiya seriyası, qələbə/məğlubiyyət statusu, revanş, bot oyununda pauza.  
**Avtomat:** taymer sıfırlanması, sona çatma, oyun vaxtı fonda olması, seriya ardıcıllığı, nəticənin bir dəfə elan edilməsi.  
**İnsan:** rahat/sürətli matç və revanşı yoxlayır.  
**Yoxdur:** ranked backend.

### FAZA 4 — İlk real vizual məhsul: “AI slop” qapısı

**P04.1 — Art Bible və bir vizual istiqamət seçimi.**  
**AI edir:** GDD palitrası və istəklərə əsasən eyni üslubda 1 əsas moodboard + ana masa və matç üçün vizual eskiz, material/işıq/font/ikon/negative rules. AI görüntülərini yalnız referans kimi yox, sonradan təmiz asset ola biləcək keyfiyyətlə yaradır.  
**Avtomat:** fayllar ölçü, transparanlıq, rəng, asset manifest, lisenziya qeydindən keçir.  
**İnsan:** istilik, səmimiyyət, milli ruh və “oyundur, proqram deyil” hissini təsdiqləyir.  
**Yoxdur:** bütün oyunun 18 ekranını bir dəfəyə rəngləmək.

**P04.2 — Son keyfiyyətdə nərd taxtası.**  
**AI edir:** qoz ağacı, əl işi toxuma, incə milli naxış, daş və zər ailəsi; mobil oxunaqlılıq. Asset lazımdırsa özü yaradır/tapır, alınmırsa bir tam prompt verir.  
**Avtomat:** hər iki istiqamətdə daş kontrastı, gediş markerləri, rendering/sprite atlas, ölçü və RAM.  
**İnsan:** real Android-də taxtanı izləyir, “plastik”, “solğun”, “qarışıq” kimi hissləri bildirir.  
**Yoxdur:** başqa səhnə kolleksiyaları.

**P04.3 — Zərin və daşın motion/sound hissi.**  
**AI edir:** qısa təbii zər atışı; daşın seçilməsi/sürüşməsi/yerləşməsi; minimal vibrasiya; səs; azalt/söndür parametri.  
**Avtomat:** animasiya state-i dəyişmir; sıx-sıx toxunma; skip; 60 FPS hədəfi ölçülür, cihazdan asılı olduğu qeyd edilir.  
**İnsan:** 20+ gediş edir, səsin və animasiyanın bezdirici olub-olmadığını yoxlayır.  
**Yoxdur:** mükafat mağazası.

**P04.4 — İlk Azərbaycan məkanı: İçərişəhər həyəti.**  
**AI edir:** mətnsiz, vahid üslublu arxa plan və 2–3 zərif 2D qat; çay/daş divar/gün işığı; taxtaya kölgə salıb oxunuşu pozmamaq.  
**Avtomat:** performans, əksinə çox diqqət çəkən hərəkətin olmaması, ekran kənarı clipping.  
**İnsan:** səhnənin səmimiliyini, yerli xarakterini və “ağır UI deyil” hissini yoxlayır.  
**Yoxdur:** Şəki/Bulvar/Lahıc hələ.

**P04.5 — Ana masa (home), Oyna girişi.**  
**AI edir:** nərd masasını baş qəhrəman edən giriş/ana UI, yalnız Oyna və lazımi keçid; 1–2 toxunuşla matç; logo, ilk səssiz animasiya.  
**Avtomat:** ən qısa istifadəçi axını, offline/xəta halları, mobil safe area, Azərbaycan hərfləri.  
**İnsan:** yeni istifadəçi kimi oyunu açır və az addımda matça keçir.  
**Yoxdur:** profil/mağaza/xəritə dolu dashboard.

**P04.6 — Oyun rejimi seçimi və qısa interaktiv intro.**  
**AI edir:** uzun/qısa nərd, bot seçimi, sürətli/klassik; 2–3 saniyə hədəfli, skip edilən qısa təqdimat; ekran sanki oyunun obyektləri kimi açılır.  
**Avtomat:** intro təkrar oynanılsa da ilişmir, skip, geri, orientation, yanlış seçim, bütün seçimlərin həqiqi rejimə aparması.  
**İnsan:** ilk giriş, sonrakı giriş, sürətli oyuna başlama.  
**Yoxdur:** online seçim işləmədən aktiv görünməz; açıq “hazırlanır” və ya gizli.

**P04.7 — Matç nəticəsi, sadə profil və əsas təlim.**  
**AI edir:** qələbə/məğlubiyyət ekranının səmimi qısa animasiyası, təkrar matç; yeni oyunçu üçün vəziyyətə bağlı kiçik interaktiv izah.  
**Avtomat:** yanlış qələbə mətninə yol verilmir, revanş state sıfırlanır, skip və ekran ölçüləri.  
**İnsan:** bir oyunu bitirir və revanş edir; uzun tutorial məcburi deyil.  
**Yoxdur:** rəqabət reytinqi və satılacaq kosmetika.

### FAZA 5 — Nakama texniki sınağı: 1 GB serverə tələsmədən

**P05.1 — Nakama seçim riskinin ölçülməsi.**  
**AI edir:** Nakama-nın cari stabil OSS versiyası və rəsmi Unity SDK, TypeScript runtime API-si, DB seçimləri, Docker resurs tələbləri, lisenziyanı rəsmi mənbədən yoxlayır. Lokal development üçün rəsmi Docker compose nümunəsinə əməl edir.  
**Avtomat:** konteynerlər başlayır, DB bağlantısı, healthcheck, restart, log, RAM istifadəsi, heç bir default parola internet çıxışı yoxdur.  
**İnsan:** lokal sınağın başqa sistemlərə təsir etmədiyini təsdiqləyir.  
**Yoxdur:** 1 GB VPS-ə deploy, ödəniş, açıq port.

**P05.2 — Minimal server-authoritative nərd POC.**  
**AI edir:** Unity müştəri → Nakama `roll_request`, `move_proposal`, `end_turn`, `state_snapshot`; server zəri yaradır, gedişi yoxlayır; client-ə sadəcə təsdiqli state göndərir. **Unity-dəki qaydalar və Nakama TypeScript qaydaları üçün ortaq fixture testi** qurulur.  
**Avtomat:** yanlış daş/zər/turn/replay request-lərin rəddi, iki client's state hash uyğunluğu, bir request-in 2 dəfə gəlməsi zamanı iki dəfə icra olunmaması.  
**İnsan:** 2 müştəridə eyni daş vəziyyətini izləyir.  
**Yoxdur:** chat, reytinq, kosmetika.

**P05.3 — Özəl masa: yaradılma və qoşulma.**  
**AI edir:** bir oyunçu masa yaradır, ikinci oyunçu kodla qoşulur; bir dəfəlik/vaxtı bitən linkin davranışı; gözləmə yeri; dostla reytinqsiz oyun.  
**Avtomat:** düzgün və səhv kod, qoşulmanı ləğv etmək, üçüncü şəxsin oyunçu yeri tutmaması, bağlantı gecikməsi.  
**İnsan:** iki real Android cihazı və ya cihaz + ikinci müştəri ilə yoxlama.  
**Yoxdur:** push bildiriş (əvvəl rədd edilib).

**P05.4 — Bağlantı qırılması və server bərpası.**  
**AI edir:** serverdə snapshot+jurnal; telefon şəbəkədən çıxıb qayıdanda eyni matça qayıtma; server restart üçün ayrıca fallback; vaxt məhdudiyyəti ilə qarşı tərəfin hüququ.  
**Avtomat:** airplane mode / paket itkisi simulyasiyası, iki dəfə reconnect, duplicate move, server restart, state hash.  
**İnsan:** bir cihazda interneti kəsir, qayıdır, matçın necə davrandığını görür.  
**Yoxdur:** “sonsuz reconnect” və ya yalan bərpa zəmanəti.

**P05.5 — Tamaşaçı rejimi.**  
**AI edir:** üçüncü istifadəçi yalnız `read-only` state alır; dost masasında tamaşaçı icazəsi; oyuna müdaxilə mümkün deyil.  
**Avtomat:** spectator-dan roll/move/undo request-lər 403/uyğun rədd, oyunun state-i dəyişmir; məxfilik parametrinə əməl edilir.  
**İnsan:** iki oyunçu + tamaşaçı sınağı.  
**Yoxdur:** canlı tamaşaçı reaksiyaları (rədd edilib).

**P05.6 — 1 GB server üçün qərar qapısı.**  
**AI edir:** server sahibinin razılığı ilə yalnız resurs yoxlaması, lokal və VPS profilinin müqayisəsi; xidmətin memory/disk/backup/tls ehtiyacı; POC yükü altında ölçmə və nəticə.  
**Avtomat:** monitor metrikləri (RAM peak, OOM, DB IO, latency, restart, TLS), test və backup/restore.  
**İnsan:** server həqiqətən boşdurmu, başqa servislər varmı, risk qəbul edilirmi?  
**Qərar:** `FITS_TEST_ONLY`, `NOT_FIT`, `NEEDS_MORE_DATA`. `FITS_TEST_ONLY` istehsal üçün zəmanət deyil. `NOT_FIT` olduqda mövcud serverə zorla sığışdırma, pulsuz/uyğun alternativ araşdır, istifadəçiyə açıq göstər.  
**Yoxdur:** avtomatik hosting alışı, production DB migrasiyası.

### FAZA 6 — Hesablar, dostlar və onlayn rəqabət

**P06.1 — Qonaq identifikatoru və hesabın qorunması.**  
**AI edir:** guest ID; offline lokal data; online data ilə düzgün əlaqələndirmə, hesabın bərpası; login məcburi olmadan başlanğıc.  
**Avtomat:** guest upgrade, eyni ID ilə iki cihaz, logout, token expiry, data loss, başqa hesaba yanlış bağlanma.  
**İnsan:** ilk dəfə oynamaq, hesabı bağlamaq, sonra geri qayıtmaq.  
**Yoxdur:** e-mail marketing, üçüncü tərəf sosial logini məcbur etmək.

**P06.2 — Dost siyahısı, unikal ID, online status.**  
**AI edir:** istifadəçi ID-si ilə dost tapmaq, dost əlavə etmək/silmək, onlayn görünüşü gizlətmək, hazır reaksiyalar.  
**Avtomat:** privacy, eyni ad, səhv ID, offline status, bloklanmış istifadəçiyə sorğu.  
**İnsan:** dostu ID ilə tapır, əlavə edir, status görür.  
**Yoxdur:** sərbəst chat/səsli söhbət/avtomatik push dəvət.

**P06.3 — Təsadüfi rəqib axtarışı.**  
**AI edir:** uzun/qısa və sürətli/klassik filtr; opponent selection; ilk dar reytinq aralığı, gözləmə uzananda genişlənmə; real axtarış statusu, ləğv.  
**Avtomat:** fərqli versiyalar, eyni istifadəçinin iki dəfə növbəyə girməsi, axtarış ləğvi, qonaq/ranked uyğunluğu, botu insan kimi göstərməmək.  
**İnsan:** test hesablarla real matchmaking.  
**Yoxdur:** uydurma “2000 oyunçu online” kimi rəqəmlər.

**P06.4 — Reytinqin serverdə hesablanması.**  
**AI edir:** uzun və qısa reytinq ayrılığı, skill əsaslı yenilənmə, rank ladder üçün sabit formul və versiyalaşdırma; friend/bot reytinqə təsir etməsin.  
**Avtomat:** qələbə/məğlubiyyət, təkrar result göndərmək, tərk etmə, mars/koks, ELO/Glicko və ya seçilən modelin tutarlılığı, mənfi dəyər / anormal sıçrayış.  
**İnsan:** 2–3 rank matçı sonrası nəticəni görür.  
**Yoxdur:** mövsümlük sıfırlama və ya şəhər reytinqi.

**P06.5 — Liqa pillələri və liderlər.**  
**AI edir:** Şagird/Həvəskar/Mahir/Usta/Böyük Usta; ölkə liderlər cədvəli; reytinq artım qrafiki; zərif 2D UI. Ölkə aidiyyəti üçün istifadəçinin məxfiliyə uyğun öz seçimi/uyğun etibarlı həll istifadə olunur; dəqiq GPS tələb olunmur.  
**Avtomat:** yanlış və təkrar leaderboard update, privacy, fərqli long/short rating, tipografiya.  
**İnsan:** reytinq və titul öz hesabında aydın görünür.  
**Yoxdur:** weekly open tournaments, city board.

**P06.6 — Oyun tarixçəsi və replay.**  
**AI edir:** təsdiqlənmiş turn-by-turn event-lərdən matçı bərpa; sürüşdürülən sadə replay timeline; saxta AI gediş tənqidi yoxdur.  
**Avtomat:** replay state hash, data silinməsi, unfinished match, böyük log, untrusted event injection.  
**İnsan:** əvvəlki matçın bir neçə gedişini izləyir.  
**Yoxdur:** “səhv etdin, düz gediş budur” sistemi (rədd edilib).

**P06.7 — Şikayət, bloklama, qərəzsiz cəzalar.**  
**AI edir:** uyğun olmayan ad, abuse, qəsdən çıxma, qıcıqlandırıcı stiker spamı üçün blok və report; server audit; aydın qayda və appeal/contact yolu.  
**Avtomat:** blokdan sonra dost/masa girişinin rəddi, təkrar report rate limit, təhlükəsiz log, data minimizasiyası.  
**İnsan:** blok funksiyasını yoxlayır və interfeysin qorxuducu olmadığını görür.  
**Yoxdur:** hər oyunçu hərəkətindən şəxsi profil yaratmaq.

### FAZA 7 — Kolleksiyalar, Azərbaycan dünyası və davamlı oyun

**P07.1 — Məkan xəritəsi (birinci mərhələ).**  
**AI edir:** illüstrativ Azərbaycan xəritəsi, hazır İçərişəhər, digər məkanlar üçün açılma davranışı; sadə və oyunvari naviqasiya.  
**Avtomat:** ekran ölçüləri, xəritədə yanlış keçid, asset memory, progress saxlanması.  
**İnsan:** xəritədən yer seçir və ana masaya qayıdır.  
**Yoxdur:** bütün regionları eyni anda hazırlamaq.

**P07.2 — İkinci məkan: Şəki.**  
**AI edir:** eyni art Bible ilə şəbəkə işığı, ayrı masa dekoru və az hərəkət; texniki sistem təkrar istifadə olunmalıdır.  
**Avtomat:** source/data driven scene skin, iki məkan arasında gediş/score dəyişməz, resurs istifadəsi.  
**İnsan:** iki məkanın fərqli, amma eyni oyunun parçası hiss etdirdiyini yoxlayır.  
**Yoxdur:** NPC dialoqları və hekayə.

**P07.3 — Bulvar, Lahıc və digər seçilmiş məkanların hissə-hissə art paketi.**  
**AI edir:** hər yeni məkan üçün yalnız **bir asset bundle**; ayrıca mini-animasiya, uyğun musiqi, gündüz/gecə, xüsusi masa materialı. Hər məkan ayrıca təsdiqlənir.  
**Avtomat:** performans, asset author/license, fərqli telefonların RAM yüklənməsi, gediş sayının dəyişməməsi.  
**İnsan:** hər məkanı ayrı təsdiqləyir.  
**Yoxdur:** hamısının bir mesajla istehsalı.

**P07.4 — Avatarlar, çərçivələr və stikerlər.**  
**AI edir:** ilk kiçik, vahid üslublu avatar dəsti, aksesuar/çərçivə, animasiyalı reaksiyalar və “Halal olsun!”, “Bir də?”, “Əla gediş!” kimi idarə olunan ifadələr. Çayxana fonunda səssiz personajlar yalnız atmosferdir.  
**Avtomat:** mətnin uyğunluğu, sticker spam limiti, avatar sprite keyfiyyəti, qrafik performans, bloklama.  
**İnsan:** istifadəçi olaraq uyğun avatar seçir, reaksiya verir.  
**Yoxdur:** maskot, yüklənən profil fotoşəkli, NPC tanışlığı.

**P07.5 — Məkan/taxta/zər kolleksiyası və albom.**  
**AI edir:** az sayda əşya ilə başlanan kataloq; albomda yaxınlaşdırma, əl işi 2D görünüş, equipped state, dekor açılma qaydası.  
**Avtomat:** inventar dublikatı, itmə, böyük asset ölçüsü, seçimdən sonra matç qaydası dəyişməz.  
**İnsan:** taxtanı dəyişir, zərə yaxın baxır, seçimin mətninin aydınlığını görür.  
**Yoxdur:** hələ real pul checkout.

**P07.6 — Tapşırıqlar və uğurlar.**  
**AI edir:** əvvəlcədən təsdiqlənmiş gündəlik/həftəlik tapşırıq, qələbə nailiyyətləri, ardıcıl oyun günləri, gündəlik giriş mükafatı və titullar; cəza/təzyiq yoxdur.  
**Avtomat:** bir tədbirə 2 dəfə mükafat verməmək, saat qurşağı sərhədləri, server təsdiqi, offline sync.  
**İnsan:** bir tapşırıq tamamlayır, nə qazanıldığını anlayır.  
**Yoxdur:** mövsümlük battle pass və gündəlik seriya itirənə cəza.

**P07.7 — Bir virtual oyun xalı və çarşı.**  
**AI edir:** yalnız kosmetik alış üçün bir balans; balansı dəyişən əməliyyat serverdə ledger vasitəsilə; isti “çarşı” UI, önizləmə.  
**Avtomat:** double spend, mənfi balans, təkrar request, inventory/ledger bərabərliyi, client hacking.  
**İnsan:** təcrübə xalından bir kosmetika alır, geri qayıdır, inventarda görür.  
**Yoxdur:** mərc və pullu üstünlük.

**P07.8 — Premium kolleksiyalar və hədiyyələr.**  
**AI edir:** real mağaza ödənişi əvvəl `sandbox/mock`; satılan item yalnız kosmetik; gift transfer təhlükəsiz, dublikat, geriödəmə şərtləri. Apple/Google uyğunluğu dəqiq yoxlanmadan canlı satış açılmır.  
**Avtomat:** receipt validation (sınaq), duplicate purchase, refund, user identity, item gift double-transfer.  
**İnsan:** sandbox sınağını görür, pul alınmır.  
**Yoxdur:** real pul aktivləşdirməsi ayrıca açıq icazəsiz.

**P07.9 — Mövsümi Azərbaycan tədbirləri və xəbər lövhəsi.**  
**AI edir:** Novruz və digər mədəni bayram dekorları, vaxtlı kosmetik mövzu, sakit xəbərlər lövhəsi; ranking sıfırlanmır.  
**Avtomat:** yanlış tarix/vaxt, tədbir bitəndə dərhal matç pozulmaması, ekranın reklam panelinə çevrilməməsi.  
**İnsan:** xüsusi mövzunu açır, nərd oyunu arxa planda itmir.  
**Yoxdur:** mövsümlük reytinq yarışı.

**P07.10 — Könüllü mükafat reklamı — yalnız gələcək yeniləmə.**  
**AI edir:** təsdiqlənmiş roadmap elementini sənədləşdirir, ilk ictimai build-də tam deaktiv qalır; sonra istifadəyə verilərsə reklam SDK-ları məxfilik/uşaq qaydalarına uyğun ayrıca bilet və test tələb edir.  
**Avtomat:** ilk yayımlanan build-də reklam SDK-sı/çıxış nöqtəsi yoxdur və ya deaktiv; reklam olmasa oyun tam işləyir.  
**İnsan:** reklam görmür.  
**Yoxdur:** erkən reklam optimizasiyası.

### FAZA 8 — Qapalı beta və eyni vaxtlı mobil buraxılış

**P08.1 — Android real cihaz sınağı.**  
**AI edir:** fərqli ekran/Android versiyası matrisi, APK/AAB, crash/log, animasiya/CPU/RAM/batareya, toxunma, orientation, save/reconnect. ADB screenshot/logcat və mövcud GUI imkanlarından istifadə edir.  
**Avtomat:** clean build + EditMode/PlayMode, en az bir fiziki telefon test qeydiyyatı (insan cihazı qoşarsa), xətaların siyahısı.  
**İnsan:** tam bir bot və onlayn matç oynayır.  
**Yoxdur:** bütün Android cihazlarında işlədiyi iddiası.

**P08.2 — iOS build və fiziki iPhone sınağı.**  
**AI edir:** dəstəklənən macOS/Xcode və ya bulud build marşrutunu seçir; signing/provisioning tələbini və xərci əvvəl göstərir; iPhone ekran ölçüləri, cihazın sadə toxunuş/səs testi.  
**Avtomat:** real iOS build log və imzalanma; iPhone device run yalnız həqiqətən mümkün olduqda.  
**İnsan:** real iPhone-da nərd oynayır və ekran çevrilməsini yoxlayır.  
**Yoxdur:** Android build nəticəsi ilə “iOS da test edildi” iddiası.

**P08.3 — Closed beta və ölçmə.**  
**AI edir:** beta tapşırıq ssenariləri, bug forması, qonaq/multiplayer/account/privacy, ilkin retention/first-match completion; məlumatların minimum toplanması.  
**Avtomat:** kri̇tik test matrisi, matç nəticələrinin server/kli̇ent uyğunluğu, çöküşlər, ədalət, performance.  
**İnsan:** uyğun yaş və hüquqi şərtlərlə seçilmiş testçilər real rəylər verir.  
**Yoxdur:** beta müsbət görünür deyə production-u özü aktivləşdirmək.

**P08.4 — Dizayn və məhsul keyfiyyət qapısı.**  
**AI edir:** screenshot/motion/audio audit; AI-slop checklist; Azərbaycan hərfləri; oyun açılışı; 1–2 tap UX; reklam yoxdur; 60 FPS hədəfi ölçülür.  
**Avtomat:** hər ekranda texniki test, kritik xəta 0 hədəfi, visual artifact lisenziyası, iOS+Android release candidate.  
**İnsan:** ana ekran, portret/landşaft matç, nəticə və çarşı hissinin qəbul edilməsinə açıq qərar verir.  
**Yoxdur:** sırf tarixə görə keyfiyyətsiz buraxılış.

**P08.5 — Mağaza uyğunluğu və birlikdə buraxılış.**  
**AI edir:** Google/Apple metadata, ikon/screenshot, privacy policy, support contact, account deletion, report/block, age ratings, test tracks; mağazaların son qaydalarını yenidən oxuyur. Apple hesabı, sertifikat və mağaza ödənişi üçün hüquqi sahibin təsdiqini alır. Google şəxsi hesabı üçün 12/14 closed test şərti tətbiq olunursa qeydiyyatı təşkil edir.  
**Avtomat:** iki imzalı release build və store checks; heç bir gizli funksiyanın olmaması, satınalma testləri.  
**İnsan:** **iki mağazada yayımlanmanı ayrıca açıq şəkildə təsdiqləyir**. İcazəsiz publish yoxdur.

**P08.6 — Yayımdan sonrakı təhlükəsiz artım.**  
**AI edir:** crash/latency/retention/feedback üzrə ölçülən dəyişiklik təklifləri; qəbul edilmiş 87 backlogin qalan hissələrini mikro-biletlərə bölmə; xərclər və uğur ölçümü.  
**İnsan:** hər yeni buraxılışı ayrıca təsdiqləyir.  
**Yoxdur:** “artıq özüm əlavə edim” rejimi.

---
## VII. TESTLƏRİN DƏQİQ SİSTEMİ — AGENT ÖZÜNÜ NECƏ YOXLAYIR?

### VII.1. Test növləri və nə vaxt işə düşür

| Test | Nəyi ölçür | Tələb edilən sübut |
|---|---|---|
| C# unit/EditMode | Qayda və bot məntiqi, vəziyyət keçidləri | Test adı, nəticə XML və fail log |
| Unity PlayMode | UI, scene və qarşılıqlı interaksiyalar | Test log, Game View screenshot |
| Fixture / differential | Client və server qaydalarının eyni cavabı | `ruleset_version`, seed, input, expected state hash |
| Simulyasiya/fuzz | Minlərlə təsadüfi qanuni/mümkün olmayan gediş | Seed, uğursuz nümunələr, illegal move sayısı |
| Build smoke | Həqiqi Android/iOS build | Build log, ID, artifact, açılış nəticəsi |
| GUI click/touch | Düymə, zər, daşı sürüşdürmək, geri, orientation | Ekran yazısı / test screenshot / addımlar |
| Multiplayer integration | İki cihaz, reconnect, turn order, spectator | Server log, client log, state parity |
| Security | Duplicate rewards, auth, privacy, moderation | Rejection nəticələri və audit |
| Accessibility | Touch ölçüsü, mətn kontrastı, Azərbaycan hərfləri, səs söndürmə | Ölçü və screenshot |
| Performance | Frame time/FPS, RAM, yükləmə, batareya | Real cihaz profili, ölçülmüş metrik |
| Regression | Əvvəl təsdiqlənmiş partın işləməsi | Baseline ilə müqayisə |

**Vacib:** “Test yazdım” başqa, “test işləyib keçdi” başqa şeydir. Agent icra edə bilmədiyi testləri `NOT_RUN` kimi qeyd edir. `NOT_RUN` avtomatik `PASS` sayılmır. “Ekran düzgün görünür” iddiası üçün faktiki render edilmiş screenshot və ya insan baxışı lazımdır. Yalnız mənbə koda baxmaq vizual test deyil.

### VII.2. Qayda testlərinin minimum matrisi

**Ümumi:** başlanğıc mövqe, 15 daş invarianti, 24 xana, növbə sahibi, yüksək zər prioriteti (müvafiq variant), qoşa zərin dörd hissəsi, qismən zər istifadə, bütün zərlər mümkün olduğu qədər istifadə, qanunsuz gediş, mərhələ keçidi, matç sonu, seriya, undo zamanı state və zərlərin bərpası, save/load.

**Uzun nərd:** başdan 1 daş limiti, seçilmiş 3–3/4–4/6–6 istisnası, altılıq blokun düzgün tətbiqi, rəqib daşının olduğu xananın qapanması, başlanğıc zəri, evə toplama/daşı çıxarma, mars.

**Qısa nərd:** 2/5/3/5 başlanğıc, vahid blot vurulması, iki və daha çox daşın bloklaması, bar-dan giriş, bar doluykən məcburi prioritet, bearing off overshoot, gammon/backgammon şərtləri.

**Sabit invariantlar:** 15 daş heç yerdə itmir və çoxalmır; zər limitindən artıq hərəkət yoxdur; server client-in özbaşına göndərdiyi random nəticəni qəbul etmir; qələbə yalnız qayda motoru ilə yaranır; tərk etmə/reconnect nəticəni iki dəfə yazmır.

### VII.3. Həqiqi GUI yoxlaması üçün agent qaydası

1. Unity Editor-un Game View görüntüsünü və cari cihaz nisbətini aç.
2. Agentin real GUI/computer-use çıxışı varsa düyməyə toxunma və drag hərəkətlərini icra et; klikdən əvvəl/sonra screenshot saxla.
3. Android bağlıdırsa ADB ilə build quraşdır, logcat/screenshot əldə et; mümkün olduğu halda automatisasiya çərçivəsi ilə tap/click sınağı et.
4. GUI və ya cihaz çıxışı yoxdursa alternativ test düzəlt (PlayMode UI tests), amma bunu fiziki insan testi kimi təqdim etmə; `NEEDS_HUMAN_DEVICE_TEST` qeyd et.
5. İnsan 3–5 sadə addımı sınaqdan keçirdikdən sonra nəticə verir.

Unity Test Framework-un rəsmi CLI parametrləri `-runTests`, `-batchmode`, `-testPlatform EditMode/PlayMode`, `-testResults` kimi variantları dəstəkləyir; agent istifadə etdiyi Unity versiyası üçün rəsmi sintaksisi təsdiqləyir. ADB screenshot da rəsmi Android alətidir. Agent elə bir tool-un uğurunu iddia etməməlidir ki, ona real çıxışı yoxdur.

### VII.4. İnsan qəbul ssenarisinin nümunəsi

**Part P02.4 üçün:**
1. Oyunu Android telefonda aç; bir daşı sürüşdür və qanuni xanaya qoy. Gözlənilən: daş bir dəfə köçürülür.
2. Həmin daşı qadağan olunmuş xanaya dart. Gözlənilən: daş geri dönür, növbə pozulmur.
3. Bir qanuni gedişdən sonra “geri al” et. Gözlənilən: daş və zər hüququ bərpa olur.
4. Növbəni yekunlaşdır, sonra “geri al” et. Gözlənilən: rəqibin növbəsində geri alma yoxdur.
5. Ekranı çevir. Gözlənilən: eyni vəziyyət qalır.

**Part P05.4 üçün:**
1. İki testçi dost masasına girsin.
2. Birinci oyunçunun internetini kəs.
3. İkinci oyunçunun statusu anlaşılır qalsın, saxta növbə başlamasın.
4. Birinci oyunçunu yenidən qoş.
5. Hər iki cihazda daş/zər/taymer vəziyyəti uyğun olsun; qayda nəticəsi dəyişməsin.

### VII.5. Xəta dərəcələri

- **Blocker:** oyun açılmır, müştəri state-i pozur, server qələbəni saxtalaşdırmağa icazə verir, məxfi məlumat açılır, data itir. Buraxılış qadağandır.
- **Critical:** çox tez-tez crash, əsas nərd qaydası səhvi, onlayn resync ciddi pozulur, satınalma iki dəfə yazılır. Bilet təsdiqlənmir.
- **Major:** müntəzəm yanlış toxunma, ekran kənarında oyunun görünməməsi, təkrar effektin oyunu ləngitməsi. Aid olduğu part bağlanmır.
- **Minor:** kosmetik mikro-kölgə, mətnin kiçik düzülüş fərqi; səviyyə və vizual standartdan asılıdır, qəbuldan əvvəl qeyd olunur.

---

## VIII. FAYL VƏ REPO QURULUŞU — AI-NİN İTİRMƏDİYİ KONTEKST

```
NARD/
  AGENTS.md                  # Agentin dəyişməz qaydaları
  README.md                  # Açılış, pulsuz inkişaf, ilkin tələblər
  GDD.md                     # 87/20 qərarın dəqiq mənbə sənədi
  docs/
    MASTER_PLAN.md           # Bu plan
    APPROVED.md              # A01–A87, status və part əlaqəsi
    REJECTED.md              # R01–R20, qəti DO NOT BUILD
    STATE.json               # Yeganə aktiv bilet və təsdiq statusu
    DECISIONS.md             # Fakt və yeni fərziyyələri ayırır
    RULES_LONG.md
    RULES_SHORT.md
    RULESET_VARIANTS.md
    ART_BIBLE.md
    ASSET_MANIFEST.csv       # Hər faylın hüququ, mənbə/generatoru
    TEST_MATRIX.md
    COSTS.md                 # 0 AZN olan və məcburi ödənişlər
    SECURITY.md
    RELEASE_CHECKLIST.md
    reports/
      P00.1.md
      P00.2.md
      ...
    screenshots/
      ...
  unity/
    Assets/Game/Rules/
    Assets/Game/Bots/
    Assets/Game/Scenes/
    Assets/Game/UI/
    Assets/Game/Animation/
    Assets/Game/Audio/
    Assets/Game/Network/
    Assets/Game/Tests/
    Assets/Game/Art/
  nakama/
    server/
    fixtures/
    docker-compose.dev.yml
    tests/
  art/
    originals/              # Mənbə, layered konsept
    exported/               # Optimizə edilmiş real oyun asseti
  scripts/
    validation/
    build/
  CHANGELOG.md
```

**Sürətli bərpa:** Söhbət və AI konteksti dəyişsə belə agentin işi `STATE.json`, GDD, approved/rejected və son part hesabatından bərpa olunur. AI əvvəl görülmüş işi yenidən sıfırdan yazmır. Nə baş verdiyini təxmin etmir, faylları oxuyur.

**Part bitəndə saxlanan nümunə status:**

```json
{
  "active_part": "P02.4",
  "status": "WAITING_FOR_HUMAN_REVIEW",
  "last_human_approved_part": "P02.3",
  "build_id": "android-dev-local-014",
  "test_report": "docs/reports/P02.4.md",
  "next_part": "P02.5",
  "next_part_authorized": false,
  "approved_feature_count": 87,
  "rejected_feature_count": 20
}
```

**Status faylına heç vaxt parol, API token, şəxsi cihaz ID-si və istifadəçinin həssas məlumatı yazma.**

---

## IX. HAZIR ASSET TAPILMAYANDA AGENTİN TƏTBİQ EDƏCƏYİ QAYDA

Əvvəl **öz yarada bildiyi** yolu yoxlasın. Həmin assetin Unity-də vector, shader və ya sadə 2D sprite kimi daha yaxşı yaradıla biləcəyini təhlil etsin. Asset haradasa açıq mənbədə varsa uyğun lisenziya və real sənədləşdirilmiş hüquqla götürsün. Rəng/üslub/GDD ilə uyğun gəlməyən “tapdım, istifadə etdim” qadağandır.

**İstifadəçiyə görüntü lazım gələndə agentin hesabatı bu formatdadır:**

```
ASSET REQUEST AR-004
İş yeri: İçərişəhər ana ekran fonu
Niyə lazımdır: Ana masanın arxa mühiti
Yaradılma imkanı: AI agenti bu mühitdə image_gen alətinə çıxışı olmadığını təsdiq etdi
Lazım olan fayl: PNG (mümkünsə layers), mətnsiz; landscape master 2048x1152;
  görünən safe-zone üçün ayrıca eskiz; asset resolution testlə tənzimlənəcək
Gözlənilən tərz: yumuşaq əl işi 2D illüstrasiya, qoz masa, isti işıq, ölçülü milli ornament
Görünməsin: qara korporativ UI, 3D, neon, AI glows, logo, yanlış mətn, watermark
PROMPT: [tam aşağıdakı mətn]
NEQATİV TƏLƏBLƏR: [tam aşağıdakı mətn]
İstifadəçidən tələb: yalnız uyğun təsviri yaradıb PNG göndərmək
Növbəti addım: asseti oyuna yerləşdir və eyni partın testini təkrar et
```

**Misal — vizual asset prompt** (yalnız belə assetə ehtiyac yarananda istifadə olunmalıdır):

> A handcrafted 2D background illustration for an original Azerbaijani mobile backgammon game. A warm sunny courtyard inspired by historic Baku Icherisheher, quiet stone walls, subtle traditional architectural details, an inviting walnut wooden table, a small armudu tea glass, gentle natural daylight, warm cream and muted terracotta palette with restrained turquoise accents, subtle natural paper texture, restrained hand-painted brushwork, cozy authentic local atmosphere, clear focal space reserved for the playable backgammon board in the center, designed as a premium stylized 2D mobile game environment, coherent art direction, clean silhouettes, readable on a small phone screen, no text, no interface, no characters blocking the board, no watermark, no 3D rendering, no black corporate app style, no neon, no glossy plastic, no exaggerated fantasy lighting, no generic AI illustration look.

**Texniki qeyd:** Şəkildəki taxta və zərləri birbaşa arxa plana yapışdırmaq olmazsa, arxa planı ayrı, taxta/daş/zəri ayrıca şəffaf layer-də yaratmaq daha uyğun ola bilər. AI bu seçimi art pipeline üzrə öncədən verməlidir. Həmin nümunə prompt icra bileti olmadan kütləvi şəkil yaratmağa icazə vermir.

**Yaratma sonrası test:** mərkəzdə boş sahə var? Oyundakı taxta/daşlar oxunur? Ton/işıq digər asset-lərlə uyğundur? Şəkildə süni/yanlış əl, qəribə ornament, mətn qalıqları var? Material mobil exportda piksellənir? Fontlar ayrıca Unity-də işləyir? Hüquqi qeyd var? İnsan bəyənmədi — cari asset yenidən işlənir, digər parta keçilmir.

---

## X. QƏBUL MEYARLARI VƏ KEYFİYYƏT SƏRHƏDLƏRİ

### X.1. First Play gate

- App açılır, çöküş olmur, uzun loading/poster ilə oyuna giriş bağlanmır.
- İlk matça 1–2 toxunuş yolu var; qonaq kimi oynamaq mümkündür.
- Uzun/qısa nərd qanuni gedişlər və tam matç sonu ilə işləyir.
- Hər iki orientation-da daş, zər, UI elementləri aydın və toxunulandır.
- Əsas mətnlər düzgün Azərbaycan dilindədir; ə/ğ/ı/İ/ö/ş/ç/ü qüsursuzdur.

### X.2. Visual & feel gate

- Bir-birinə uyğun, isti Azərbaycan çayxanası vizual dili.
- Ana ekran tətbiqin dashboard-u kimi yox, oyunun interaktiv giriş səhnəsi kimi görünür.
- 2D material, zər/daş səsi və qısa animasiyalar real masa hissinə xidmət edir.
- Neon, qara korporativ stil, plastik AI slop, hədsiz dekor yoxdur.
- Animasiya oyunçunun qərarını gecikdirmir, parametrlə azaldılır və ya söndürülür.
- Yaşadığımız “oyun hissi” insan testində təsdiqlənməyənədək art partı bağlanmır.

### X.3. Multiplayer gate

- Server zər və gedişin son qərarvericisidir.
- İki müştərinin vəziyyəti hər təsdiqlənmiş növbədən sonra uyğundur.
- Qanunsuz gediş, saxta reward/rating, duplicate request rədd edilir.
- Reconnect, disconnect, server restart sınağı və açıq fallback siyasəti var.
- Tamaşaçı hərəkət edə bilmir; blok/məxfilik qüvvədədir.
- Hər iki nərd rejiminin reytinqi ayrıdır; özəl masa və bot nəticəsi reytinqə təsir etmir.

### X.4. Release gate

- Android və iOS real cihaz testləri sənədləşib; yalnız build uğuru test hesab edilmir.
- Qapalı beta və dizayn keyfiyyət yoxlaması keçib.
- Kritik qayda, hesab və ödəniş xətaları: **0 açıq blocker/critical**.
- Şikayət/blok, hesab silinməsi, məxfilik və mağaza tələbləri nəzərə alınıb.
- İlk ictimai build **reklamsızdır**; mübahisəli monetizasiya rejimi açılmayıb.
- İki mağaza yayımlaması üçün insanın ayrıca açıq icazəsi və uyğun hüquqi developer hesabı var.

---

## XI. İNSAN TESTİNDƏ ŞİKAYƏTLƏR NECƏ EMAL EDİLMƏLİDİR?

**İstifadəçi deyir: “Bəyənmədim, AI kimi görünür.”** Agent müdafiəyə keçmir; niyə belə görünə biləcəyini vizual meyarlara bölür: ümumi həddindən artıq parıltı, bir-birinə zidd illüstrasiyalar, yanlış perspektiv, süni naxış, mobil ekran sıxlığı. Əvvəlki təsdiqlənmiş Art Bible-ı dəyişdirmədən cari asset/UI-ni düzəldir; yeni screenshot verir və istifadəçi təsdiqini gözləyir.

**“Bu işləmir.”** Agent əvvəlcə konkret reproduksiya addımlarını toplamağa çalışır; istifadəçi videosu/screenshot varsa ondan istifadə edir; bug üçün əvvəl uğursuz test yazır; düzəldir; testin indi keçdiyini göstərir; regression işlədir. Digər feature-a keçmir.

**“Çox yavaşdır.”** Agent əvvəl hansı mərhələdə yavaş olduğunu ölçür (yükləmə, animasiya, bot düşünməsi, şəbəkə). Fakt olmadan “server yavaşdır” demir. Ölçmə ilə ən böyük bottleneck-i aradan qaldırır.

**“Təkcə bu hissə gözəl olsun, qalanını sonra.”** Plan üzrə yalnız cari bileti cilalayır; 20 digər sistemi arxa planda başlatmır.

**“AI əvvəl kodu silib başdan yazsın.”** İlk cavab root cause, geri çevrilmə riski və dəyişiklik təsiri olmalıdır. Həqiqətən zəruridirsə kiçik, testlə qorunan refactor; qlobal rewrite avtomatik deyil.

**“1 GB VPS-ə hər şeyi yerləşdir.”** Agent əvvəl real RAM və risk ölçüsünü göstərir; sistem qeyri-sabit olacaqsa açıq şəkildə “bu server istehsal üçün uyğun deyil” deyir. İstifadəçini razı salmaq üçün yalan “problem yoxdur” demir.

---

## XII. RİSK REYESTRİ VƏ OPPORTUNITY COST

| Risk | Nə itiririk? | Preventiv qərar |
|---|---|---|
| Çox böyük scope | Aylarla görünməyən nəticə, fokus itkisi | Bir dəfə bir mikro-bilet, 87 backlog |
| Yanlış nərd qaydası | Etibar və reytinq reputasiyası | Pure rules, fixtures, real yerli oyunçu testi |
| AI slop vizual | Oyunu ucuz göstərən təəssürat | Art Bible + bir asset/səhnəlik insan qapısı |
| Agentin “işləyir” deyib test etməməsi | Səhv inteqrasiyalar | Real log + screenshots + device test |
| 1 GB VPS-ə zorla yerləşdirmə | OOM, məlumat itkisi, oynanılmayan online | Lokal POC + real yük ölçməsi |
| Eyni qaydanın C# və TS versiyalarında fərqlənməsi | Online cheating/bug | Eyni fixture, differential tests |
| iOS-u sona saxlamaq | Son dəqiqə cihaz/build/hesab bloklanması | Erkən minimal iOS build proof, rəsmi yayımlama sonda |
| Asset lisenziyasının bilinməməsi | Hüquqi/reputasiya xərci | Source/license registry |
| Pulları erkən yığmaq | UX etibarına xələl | İlk ictimai build reklamsız, əvvəl əsas gameplay |
| Söhbət kontekstinin itməsi | Təkrar qurma və risk | State/decisions/tests/art Bible Git-də |

**Birinci prinsip:** ən böyük leverage növbəti 10 vizual effektdə deyil, ilk düzgün nərd matçı + xoş mobil hiss + etibarlı dostla online təcrübəsindədir. Bir partda “çox şey” etmək əvəzinə, azı yaxşı və ölçülən etmək lazımdır.

---

## XIII. SON STATUS VƏ İLK HƏQİQİ ADDIM

**Hazırkı layihə vəziyyəti:** sənədləşmə və qərarlar tamamlanıb; icra sübutu hələ yoxdur. Yəni “oyun hazırlanıb” demirik.

**İLK TAPŞIRIQ:** `P00.1 — Mühit auditi`, yalnız *oxu/ölçmə*. AI kompüterdə Unity/Antigravity/plugin/Android build alətləri nə vəziyyətdədir deyə baxır, heç bir əlavə sistemi özbaşına quraşdırmır, nəticə və plan verir. İstifadəçi təsdiqlədikdən sonra `P00.2` başlanır. Bundan əvvəl serverə və ya iOS ödənişlərinə ehtiyac yoxdur.

**Agent üçün yekun əməliyyat müqaviləsi:** “Bu GDD və Master Plan-a əməl et. Yalnız istifadəçinin son açıq şəkildə təsdiq etdiyi partı icra et; əgər aktiv part hələ təsdiqlənməyibsə onu test edib insan üçün hesabatla dayandır. Bütün kod/asset/UX qərarlarında təsdiqlənmiş 87 funksiyaya və 20 qadağaya riayət et. Hər partı özün yoxla, sübut göstər, istifadəçiyə 3–5 sadə manual test ver, açıq ‘keç’ icazəsi olmadan növbəti parta keçmə. İmkanın olmayan aləti işlətdim demə. Pullu xidmət, production server/mağaza, məlumatların ötürülməsi və böyük refactor üçün ayrıca icazə al. Söz və görüntü yox, ölçülən işlək nəticə ver.”

---

## XIV. YOXLADILMIŞ RƏSMİ MƏNBƏLƏR

- Unity Antigravity plugin: https://docs.unity.com/en-us/ai/unity-plugin/antigravity
- Unity plugin haqqında: https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin
- Unity Personal qiymət şərtləri: https://unity.com/products/pricing-updates
- Nakama Docker development nümunəsi (PostgreSQL): https://heroiclabs.com/docs/nakama/getting-started/install/docker/
- Nakama Linux və DB statusu: https://heroiclabs.com/docs/nakama/getting-started/install/linux/
- Nakama authoritative turn-based: https://heroiclabs.com/docs/nakama/concepts/multiplayer/authoritative/
- Unity test CLI: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/test-framework-introduction/reference-command-line
- Android ADB: https://developer.android.com/tools/adb
- Unity iOS build automation və signing: https://docs.unity.com/en-us/build-automation ; https://docs.unity.com/en-us/build-automation/sign-build-artifacts/sign-an-ios-application
- Apple developer üzvlük/yaş: https://developer.apple.com/help/account/membership/program-enrollment
- Google developer yaş/ödəniş: https://support.google.com/googleplay/android-developer/answer/6112435
- Google qapalı test: https://support.google.com/googleplay/android-developer/answer/14151465
- Apple App Review: https://developer.apple.com/app-store/review/guidelines/

---

## XV. İL ƏVVƏLİ MƏNBƏ GDD-DƏN SÖZBƏSÖZ QƏRAR İNVENTARI

Aşağıdakı inventar əvvəlki Final GDD sənədinin qərar siyahısından alınır. Buradakı bütün maddələr **məhsulun uzunmüddətli planında saxlanır**; onların birinci buraxılışda tamamlanması tələb deyil. İnkişaf mikro-biletləri bu qərarları hissə-hissə həyata keçirir. Rədd siyahısındakı bəndlər icazəsiz geri gətirilmir.

### A01–A87 qəbul edilmiş xüsusiyyətlər

**Bu siyahı mənbə qərardır; hər maddə ayrıca backlog ID alır.** Funksiyanın qəbul edilməsi ilk buraxılışa daxil olacağı demək deyil.

### A01–A10 | Mərhələ 1 — oyun özəyi

1. **A01** Uzun nərd
2. **A02** Qısa nərd
3. **A03** Dostla onlayn oyun
4. **A04** Təsadüfi onlayn rəqib
5. **A05** Offline bot
6. **A06** Reytinq və liqalar
7. **A07** Çayxana masaları
8. **A08** Taxta və zər kolleksiyası
9. **A09** Gündəlik tapşırıqlar
10. **A10** Matç təkrarı

### A11–A18 | Mərhələ 2 — rəqabət və rahatlıq

11. **A11** Sürətli və klassik matçlar
12. **A12** Azərbaycan məkanları
13. **A13** Dostluq və revanş sistemi
14. **A14** Avatarlar və profillər
15. **A15** Uğurlar və mükafatlar
16. **A16** Matçı izləmə rejimi
17. **A17** İnteraktiv nərd dərsləri
18. **A18** Best of 3 və Best of 5

### A19–A23 | Mərhələ 3 — geri dönüş

19. **A19** Qonaq kimi oynamaq
20. **A20** Mövsümi Azərbaycan tədbirləri
21. **A21** Canlı ifadələr və reaksiyalar
22. **A22** Şəxsi oyun tarixçəsi
23. **A23** Həftəlik missiyalar

### A24–A31 | Mərhələ 4 — canlı oyun dünyası

24. **A24** Canlı ana ekran
25. **A25** Gündüz və gecə atmosferi
26. **A26** İllüstrativ Azərbaycan xəritəsi
27. **A27** Xüsusi matç başlanğıcı
28. **A28** Çayxana personajları (atmosfer, dialoq yox)
29. **A29** Xarakterli qələbə səhnələri
30. **A30** Canlı mühit səsləri
31. **A31** Masanın atmosferini dəyişmək

### A32–A33 | Mərhələ 5 — inkişaf

32. **A32** Usta titulları
33. **A33** Ardıcıl oyun günləri

### A34–A40 | Mərhələ 6 — iqtisadiyyat

34. **A34** Oyun daxili xal sistemi
35. **A35** Kosmetik mağaza
36. **A36** Premium kosmetik kolleksiyalar
37. **A37** Gündəlik giriş mükafatı
38. **A38** Könüllü reklamla mükafat (ilk buraxılışda söndürülür)
39. **A39** Dostlara hədiyyə göndərmək
40. **A40** Kolleksiya albomu

### A41–A48 | Mərhələ 7 — mobil UX

41. **A41** Bir toxunuşla oyuna başlamaq
42. **A42** İlk oyunda ağıllı kömək
43. **A43** Qısa interaktiv açılış
44. **A44** Matç zamanı üfüqi ekran
45. **A45** Yarımçıq matça qayıtmaq
46. **A46** Rahatlıq parametrləri
47. **A47** Daşları sürüşdürərək oynamaq
48. **A48** Şaquli matç rejimi də olsun

### A49–A55 | Mərhələ 8 — taxta mexanikası

49. **A49** İnteraktiv zər atışı
50. **A50** Mümkün gedişlərin göstərilməsi
51. **A51** Gedişi geri almaq (növbə yekunlaşmazdan əvvəl)
52. **A52** Növbə vaxtı və vizual taymer
53. **A53** Taxtanın rahat görünüşü
54. **A54** Sürətli daş hərəkəti
55. **A55** Xüsusi matç nəticəsi

### A56–A62 | Mərhələ 9 — onlayn təhlükəsizlik

56. **A56** Unikal oyunçu ID-si
57. **A57** Dostların onlayn vəziyyəti
58. **A58** Otaq kodu və dəvət linki
59. **A59** Şəxsi masa parametrləri
60. **A60** Şikayət və bloklama sistemi
61. **A61** Ədalətli oyun nəzarəti
62. **A62** Gizlilik və hesab təhlükəsizliyi

### A63–A68 | Mərhələ 10 — məkanlar

63. **A63** Hər məkanın özünəməxsus masası
64. **A64** Kiçik canlı məkan detalları
65. **A65** Məkanları irəlilədikcə açmaq
66. **A66** Məkanlara məxsus kiçik hadisələr
67. **A67** Kolleksiyaya yaxından baxmaq
68. **A68** Hər məkanın öz musiqisi

### A69–A74 | Mərhələ 11 — avatarlar

69. **A69** Fərqli illüstrativ avatar dəstləri
70. **A70** Avatar aksesuarları
71. **A71** Animasiya edilmiş avatar reaksiyaları
72. **A72** Profil çərçivələri və fonları
73. **A73** Xüsusi zər atma effektləri
74. **A74** Azərbaycan üslubunda stikerlər

### A75–A81 | Mərhələ 12 — naviqasiya

75. **A75** İnteraktiv ana masa
76. **A76** Oyun üslubunda naviqasiya
77. **A77** Oyun rejimi seçimi
78. **A78** İnteraktiv oyunçu profili
79. **A79** Çarşı üslubunda mağaza
80. **A80** Canlı ekran keçidləri
81. **A81** Oyun daxilində xəbər lövhəsi

### A82–A87 | Mərhələ 13 — rəqabət dəqiqləşdirməsi

82. **A82** Bacarıq əsaslı reytinq
83. **A83** Balanslı rəqib seçimi
84. **A84** Daimi liqa pillələri
85. **A85** Ölkə üzrə liderlər cədvəli
86. **A86** Yarış qaydaları və cəzalar
87. **A87** Reytinq inkişaf qrafiki

### R01–R20 rədd edilmiş xüsusiyyətlər — DO NOT BUILD

1. **R01** Səsli söhbət
2. **R02** Nərd klubları
3. **R03** Səhvlərdən öyrənmə / AI gediş təhlili
4. **R04** Qələbə kartının paylaşılması
5. **R05** Mövsümlük reytinq yarışı
6. **R06** Əzəli rəqib sistemi
7. **R07** Canlı tamaşaçı reaksiyaları
8. **R08** Günün masası
9. **R09** Dostdan oyun dəvəti push-bildirişi (özəl masa linki RƏDD DEYİL)
10. **R10** Virtual jetonla mərc
11. **R11** Mövsümlük mükafat yolu / battle pass
12. **R12** Son zərlərin tarixçəsi paneli (matç təkrarı qalır)
13. **R13** Matç daxilində sərbəst yazılı söhbət
14. **R14** Yerli personajlarla tanışlıq dialoqları
15. **R15** Məkanların kiçik hekayələri
16. **R16** Oyunun öz maskotu
17. **R17** Öz şəklini avatar etmək
18. **R18** Sadə/genişləndirilmiş interfeys rejimləri
19. **R19** Şəhərlər üzrə reytinq
20. **R20** Həftəlik açıq turnirlər

**Əhəmiyyətli fərqlər:** A16 matçı izləmə mövcuddur, R07 tamaşaçı reaksiyası yoxdur. A21 əvvəlcədən hazırlanan ifadələr var, R13 sərbəst chat yoxdur. A58 otaq kodu/linki var, R09 avtomatik dəvət bildirişi yoxdur. A20 mədəni mövsümi tədbir var, R05 mövsümlük reytinq sıfırlaması və R11 battle pass yoxdur. A28 səssiz NPC mövcuddur, R14 NPC dialoqu və R15 hekayə yoxdur.


