# Test matrisi və sübut qaydası

P00.2-də yalnız repository/sənəd/status yoxlanır. Bu, oyun testləri deyil.
Əmrlər repo kökündən, Python 3.10+ standart kitabxanası və Git ilə işləyir.

```bash
python3 scripts/validation/validate_project.py --output docs/reports/P00.2.validation.json
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
| Asset manifesti | Sütunlar/ID/yol/hüquq sahələri; hazırda 0 asset |
| Validatorun mənfi sınaqları | unittest: zədələnmiş mənbə, status, inventar, istinad, ignore və secret rədd olunur |
| Git bütövlüyü, whitespace, təmiz commit vəziyyəti | git fsck, git diff --check, son status; part hesabatında qeyd |
| Uzaq nüsxə | Privacy, push və SHA yoxlaması; yalnız real nəticə qeyd edilir |
| Unity compile/EditMode/PlayMode, APK/iOS, GUI/cihaz | NOT_RUN — P00.2 əhatəsində deyil, oyun layihəsi yoxdur |

Validator çıxış kodu: 0 = yoxlamalar keçib; 1 = uyğunsuzluq və ya oxuma xətası.
Hər yoxlama adı və statusu JSON-da qalır. `--root` başqa checkout-un yoxlanmasına imkan verir.
Validatorun özü heç bir statusu təsdiqləmir və növbəti partı açmır.
P00.2 sərhəd yoxlaması gələcək part üçün dəyişdiriləcəksə insan icazəsini əvvəl STATE-də qeyd et;
validatoru sadəcə yaşıl nəticə almaq üçün zəiflətmə.

## Gələcək partlar üçün məcburi yanaşma

Mənbə: Master Plan VII. Qaydalar — fixture/invariant, bot — seed-li simulyasiya,
UI — real render/input və Azərbaycan hərfləri, network — iki klient parity/reconnect,
economy — duplicate/receipt, release — Android və iPhone real cihaz sübutu.
Hər nəticə PASS / FAIL / BLOCKED / NOT_RUN kimi, əmri, exit code-u, əhatəsi və sübut yolu ilə qeyd edilir.
Build uğuru cihaz testi deyil; screenshot olmadan vizual PASS yoxdur.
P00.2 mənfi sınaqları yalnız müvəqqəti surətdə işləyir; iş qovluğundakı mənbələri dəyişmir.
