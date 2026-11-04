# Test matrisi və sübut qaydası

P02.5-də drag/sürətli tap/undo və lokal zər/tam növbə və əvvəlki uzun/qısa/matç/taxta/tap sistemlərinin regressiyası yoxlanır.
67 unikal fixture-in 26-sı uzun, 30-u qısa, 7-si matç modulunda PASS-dır; 4 undo P02.5-də ayrıca PASS-dır; 67 unikal nümunə tam icra edilir.
Uzun test faylındakı 41 tarixi skip-dən 30-u ShortFixture, 7-si MatchFixture, 4-ü UndoFixture kimi ayrıca keçir.
Əmrlər repo kökündən, Python 3.10+ standart kitabxanası və Git ilə işləyir.

```bash
python3 scripts/validation/validate_project.py --output docs/reports/P02.5.validation.json
python3 scripts/validation/validate_rule_fixtures.py
python3 scripts/build/p012.py editmode --part P02.5
python3 scripts/build/p012.py playmode --part P02.5
python3 scripts/build/p012.py android --part P02.5
python3 -m unittest discover -s scripts/validation -p 'test_*.py' -v
```

| Yoxlama | Sübut / nəticə |
|---|---|
| Mənbə fayllarının SHA-256 bütövlüyü | Validator JSON; auditdəki orijinal hash-lərlə müqayisə |
| A01–A87 / R01–R20 dəqiq mətn, say, status və part bağlantısı | Validator JSON; hər iki mənbə ilə müqayisə |
| Bir aktiv part, icazə, insan təsdiqi, gözləmə qapısı | Validator JSON; STATE və PROGRESS uyğunluğu |
| Məcburi fayllar və işlək lokal Markdown istinadları | Validator JSON; orijinal mənbələrdəki gələcək nümunə yolları tələb edilmir |
| .gitignore secret/cache qorunması, source/asset fayllarının qorunması | Real git check-ignore probe; fayl yaratmadan |
| Candidate/index fayllarında böyük fayl və tanınan secret nümunələri | Validator JSON; məhdud statik yoxlama |
| Saxlanan test sübutları | STATE istinadları repo daxilində mövcuddur; PASS iddiaları JSON/log nəticələri ilə uyğunlaşır. Bu yoxlama sübutun dəyişdirilmədiyi və ya kodun cari versiyasına aid olduğu zəmanəti deyil. |
| Asset manifesti | Sütunlar/ID/yol/hüquq sahələri; hazırda 0 asset |
| Validatorun mənfi sınaqları | unittest: zədələnmiş mənbə, status, inventar, istinad, ignore və secret rədd olunur |
| Git bütövlüyü, whitespace, təmiz commit vəziyyəti | git fsck, git diff --check, son status; part hesabatında qeyd |
| Uzaq nüsxə | Privacy, push və SHA yoxlaması; yalnız real nəticə qeyd edilir |
| Fixture strukturu və uyğunluğu | P01.1.rules.json: 24 xana, 15+15 daş, tokenlər, before/after arifmetikası, ID/variant/undo istinadları, matris uyğunluğu |
| Qayda mətninin uyğunluğu | Qayda ID-lərinin mövcudluğu avtomatik; məna və yerli variant qərarı insan baxışında. Tam təbii dil sübutu iddia edilmir |
| Unity compile/EditMode/PlayMode, Android APK | P02.5.execution.json və saxlanan NUnit XML; ayrı source copy-də real icra |
| iOS və fiziki cihaz | Yalnız real icra varsa PASS; yoxdursa NOT_RUN və səbəb |

Validator çıxış kodu: 0 = yoxlamalar keçib; 1 = uyğunsuzluq və ya oxuma xətası.
Hər yoxlama adı və statusu JSON-da qalır. `--root` başqa checkout-un yoxlanmasına imkan verir.
Validatorun özü heç bir statusu təsdiqləmir və növbəti partı açmır.
P02.5 sərhəd yoxlaması gələcək part üçün dəyişdiriləcəksə insan icazəsini əvvəl STATE-də qeyd et;
validatoru sadəcə yaşıl nəticə almaq üçün zəiflətmə.

## Gələcək partlar üçün məcburi yanaşma

Mənbə: Master Plan VII. Qaydalar — fixture/invariant, bot — seed-li simulyasiya,
UI — real render/input və Azərbaycan hərfləri, network — iki klient parity/reconnect,
economy — duplicate/receipt, release — Android və iPhone real cihaz sübutu.
Hər nəticə PASS / FAIL / BLOCKED / NOT_RUN kimi, əmri, exit code-u, əhatəsi və sübut yolu ilə qeyd edilir.
Build uğuru cihaz testi deyil; screenshot olmadan vizual PASS yoxdur.
Repository mənfi sınaqları yalnız müvəqqəti surətdə işləyir; iş qovluğundakı mənbələri dəyişmir.

Aşağıdakı P00–P02.3 bölmələri həmin partların tarixi əhatəsidir; köhnə NOT_RUN qeydləri cari 67/67 nəticəsini əvəz etmir.

## Unity ilkin testləri

`python3 scripts/build/p003.py configure|editmode|playmode|android` — hər əmrdə bir addım.
EditMode versiya, build səhnəsi, 2D serialization və safe-area sərhəd nümunələrini yoxlayır.
PlayMode Bootstrap səhnəsini yükləyir və kadrların həqiqətən irəlilədiyini yoxlayır.
Cihazda test edilmədikcə bu nəticələr Android toxunma/orientation testi sayılmır.

## P01.1 sərhədi

Schema draft-2020-12 formatındadır. Offline checker yalnız faylda işlədilən sənədləşdirilmiş
keyword alt çoxluğunu yoxlayır; naməlum keyword-u rədd edir, tam JSON Schema implementasiyası deyil.
Hər iki rejimdə müqavilə və gözlənilən vəziyyət nümunələri saxlanır; uzun motorun gedişləri və maksimum zər axtarışı P01.2-də sınanır; qısa motor P01.3-də qalır.
P01.1 ilkin qayda seçimi qəbul edilib. Müstəqil yerli oyunçu baxışı [beş mövqe qrupu](RULE_MATRIX.md) üzrə hələ NOT_RUN-dır.
Fiziki Android və iOS testi bu partda NOT_RUN-dır.

## P01.2 mühərrik yoxlaması

Unity EditMode canonical JSON-u oxuyur; yalnız 26 aid uzun nümunə işlənir.
Hər iki oyunçunun ilk/sonrakı qoşaları, döngədə altılıq, sıradan asılı maksimum istifadə,
yüksək zər, qismən/tam blok, evə giriş/çıxış, son daş/mars və girişin dəyişməməsi əlavə C# testlərindədir.
Test assembly-sində istehsal addım funksiyasını çağırmayan ayrıca nisbi-koordinat hesablaması var:
72 başlanğıc roll/player kombinasiyası, 1000 sintetik mövqe və seed=1202 ilə çatılmış
mövqelər onunla müqayisə edilir. Sintetik mövqelərin başlanğıcdan çatılması iddia edilmir.
RNG yalnız test məlumatındadır; runtime qayda assembly-sində yoxdur.
Bütün mümkün nərd mövqelərinin tam isbatı və mobil performans testi aparıldığı iddia edilmir.
Runner yalnız əvvəlcədən müəyyən edilmiş 41 skip-i qəbul edir; yeni skip/fail nəticəni FAIL edir.

## P01.3 qısa mühərrik yoxlaması

30 canonical qısa nümunə, əlavə 27 çevrilmiş nümunə və bar/qoşa/çıxarma/mars-koks sərhədləri.
72 başlanğıc roll/player kombinasiyası və 1000 sintetik bar/off/usedMask mövqeyi ayrı test-only
nisbi məsafə hesablamasına qarşı müqayisə edilir. RNG yalnız test məlumatıdır.
Seed 1313 ilə çatılan mövqelərdə bar/vurma və 15+15 invariantı da yoxlanır.
Uzun runtime/test SHA-256-ları qəbul edilmiş P01.2 ilə sabitlənib. Uzun faylları dəyişdirib
regressiyanı gizlətmək rədd edilir. --part P02.4 runner-də bütün 30 ShortFixture keçməlidir;
uzun suite-in skip-i qısa nümunənin PASS-ı sayıla bilməz. Fiziki cihaz və yerli baxış NOT_RUN qalır.

## P01.4 matç və seriya yoxlaması

263 EditMode PASS = əvvəlki 201 + 62 yeni matç testi; 41 tarixi ignored case ayrıca saxlanır.
7 yeni canonical seriya/reytinq nümunəsi və 55 əlavə müsbət/mənfi/sərhəd/istisna halı var.
Created/Rolling/Moving/TurnCommitted/Finished, yanlış rol/versiya/faza, təkrar zər slotu,
qismən təklif, snapshot dəyişməzliyi, nəticə və seriya tarixçəsi qorunması yoxlanır.
Hər iki rejimdə 1/3/5 üçün 84 qalib ardıcıllığı; qoşaların xal artırmaması və koksun bir
seriya qələbəsi olması sınanır. Seed 1401 uzun oyun 95, seed 1402 qısa oyun 140 növbədə bitir.
Bunlar test-only qanuni seçimlərdir; runtime bot deyil. 67 ID-nin fərdi real statusu
P02.4.execution.json daxilində fixture_execution siyahısındadır; validator onu NUnit ilə tutuşdurur.
4 undo nümunəsini PASS göstərmək, ledger-dən nümunə silmək və əvvəlki mühərrik/testləri dəyişmək rədd edilir.
1 PlayMode bootstrap testi və Android ARM64 IL2CPP build ayrıca keçir; yeni UI sınağı iddia edilmir.
Fiziki Android/iOS və müstəqil yerli oyunçu yoxlaması NOT_RUN qalır.

## P02.1 taxta yoxlaması

38 yeni EditMode: board-global-v1-in 24 unikal indeksi, hər xanada bütün 15 daşın ayrıca görünməsi,
üst-üstə düşməməsi, hər iki oyunçunun bar/off=15 sərhədi, dəyişməz snapshot, rejim dəyişərkən köhnə
obyektlərin qalmaması, null/indeks/say/ordinal xətaları və Azərbaycan font glyph-ləri.
Əvvəlki 263 EditMode dəyişmədən keçir. PlayMode-da əvvəlki bootstrap və yeni TechnicalBoard testi var.
Yeni test uzun/qısa/bar-off snapshot-larını yükləyib 30 daş/24 xana və camera bounds yoxlayır;
real graphics context-də 540×960 RenderTexture screenshot-ları və rəng pixel yoxlaması yaradır.
Bunlar Editor PlayMode camera renderidir; fiziki cihaz/touch testi deyil. Əlavə açıq GUI preview
Game View screenshot-u ayrıca saxlanır; klik/gediş/atış funksiyası bu partda sınaq predmeti deyil.
Runner P02.1-də 38 projection və 1 scene testinin çatışmamasını FAIL edir.
67 canonical qayda nümunəsinin 63-ü yenidən icra olunur; 4 undo P02.4 üçün NOT_RUN qalır.

## P02.2 seçim və tap yoxlaması

55 yeni EditMode = 43 moving canonical mövqedə engine-path marker/step müqayisəsi + 12 əlavə hal.
12 əlavə hal null/indeks/player, yanlış owner/phase, təkrar mənbə seçimi, alternativ öz daşı,
yüksək zərin eyni off hədəfində seçilməsi, blocked bar, hit/off, readonly prefix və hit-map sərhədləridir.
Bir test hər iki rejimdə 2 player × 36 roll = 144 tam engine yolu prefiksini də yoxlayır.
5 yeni PlayMode: real scene-də Mouse/TouchState hadisələri, seç→rədd→gediş renderləri, 5 explicit resetli
hərəkət, duplicate/out-of-order press, drag/multitouch rəddi, qısa bar/vurma/off/yanlış owner sinxronluğu.
Bu sintetik input, real Editor render testidir; fiziki cihaz və OS barmaq testi deyil.
Əvvəlki 301 EditMode və 2 PlayMode faylları dəyişmədən təkrar işləyir. 41 legacy skip saxlanır.
63 unikal canonical PASS qalır; 43 əlavə tap projection həmin nümunələrin təkrar bridge yoxlamasıdır,
67 cəminə əlavə edilmir. 4 undo nümunəsi P02.4 üçün NOT_RUN-dır.
Native Analytics bağlanış qəzası üçün köhnə dump/log saxlanır; yeni batch/GUI exit nəticəsi ayrıca qeyd olunur.

## P02.3 zər və növbə yoxlaması (tarixi)

168 yeni EditMode: iki rejim × iki actor × 36 roll = 144 normal atış; iki rejimdə 6 ilkin qoşa = 12 tie;
12 digər hal ilkin variant fərqi, yanlış rol/faza, mənbə exception/invalid qiymət, kolleksiya qorunması,
committed/finished sərhədləri və 8 seed-li tam oyunu əhatə edir. Yerli RNG yalnız range yoxlamasından keçir;
statistik ədalət və ya online etibar sübutu iddia edilmir.
8 oyunda uzun 384 növbə/2 blok/17 qismən, qısa 459 növbə/19 blok/18 qismən hal müşahidə olunur.
5 yeni PlayMode: Mouse ilkin/double/istehlak/yeni actor; bütün 72 mode/face render kombinasiyası;
qısa ilkin zərlərin təkrar istifadəsi; Touch/duplicate/drag/multitouch qoruması; real çatılmış blok/qismən render.
Bunlar Unity Input System sintetik hadisələri və həqiqi Editor renderidir; fiziki cihaz/OS barmaq testi deyil.
Əvvəlki 356 EditMode/7 PlayMode faylı dəyişmədən qalır. Runner 168/5 yeni case və 144 roll ID-ni tələb edir.
67 canonical nümunə 63 PASS/4 undo NOT_RUN olaraq qalır; yeni kombinasiya testləri 67-yə əlavə edilmir.

## P02.4 əlavə əhatə

144 rejim/oyunçu/zər kombinasiyası üzrə undo→təkrar gediş→commit→yeni növbə yoxlanır.
43 hərəkət mövqeyində bar/off/baş/istifadə maskası bərpası, iki rejimdə çoxaddımlı undo,
qismən state-dən bərpa və bitmiş matçda undo rəddi; null/yanlış actor istisnaları.
4 kanonik UndoFixture birbaşa JSON-dakı before/expected.after ilə yoxlanır.
PlayMode: mouse drag/undo/sürətli tap, uzun/qısa touch drag, ikinci barmaq, cancel,
ekran xarici buraxma, düyməyə buraxma, kiçik sürüşmə və çoxhədəfli tap.
Unity Input System sintetik hadisələridir; fiziki barmaq/telefon testi deyil.
Ekran görüntüsü vizual sübutdur; telefon rahatlığı/FPS və yerli qayda təsdiqi deyil.

D031 görünüş dəyişikliyi: 24 xana, iki oyunçu üçün bar/off və iki düymənin tərs koordinat xəritəsi üçün
27 əlavə EditMode testi. Eyni 5 Undo PlayMode testi indi real referans üslublu taxtada təkrar icra edilir.
Əvvəlki 12 PlayMode testi texniki səhnələrdə regressiya kimi saxlanır. Art görüntüləri oyun vəziyyətindən ayrıdır.

## P02.5 yerləşim və state davamlılığı

30 əlavə EditMode: 12 ölçü × full/notch safe area = 24 hal; yanlış ölçü üçün 3 exception və safe-area fallback/clipping.
Əlavə iki test portretdə 6 dik sütunu, daş diametrini, yanlış indeksləri və çərçivənin passivliyini yoxlayır.
Hər ölçüdə bütün 24 xana, hər iki owner bar/off, ayrı idarələr, böyük board sahəsi, təbii aspect və tərs input yoxlanır.
7 əlavə PlayMode: 5 real render nisbəti; tap→undo→resize→drag→commit; iki istiqamətdə touch/multitouch;
ekran/control sərhədi; həqiqi çatılmış bar/off/bitmiş matçın çevrilməsi. Hər renderdə 30 daş,
qorunan safe area, zərlə daşın toqquşmaması, köhnə zolağın/debug yazıların gizlənməsi yoxlanır.
Əlavə PlayMode: portretin xana kənarından seçim/real iki tərəf HUD-u və ilkin bərabər atış/real idarələr.
Bunlar sintetik Input System + real Editor renderidir, fiziki orientation/barmaq sınağı deyil.
Əvvəlki 749 EditMode və 17 PlayMode adlarının saxlanması ayrıca müqayisə edilir; 67 canonical nümunə və 41 tarixi Ignore dəyişmir.
