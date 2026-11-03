# Test matrisi və sübut qaydası

P01.1-də repository/sənəd/status, qayda fixture sxemi və məlumat uyğunluğu yoxlanır.
Qanuni gediş motoru yoxdur; 67 nümunə gözlənilən cavablardır, motor testinin nəticəsi deyil.
Əmrlər repo kökündən, Python 3.10+ standart kitabxanası və Git ilə işləyir.

```bash
python3 scripts/validation/validate_project.py --output docs/reports/P01.1.validation.json
python3 scripts/validation/validate_rule_fixtures.py --output docs/reports/P01.1.rules.json
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
| Unity compile/EditMode/PlayMode, Android APK | P00.3 tarixi sübutu; bu partda Unity kodu dəyişməyib, yenidən icra NOT_RUN |
| iOS və fiziki cihaz | Yalnız real icra varsa PASS; yoxdursa NOT_RUN və səbəb |

Validator çıxış kodu: 0 = yoxlamalar keçib; 1 = uyğunsuzluq və ya oxuma xətası.
Hər yoxlama adı və statusu JSON-da qalır. `--root` başqa checkout-un yoxlanmasına imkan verir.
Validatorun özü heç bir statusu təsdiqləmir və növbəti partı açmır.
P01.1 sərhəd yoxlaması gələcək part üçün dəyişdiriləcəksə insan icazəsini əvvəl STATE-də qeyd et;
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
Hər iki rejimdə müqavilə və gözlənilən vəziyyət nümunələri saxlanır; motorun bütün qanuni
gedişləri, maksimum zər axtarışı və fixture gözləntilərini icra etməsi P01.2/P01.3-də sınanacaq.
İnsan baxışı [beş mövqe qrupu](RULE_MATRIX.md) üzrədir; hazırda PENDING.
Fiziki Android və iOS testi bu partda NOT_RUN-dır.
