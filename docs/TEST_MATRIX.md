# Test matrisi və sübut qaydası

P01.2-də repository/sənəd/status və uzun nərd mühərriki yoxlanır.
67 nümunənin 26-sı uzun mühərrikdə icra olunur; 39 qısa/ortaq və 2 undo nümunəsi öz partını gözləyir.
Əmrlər repo kökündən, Python 3.10+ standart kitabxanası və Git ilə işləyir.

```bash
python3 scripts/validation/validate_project.py --output docs/reports/P01.2.validation.json
python3 scripts/validation/validate_rule_fixtures.py
python3 scripts/build/p012.py editmode
python3 scripts/build/p012.py playmode
python3 scripts/build/p012.py android
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
| Unity compile/EditMode/PlayMode, Android APK | P01.2.execution.json və saxlanan NUnit XML; ayrı source copy-də real icra |
| iOS və fiziki cihaz | Yalnız real icra varsa PASS; yoxdursa NOT_RUN və səbəb |

Validator çıxış kodu: 0 = yoxlamalar keçib; 1 = uyğunsuzluq və ya oxuma xətası.
Hər yoxlama adı və statusu JSON-da qalır. `--root` başqa checkout-un yoxlanmasına imkan verir.
Validatorun özü heç bir statusu təsdiqləmir və növbəti partı açmır.
P01.2 sərhəd yoxlaması gələcək part üçün dəyişdiriləcəksə insan icazəsini əvvəl STATE-də qeyd et;
validatoru sadəcə yaşıl nəticə almaq üçün zəiflətmə.

## Gələcək partlar üçün məcburi yanaşma

Mənbə: Master Plan VII. Qaydalar — fixture/invariant, bot — seed-li simulyasiya,
UI — real render/input və Azərbaycan hərfləri, network — iki klient parity/reconnect,
economy — duplicate/receipt, release — Android və iPhone real cihaz sübutu.
Hər nəticə PASS / FAIL / BLOCKED / NOT_RUN kimi, əmri, exit code-u, əhatəsi və sübut yolu ilə qeyd edilir.
Build uğuru cihaz testi deyil; screenshot olmadan vizual PASS yoxdur.
Repository mənfi sınaqları yalnız müvəqqəti surətdə işləyir; iş qovluğundakı mənbələri dəyişmir.

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
