# NƏRD. — FINAL GAME DESIGN DOCUMENT

**Versiya:** 1.0 (planlaşdırma yekunu)  
**Tarix:** 3 noyabr 2026  
**Status:** Məhsul konsepti təsdiqlənib; inkişaf və real oyunçu yoxlamaları başlamayıb  
**Platformalar:** Android və iOS  
**Mühərrik:** Unity 6, C#  
**AI inkişaf mühiti:** Google Antigravity + rəsmi Unity plugin / Unity CLI  
**Multiplayer backend:** Nakama (server-authoritative), TypeScript runtime  
**İstehsal DB:** CockroachDB (Nakama-nın rəsmi dəstəklənən yolu)

> **Qərarların vəziyyəti:** 87 təsdiqlənmiş məhsul xüsusiyyəti, 20 rədd edilmiş xüsusiyyət, 7 təsdiqlənmiş buraxılış qərarı. Əlavə olaraq GDD mərhələ 15-in 6 əsas qaydası, mərhələ 16-nın 6 variant qərarı və mərhələ 17–18-in texniki qərarları mövcuddur. Bu saylar ayrı kateqoriyalardır, bir-birinə əlavə olunmur.

## 1. İcra xülasəsi

**NƏRD. — Bir masa. Min rəqabət.** Azərbaycana xas, səmimi və keyfiyyətli 2D mobil nərd oyunu. Məhsulun əsas dəyəri eyni anda üç istiqaməti birləşdirməkdir: (1) nərd qaydalarını düzgün və ədalətli icra etmək, (2) dostla və təsadüfi oyunçu ilə rahat rəqabət aparmaq, (3) isti Azərbaycan çayxanasını hiss etdirən, lakin adi mobil tətbiqə və ya nizamsız AI vizuallarına oxşamayan oyun dünyası yaratmaq.

**Məhsulun dəyişməz prinsipləri:**

- Oyun birinci yerdədir: növbəti matça keçid 1–2 toxunuşla mümkün olmalıdır.
- **2D** qalır; 3D mühit, neon, qara-korporativ vizual kimlik və kazino estetikasından imtina edilir.
- Əsas rənglər: isti krem `#F6EBD5`, terrakota `#C96F4C`, firuzə `#4D9995`, bal sarısı `#E8B85D`, qoz `#79563D`.
- Uzun və qısa nərd üçün ayrıca qayda mühərrikləri və ayrıca bacarıq reytinqləri.
- Zər nəticəsi və online gediş etibarı müştəri tətbiqində deyil, serverdə müəyyənləşdirilir.
- Ödənişli kosmetika rəqabət üstünlüyü vermir; real və ya virtual mərc yoxdur.
- Animasiya səmimidir, sürətə mane olmur; qələbə effektləri qısadır, keçilə bilir.
- Android ilk test hədəfidir; Android/iOS ictimai buraxılışı birlikdədir.

### Qərar kateqoriyaları

| Kateqoriya | Vəziyyət | Mənası |
|---|---|---|
| A — Məhsulun funksiyaları | 87 təsdiq | Uzunmüddətli product backlog; hamısı ilk buraxılışa daxil deyil |
| R — Rədd edilmiş | 20 rədd | Bu funksiyalar geri gətirilmir; yalnız yeni açıq qərarla dəyişər |
| L — Buraxılış strategiyası | 7 təsdiq | Qapalı beta, keyfiyyət qapısı, sinxron buraxılış və s. |
| Q — Əsas nərd prinsipləri | 6 təsdiq | Reytinq sərhədi, undo və variantların ayrılığı |
| V — Detallı qayda variantları | 6 qərar | Lokal nərd icması ilə testə qədər versiyalı qayda dəsti |
| T — Texniki seçim | Qəbul olunub | Unity 6, Antigravity, Nakama, CockroachDB; POC ilə təsdiq |

## 2. Hədəf auditoriya və oyunçuya verilən vəd

**İlkin hədəf:** Azərbaycanda nərd oynayan, dostları ilə telefonda matç qurmaq istəyən və öz səviyyəsinə uyğun rəqib axtaran insanlar. İkinci qrup: nərdi yeni öyrənənlər; üçüncü qrup: vizual mədəniyyətə dəyər verən, xaricdə yaşayan azərbaycanlılar.

**Oyunçuya vəd:** “Nərdin özünü pozmadan, rahat bir masada və əsl rəqiblərlə oynayırsan.”

**İlk 60 saniyə:** tətbiq açılır → qısa interaktiv masa → qonaq kimi daxilolma → bir toxunuşla rejim → ilk bot gedişi və ya dost masası. Daxilolma, satış təklifi və uzun təlim ilk matçı bloklamır.

**Əsas oyun dövrü:** Ana masa → rejim (uzun/qısa + bot/dost/rəqib) → matç → nəticə → reytinq/mükafat (uyğun rejimdə) → revanş/yeni matç → kolleksiya və məkanlar.

## 3. Rejimlər və matç konfiqurasiyası

| Rejim | Offline | Reytinq | Oyunçular | Qeyd |
|---|---|---|---|---|
| Uzun nərd — bot | Bəli | Xeyr | 1 insan + bot | 3 çətinlik səviyyəsi |
| Qısa nərd — bot | Bəli | Xeyr | 1 insan + bot | 3 çətinlik səviyyəsi |
| Dostla özəl masa | Xeyr | Xeyr | 2 | Kod/link, masa parametrləri, izləyici icazəsi |
| Təsadüfi onlayn rəqib | Xeyr | Bəli, reytinqli seçildikdə | 2 | Ayrı uzun/qısa reytinq |
| Sürətli matç | Rejimə görə | Şərtdən asılı | 2 | Qısa növbə taymeri |
| Klassik matç | Rejimə görə | Şərtdən asılı | 2 | Daha rahat taymer |
| Best of 3 / 5 | Rejimə görə | Matç qaydasına görə | 2 | 2 və ya 3 partiya qələbəsi ilə seriya bitir |

**Təklif edilən ilk parametr dəstləri (hələ ölçülməmiş dizayn fərziyyəsi):** sürətli matçda 30 s/növbə, klassikdə 90 s/növbə; özəl masada seçim imkanı. Taymer bitməsi zamanı qayda testləri və disconnect siyasəti ayrıca təsdiqlənməlidir. Bunlar istifadəçinin təsdiqlədiyi rəqəmlər deyil və beta zamanı dəyişə bilər.

## 4. Nərd qaydaları və vahid doğruluq mənbəyi

Qaydaların **domain engine** hissəsi Unity vizuallarından, botdan və serverdən asılı olmayan deterministik modul kimi layihələndirilir. Eyni giriş vəziyyəti və gediş üçün eyni çıxış nəticəsi verilməlidir. Server oyunun son qərarvericisidir; klientdəki gediş göstəricisi yalnız əvvəlcədən baxışdır.

### 4.1 Bütün rejimlər üçün

- Taxta 24 xanalıdır. Hər oyunçuda 15 daş və iki altıüzlü zər var.
- Hər növbədə zər qiymətləri ayrıca hərəkət kimi istifadə olunur; qoşa atışda 4 ayrı hərəkət hüququ yaranır.
- Mövcud qanuni gedişlərin mümkün maksimumu icra edilməlidir; tək zər oynanırsa, yüksək olanın prioriteti variant qaydasına uyğun yoxlanır.
- Bütün daşlar evə gətirilmədən daşları taxtadan çıxarmaq olmaz.
- Gedişi geri alma: yalnız **öz növbəsi hələ serverdə yekun təsdiqlənməyibsə**. Online rəqibə göndərilmiş, təsdiqlənmiş növbəni geri çevirmək olmaz.
- Uzun və qısa nərdin vəziyyətləri və reytinqləri ayrı saxlanır. Bot/özəl masa reytinqi dəyişmir.
- Səhnə, zər görünüşü, kosmetika və ya şəxsi məkan qaydanı dəyişmir.

### 4.2 Uzun nərd

- Başlanğıc: hər oyunçunun bütün 15 daşı öz baş nöqtəsində; daş vurma mexanikası **yoxdur**; rəqibin ən azı bir daşı olan xananı tutmaq olmaz.
- Baş qaydası: hər növbədə adətən başdan yalnız bir daş çıxarıla bilər. İlk şəxsi növbədə 3–3, 4–4 və 6–6 qoşa atışlarında iki daş istisnası tətbiq edilir.
- Altılıq blok məhdudiyyəti: rəqibin bütün 15 daşını hərəkətsiz qoyan qadağan blok qurulmur; hansı mövqe üçün qanuni olduğu ayrıca test edilir.
- İlk kimin başlayacağı bir zərlə müəyyənləşdirilir; **seçilmiş layihə variantı**: böyük atan sonra iki zər atıb başlayır. Qeyd: fərqli qayda mənbələrində ilk atış qiymətlərinin oynanması variantı da var — beta öncəsi lokal qayda dəsti sınağında bunu yoxlamaq vacibdir.
- Adi qələbə 1, mars 2 xal; **uzun nərddə koks tətbiq edilmir** (layihə seçimi, universal qayda iddiası deyil).

### 4.3 Qısa nərd

- Standart qısa nərd başlanğıc düzülüşü: hər oyunçu 2/5/3/5 daş qrupları ilə başlanır, rəqiblərin gediş istiqamətləri əksdir; tam 24-xanalı koordinat xəritəsi implementasiyadan əvvəl test şəklində təsbit edilməlidir.
- Rəqibin tək daşı vurula bilər; vurulan daş bar bölməsinə keçir və hərəkət etməzdən əvvəl qanuni qayda ilə yenidən oyuna daxil olur.
- Başlanğıcda hər kəs bir zər atır; böyük atan iki ilkin qiymətlə ilk gedişi edir. Bərabərlikdə təkrar atılır.
- Adi qələbə 1, mars (gammon) 2, koks (backgammon) 3 xal — sonuncu nəticənin tərifi (bar və ya qalibin evində məğlub daşının qalması) testlə kilidlənəcək.
- **Doubling cube yoxdur**; xalın artırılması və mərc mexanikası əlavə olunmur.

### 4.4 Seriyalar, xallar və statistika

Best of 3-də ilk iki partiyanı, Best of 5-də ilk üç partiyanı qazanan seriyanı udur. Mars/koks ayrıca nəticə statistikası kimi saxlanır; seriyada iki və ya üç qələbə yerinə sayılmır. Rəqabətli reytinq yalnız serverin yekun təsdiqlədiyi nəticədən hesablanır. “Mars” xalını kosmetik valyuta ilə qarışdırmaq olmaz.

### 4.5 Qayda variantlarının yoxlanması

Uzun nərd üzrə vahid universal kodeks yoxdur. Ona görə `RuleSetVersion` saxlanmalıdır. **Məcburi beta qapısı:** ən azı bir neçə təcrübəli yerli oyunçu ilə 3–3/4–4/6–6 baş istisnası, ilk atış, altılıq blok, daş çıxarma və mars nümunələri üzərində əl ilə test. Bu yoxlama ilkin qərarları gizlicə dəyişmək deyil; versiyalı qaydanın doğru icra olunduğunu təsdiqləmək üçündür.

## 5. Offline bot: üç səviyyə

**Yeni başlayan:** qanuni, sadə, bəzən strateji olaraq zəif seçimlər. **Təcrübəli:** bloklama, risk, daşların inkişafı arasında balans. **Usta:** bir neçə gediş və mümkün növbəti mövqe qiymətləndirməsi. Səviyyələr zərlərə deyil, hərəkət qiymətləndiricisinə təsir edir. Uzun və qısa nərd üçün ayrıca strategiya adapterləri yazılır.

- İnternet bağlantısı və ödənişli generativ AI sorğusu tələb olunmur.
- Eyni qanuni gediş motorundan istifadə olunur, bot qanunsuz gediş edə bilmir.
- Offline fasilə və matçın saxlanması var; botla Best of 3/5 mümkündür.
- Botun AI olduğu açıq görünür; insan oyunçusu kimi saxta təqdim edilmir.
- Çətinlik real test seriyalarında güc fərqi göstərməlidir. 100-lərlə avtomatik matçla yoxlanmalıdır; zərlərin ədalətliliyi hər səviyyədə eyni qalır.

## 6. İstifadəçi axını və ekran spesifikasiyası

**Ekranlar (əvvəlcə mobil, portret; matç həm portret, həm landşaft):**

| Kod | Ekran | Əsas interaksiya | Uğur meyarı |
|---|---|---|---|
| S01 | Qısa intro | Masaya toxun → daxil ol | İntro keçilə bilir |
| S02 | Ana masa | Oyna, dost, xəritə, mağaza, profil | Matça 1–2 toxunuş |
| S03 | Rejim seçimi | Uzun/qısa; bot/dost/rəqib; sürətli/klassik | 3 aydın qərar |
| S04 | Özəl masa | Kod/link, taymer, izləyici icazəsi | Dost stabil qoşulur |
| S05 | Matchmaking | Rəqib tap / ləğv et | Real gözləmə göstərilir |
| S06 | Matç | Zər, daş, gediş, undo, reaksiyalar, taymer | Toxunma xətası minimum |
| S07 | Nəticə | Qələbə, statistika, revanş | Revanş bir toxunuş |
| S08 | İllüstrativ xəritə | Məkan seç / aç / bax | Məkan qaydaya təsir etmir |
| S09 | Çarşı / mağaza | Kosmetika, önizləmə, əldə etmə | Qiymət və mənbə aydın |
| S10 | Kolleksiya albomu | Taxta/zər/daş/dekor böyütmə | Bir vizual üslub |
| S11 | Profil | Avatar, titul, ayrı reytinq, tarixçə, qrafik | İrəliləyiş görünür |
| S12 | Dostlar | ID ilə axtar, onlayn status, blok | Məxfilik idarəsi |
| S13 | Tapşırıqlar | Gündəlik/həftəlik, uğurlar, streak | İcbari iş hissi yaratmır |
| S14 | Xəbər lövhəsi | Mövsümi tədbirlər və yeniliklər | Reklam paneli deyil |
| S15 | Parametrlər | Səs, vibrasiya, gediş göstəricisi, animasiya, gizlilik | Rahatlıq qorunur |
| S16 | Dərslər | Qısa interaktiv nərd təlimi | Yeni oyunçu oyundan çıxmır |
| S17 | Matç təkrarı | Növbə-növbə irəli/geri | Səhvləri AI izah etmir |
| S18 | Matçı izləmə | Yalnız izləmə | Tamaşaçı reaksiyaları yoxdur |

**Oyunun hissi:** Standart biznes tətbiqi kimi kart-panellər yığını deyil. Ana ekran interaktiv nərd masasıdır; keçidlər səhnə animasiyalarıdır; zərlər və daşlar toxunuşa dərhal cavab verir. Qələbə animasiyası qısadır, məcburi gözlətmir. Portret/landşaft keçidində matçın state-i saxlanır; UI təzədən düzülür, oyun başdan başlamır. İnkişaf etmiş oyunçular üçün sürətli gedişlər var. Parametrlərdə vibrasiya, effekt və animasiya sürəti tənzimlənir.

## 7. Vizual və audio bələdçi (Art Bible)

**Ad:** `NƏRD.` — işçi brend; ticarət nişanı və mağaza adı uyğunluğu yayımdan əvvəl yoxlanmalıdır. **Sloqan:** `Bir masa. Min rəqabət.`

**İstiqamət:** Müasir Azərbaycan çayxanası, gün işığı, əl işi materiallar, xalça motivlərinin ölçülü istifadəsi, isti rəng. Ucuz plastik, neon, şişirdilmiş gradient, dekorativ AI şəkillərinin təsadüfi qarışığı və qara “corporate” fonlar **yox**.

**Palitra:** İsti krem `#F6EBD5` (fon), terrakota `#C96F4C` (əsas hərəkət), firuzə `#4D9995` (seçim/vurğu), bal `#E8B85D` (mükafat), qoz `#79563D` (taxta/mətn). İkinci dərəcəli semantik rənglər kontrasta görə seçiləcək.

**Vizual yoxlama prinsipləri:**

1. Eyni fırça/xətt dili, perspektiv, işıq mənbəyi və tekstura sıxlığı.
2. UI düymələri oyunun obyektlərinə uyğun, amma oxunaqlı və aşkar toxunulan səthlərdir.
3. Ornament toxunma hədəfinə, mətnə və daşların ayırdedilməsinə mane olmur.
4. 2D daş, zər və taxta materiallarında həddindən artıq fotorealistik və illüstrativ stil qarışdırılmır.
5. Azərbaycan hərfləri `ə, ğ, ı, İ, ö, ş, ç, ü` və mətn uzunluqları ayrıca yoxlanır.
6. AI istehsalı hər yeni asset art-direction checklist-i keçmədən oyuna daxil edilmir; əqli mülkiyyət hüququ yoxlanır.

**Məkanlar:** İçərişəhər həyəti, Şəki şəbəkə pəncərəsi, Bakı bulvarı, Lahıc mis sənətkarlığı — ilkin art direction dəsti; təsdiqlənmiş “Azərbaycan məkanları” funksiyasının konkret nümunələridir, məkanların dəqiq son siyahısı art istehsalda dəyişə bilər. Hər məkanda masa materialı, yüngül canlı detal (buxar, külək, işıq), məkan musiqisi və xüsusi kiçik hadisə olur. Gündüz/gecə dəstəyi var. Məkanlar irəliləyişlə açılır; rəqabət üstünlüyü vermir.

**Personajlar:** Çayxana mühitində səssiz, yüngül animasiyalı 2D personajlar ola bilər; **personajlarla tanışlıq dialoqları və məkan hekayələri olmayacaq**. Maskot və istifadəçinin şəxsi foto avatarı yoxdur. İllüstrativ avatarlar, aksessuarlar, profil çərçivələri, reaksiyalar və stikerlər var.

**Audio:** Əl işi zər səsi, daşların taxtaya yüngül toxunuşu, çay stəkanı, mühit səsləri, hər məkan üçün orijinal/lisenziyalı instrumental musiqi. Səs və vibrasiyanı söndürmək mümkündür; musiqi məcburi deyil.

## 8. Sosial sistem və rəqabət

- Qonaq kimi matça girmək mümkündür; hesab qorunması və hesab bağlama/bərpa UX-i sonradan təklif olunur.
- Unikal oyuncu ID-si ilə dost tapma, statusu gizlətmə, özəl masa kodu/linki, dostla revanş.
- Əvvəlcədən hazırlanmış mətn/stiker reaksiyaları var; **səsli və sərbəst yazılı söhbət yoxdur**.
- Matçı izləmə mövcuddur, amma canlı tamaşaçı reaksiyası yoxdur.
- Qəsdən matçı tərk etmə, vaxt sui-istifadəsi və spam üçün server əsaslı yumşaq/ciddi tədbirlər; blok/şikayət.
- Uzun və qısa nərd reytinqləri ayrı; reytinq yalnız serverin təsdiqlədiyi reytinqli təsadüfi matçlarda dəyişir; özəl dost masası və bot dəyişmir.
- Daimi liqa titulları: Şagird → Həvəskar → Mahir → Usta → Böyük Usta. Səviyyə/rating hədləri real statistika ilə kalibrlənəcək.
- Ölkə üzrə liderlər, reytinq inkişaf qrafiki və qayda pozuntusu cəzaları var; **mövsümlük reytinq sıfırlaması, şəhər cədvəlləri və həftəlik açıq turnirlər yoxdur**.
- Gündəlik tapşırıqlar, həftəlik missiyalar, giriş mükafatı, oyun günləri ardıcıllığı və nailiyyətlər var; streak itirənə sərt cəza yoxdur.

## 9. İqtisadiyyat və monetizasiya

**İlk ictimai versiya reklamsızdır.** Daha sonra yalnız oyunçunun seçimi ilə mükafatlı reklam aktivləşdirilə bilər. Mağaza və premium kosmetik kolleksiyalar uzunmüddətli plandadır; onların hamısının ilk açılışda canlı olması tələb deyil.

- **Bir oyun valyutası / xal:** gündəlik tapşırıqlar, nailiyyət və matç irəliləyişi ilə qazanılır; satılan cosmetic əşyalarla balans ayrıca müəyyən edilir.
- Kosmetika: taxtalar, zərlər, daşlar, avatar aksesuarları, profil çərçivəsi, mühit dekorları, xüsusi animasiya və effektlər.
- Premium dəstlər yalnız görünüş dəyişir; zərin nəticəsinə, bot zəifliyinə, gediş hüququna və reytinqə təsir etmir.
- Dostlara kosmetik hədiyyə sistemi — dublikat, sui-istifadə, geri ödəmə və alıcının təsdiqi qaydaları ilə.
- Onlayn satınalmalar mağaza platformalarının rəsmi in-app purchase sistemləri ilə, server tərəfdə alış təsdiqi və log vasitəsilə işləməlidir.
- Virtual jetonla mərc, real pul mərci, battle pass və məcburi reklam yoxdur.

**Ölçmə siyasəti:** Gəlir, xalis mənfəət və server xərcləri ayrı izlənir. Başlanğıcda oyunçu sayı və dönüşüm məlum deyil: yalan ARPU, retention və mənfəət proqnozu qoyulmur. Beta nəticəsindən sonra iqtisadiyyat balansı təyin edilir.

## 10. Texniki arxitektura

```
Unity 6 (Android/iOS)
├── 2D gameplay renderer / input / animation / audio
├── Presentation UI + responsive layouts
├── Pure rules engine (LongNarde, ShortNarde)
├── Offline bot adapter + save/load
└── Nakama Unity Client SDK
              │ auth / RPC / sockets
              ▼
Nakama authoritative backend (TypeScript runtime)
├── Account & friends / presence
├── Private rooms / matchmaking / spectators
├── Server-only dice RNG + move validation
├── Match lifecycle & reconnect snapshots
├── Ratings / leaderboard / results
├── Economy / rewards / gifts / anti-fraud
└── Moderation / logs / telemetry
              │
              ▼
CockroachDB (Nakama storage + scheduled backups)
```

**Əsas arxitektura qaydaları:**

- Klient heç vaxt “mən 6–6 atdım”, “mən qalib gəldim”, “mənim xalım artdı” məlumatını fakt kimi təsdiqlədə bilməz.
- Client yalnız `roll_request`, `move_proposal`, `end_turn`, `reaction`, `reconnect`, `spectate_request` kimi niyyətlər göndərir; Nakama qaydanı yoxlayır, vəziyyəti dəyişir, sonra yayır.
- Event-lərə `match_id`, `player_id`, `turn_no`, `request_id`, `ruleset_version`, `server_ts` sahələri əlavə olunur. İdempotent request-lər təkrar gedişi/ikinci mükafatı bloklayır.
- Tam replay üçün server təsdiqli hadisə jurnalı saxlanır; audit üçün zər və gediş ardıcıllığı qorunur, lakin zər RNG gizli saxlanmalıdır.
- Matç bərpası üçün periodik snapshot + event log; restart/bağlantı xətasında son server vəziyyəti əsasdır.
- Nazik şəbəkə fasiləsində reconnection grace-window; multiplayer prosesində rəqibi sonsuz gözlətməmək üçün timeout qaydaları.
- Server resurslarına, API açarlarına və mağaza sirlərinə klientdə yer verilmir. Secret-lər environment/secret storage-da saxlanır.
- Nakama TypeScript runtime adi Node.js deyil: kitabxanalar və asılılıqlar uyğunluğa görə yoxlanmalıdır.
- Unity-də C#, Nakama-da TypeScript işləyəcəyi üçün **qayda məntiqi iki implementasiyada eyni davranmalıdır**. Vahid test-fixture faylları (`position + roll + move -> expected state`) və client/server differential test-ləri bu uyğunluğu hər dəyişiklikdə yoxlamalıdır.
- CockroachDB Nakama üçün ilkin istehsal seçimidir; PostgreSQL yalnız dəstəklənmə şərti yoxlanmış development məqsədi üçün düşünülə bilər.

### Tövsiyə edilən domen məlumatları

`Player`, `Friendship`, `PlayerSettings`, `InventoryItem`, `CurrencyLedger`, `CosmeticCatalog`, `VenueUnlock`, `Match`, `MatchParticipant`, `MatchEvent`, `MatchSnapshot`, `RatingLong`, `RatingShort`, `LeaderboardEntry`, `QuestProgress`, `Achievement`, `GiftTransfer`, `PurchaseReceipt`, `ModerationReport`, `BlockedPlayer`.

Bu adlar baza cədvəllərinin mütləq eyni belə olacağına dair göstəriş deyil: Nakama Storage Engine və daxili kolleksiyalar üstün tutulur. Əlavə SQL cədvəlləri zərurət olmadıqca yaradılmır.

### Yoxlama üçün ən kiçik inteqrasiya POC

1. İki Android müştəri eyni otağa qoşulsun.
2. Server atışı yaratsın, hər iki müştəri eyni nəticəni görsün.
3. Server qanunsuz gedişi rədd etsin.
4. Üçüncü hesab tamaşaçı kimi qoşula bilsin, lakin gediş etməsin.
5. Bir telefon şəbəkədən çıxıb qayıtdıqda davam edən matçı alsın.
6. Server yenidən başladıldıqda saxlanılan vəziyyətdən düzgün bərpa mexanizmi test edilsin (ilk versiyada avtomatik davam etmirsə, aydın fallback siyasəti olmalıdır).

## 11. AI agentlərinin icra qaydası

**İnkişafın sahibi:** Google Antigravity agenti; rəsmi Unity plugin / CLI. Bütün böyük əməliyyatlar `PLAN → IMPLEMENT → TEST → REPORT → COMMIT` qaydası ilə aparılır.

**Sərt qaydalar:**

1. GDD-dəki rədd edilmiş 20 xüsusiyyət **backlog-a avtomatik qaytarılmır**.
2. AI təsdiqlənmiş qayda variantını səssiz dəyişmir; fərq tapırsa `decision-log.md`-də qeyd edir.
3. UI üçün AI tərəfindən yaranmış təsadüfi dizaynlar istifadəyə buraxılmır: Art Bible əvəzedilməz meyardır.
4. Unity scene yaradılması ilə məhdudlaşmaq olmaz: işləyən build + test log + ekran yoxlaması tələb olunur.
5. İlk növbədə bir sadə, funksional masa hazırlamaq, sonra art polish, sonra multiplayer və xidmətlər.
6. Server, auth, satınalma və hədiyyə kimi riskli sistemlər unit/integration test olmadan bağlanmış sayılmır.
7. Asset lisenziyası, məxfi açarlar, yaş uyğunluğu, mağaza ödənişləri və istifadəçi məlumatları ayrıca yoxlanır.
8. Hər mərhələdən sonra Git commit/tag və qısa `CHANGELOG.md` yenilənir.

### Başlanğıc repo strukturu (təklif)

```
NARD/
  GDD.md
  docs/
    art-bible.md
    rulebook-long.md
    rulebook-short.md
    decisions.md
    test-plan.md
    backend-protocol.md
  unity/
    Assets/
      Game/Rules/
      Game/Bot/
      Game/Presentation/
      Game/UI/
      Game/Net/
      Game/Tests/
  nakama/
    src/
    tests/
    docker-compose.yml
  assets/
    licenses.md
  README.md
```

**AI agentinə ilk tapşırıq:** “Bu GDD-ni və decision log-u oxu. Yalnız Unity 2D layihəsinin təmiz strukturunu, uzun/qısa nərdin data modellərini, qanuni gediş motorunu və avtomatlaşdırılmış testlərini qur. Nə mağaza, nə login, nə multiplayer, nə də bəzək asseti əlavə et. İşlək test nəticələrini və vizual 2D prototipini göstər.”

## 12. Buraxılış mərhələləri və keyfiyyət qapıları

| Mərhələ | Əhatə | Buraxılışa keçid şərti |
|---|---|---|
| A — Qayda prototipi | 2 oyun variantı, zərlər, daşlar, offline bot | Kritik qayda testləri yaşıl; offline matç tamamlanır |
| B — Oyun hissi | İsti UI, canlı masa, səs, şaquli/üfüqi | Android-də səhvsiz toxunma, 1–2 toxunuşla matç |
| C — Multiplayer beta | Nakama, özəl masa, random rəqib, reconnect, ədalət | 2 oyunçu, tamaşaçı, disconnect, server validation testləri |
| D — Qapalı beta | Reytinq, profillər, lazımi təhlükəsizlik, real cihaz sınağı | İstifadəçi matçları, kritik bug 0, UX iterasiya |
| E — İctimai buraxılış | Android + iOS, mağaza uyğunluğu, vizual keyfiyyət | Hər iki platformada test və birlikdə yayımlama |
| F — Sonrakı yeniləmələr | 87 backlog-un qalan hissəsi; kolleksiya genişlənməsi, mağaza, musiqi, tədbirlər, reklam opsionu | Hər funksiya real istifadə dəyərinə görə prioritetləşdirilir |

**7 buraxılış qərarı (dəyişməz):** bütün 87 xüsusiyyət roadmap-da qalır; əvvəl qapalı beta; dizayn keyfiyyəti yayın şərtidir; Android ilk testdir; multiplayer rəsmi buraxılışdan əvvəl sınaqdan keçir; ilk versiya reklamsızdır; Android+iOS birlikdə ictimai yayımlanır.

### İlkin test ölçüləri (fərziyyə; bazar faktı deyil)

- 30–50 real beta oyunçusu ilə UX və retention sınağı; oyun qaydasını bilən ayrıca qrup.
- İlk matçı tamamlayanlar üçün 80%+ ilkin hədəf; 7 gün geri dönüş 25%+ hipotez (nəticə deyil).
- Kritik qayda xətaları və kritik satınalma/hesab təhlükəsizlik xətaları: 0.
- Mobil axıcılıq üçün 60 FPS hədəfi, zəif cihaz üçün azaldılan effektlər; cihaz matrisi testdə müəyyənləşdirilir.
- Matça başlama axını mümkün qədər qeydiyyatsız, sürətli; server gözləməsi real vaxtla ölçülür.

## 13. Qəbul edilmiş 87 xüsusiyyətin tam inventarı

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

## 14. Rədd edilmiş 20 xüsusiyyət — DO NOT BUILD

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

## 15. Qəbul edilmiş əlavə GDD qərarları

**GDD-15:** (1) uzun və qısa nərd ayrı sənədləşir; (2) yalnız reytinqli matç reytinqi dəyişir; (3) məkan qaydanı dəyişmir; (4) öz növbəsi tamamlanmadan undo; (5) qonaq irəliləyişi saxlanır; (6) ayrı long/short rating.

**GDD-16:** (1) uzun nərd ilk atış variantı; (2) qısa nərd standart başlanğıcı; (3) başdan 3–3/4–4/6–6 istisnası; (4) adi/mars/koks scoring, uzun nərddə koks yox; (5) doubling cube yox; (6) Best of 3/5 partiya qələbəsinə görə.

**GDD-17:** offline AI botun üç səviyyəsi; ayrı long/short strategiyası; eyni ədalətli RNG; internet və generativ AI API tələb olunmur.

**GDD-18:** Unity 6, Antigravity rəsmi plugin+CLI, Nakama authoritative TypeScript, CockroachDB, Android ilk test, iOS bulud build yoxlaması; bütün kritik işlər üçün PLAN/IMPLEMENT/TEST/REPORT/COMMIT.

## 16. Açıq yoxlama qapıları və risklər

Bu maddələr təsdiqlənmiş konsepti zəiflətmir; implementasiyadan əvvəl real dəlil tələb edən nöqtələrdir.

1. **Uzun nərdin lokal variantı:** ilkin atış və bəzi xüsusi nəticələrin yerli nərd ustaları ilə doğrulanması. Fərqli mənbələr arasında fərq var.
2. **Unity/Nakama POC:** rəsmi pluginin hazırkı versiyası və TypeScript runtime uyğunluğu kiçik prototiplə təsdiqlənsin.
3. **Mobil incəlik:** həm portret, həm landşaft matç görünüşü kiçik ekranlarda oxunaqlılıq testindən keçsin.
4. **iOS publishing:** macOS/Xcode imzalama və real cihaz testləri, bulud build, hesab hüququ və mağaza qaydaları yayımdan əvvəl yoxlansın.
5. **Hüquqlar və məzmun:** milli naxışların orijinal interpretasiyası, musiqi lisenziyası və istifadəyə icazəli fontlar yoxlansın.
6. **İqtisadiyyat:** hədiyyə və premium satışların geri ödəmə, saxtakarlıq, yaş uyğunluğu və bazar konversiyası riskləri test edilsin.
7. **KPI:** 60 FPS, 80% ilk matç bitirmə, 25% D7 geri dönüş yalnız ilkin hədəfdir; nəticə kimi təqdim edilməsin.
8. **Scope:** 87 qərarı ilk gündən qurmamaq. Əsas düzgün oyun hissini sübut etməkdən əvvəl dekorativ iş həcmini böyütmək olmaz.

## 17. Mənbələr və rəsmi texniki istinadlar

- Unity rəsmi plugin / Antigravity: https://docs.unity.com/en-us/ai/unity-plugin/antigravity
- Unity plugin ümumi: https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin
- Unity rəsmi kod repozitoriyası: https://github.com/Unity-Technologies/unity-agent-plugin
- Nakama server-authoritative multiplayer: https://heroiclabs.com/docs/nakama/concepts/multiplayer/authoritative/
- Nakama Linux/CockroachDB dəstəyi: https://heroiclabs.com/docs/nakama/getting-started/install/linux/
- Nakama storage engine: https://heroiclabs.com/docs/nakama/concepts/storage/
- Uzun nərd qaydalarının müqayisəsi: https://narde.games/en/long-narde/rules/ ; https://sites.google.com/site/longnarde/en ; https://nardyunion.com/en/learn/long-nardy-rules
- Qısa nərd ümumi qaydalar: https://backgammon.com/how-to-play-backgammon

---

**FINAL SƏNƏDİN STATUSU:** Bu GDD planlaşdırma üzrə yekundur; texniki tətbiq və beta doğrulaması üçün baza sənədidir. İstifadəçi tərəfindən təsdiqlənmiş məhsul qərarları agentin ehtimallarından üstündür. **İlk icra tapşırığı: qaydalar mühərriki + avtomatik testlər + minimal, oynanıla bilən 2D taxta.**
