# Modulyar arxitektura — gələcək implementasiya sərhədləri

Əsas: [GDD §4, §5, §10](../NARD_Final_GDD_v1.0.md) və
[Master Planın ardıcıllığı](../NARD_Master_Icra_Plani_v2.md).
P00.2-də qəbul edilmiş modul sərhədləridir; P00.3 texniki bootstrap əlavə edib.
P01.2-də yalnız LongPosition/LongNardeRules pure C# modulu var. Qısa qayda, bot, UI/UX və backend yoxdur.
İnfrastruktur, UI framework, əlavə servis və DB cədvəlləri bu sənədlə quraşdırılmır.

## Asılılıq istiqaməti

```text
Presentation / UI → Match application → Pure rules (Long / Short)
Offline bot ─────────────────────────→ Pure rules
Match application → save və network portları ← Unity/Nakama adapterləri
Nakama match application → TypeScript rules → təsdiqli hadisələr
Təsdiqli nəticə → ratings / progression / economy
Storage adapterləri → Nakama storage → CockroachDB
```

Port burada yalnız modulun istifadə etdiyi sərhəd müqaviləsidir; konkret interface kodu yoxdur.
Qayda modulu UnityEngine, UI, şəbəkə, DB, saat və təsadüfi generatora bağlı olmur.
Zər qiyməti açıq girişdir; RNG-ni qayda modulu yaratmır.
Kompozisiya nöqtəsi adapterləri birləşdirəcək; modullar bir-birini qlobal singleton vasitəsilə axtarmamalıdır.
İlkin quruluş bir klient və bir Nakama backend-dir; əlavə mikroservis nəzərdə tutulmur.

| Modul | Sahib olduğu məsuliyyət | Giriş / çıxış və qadağan sərhəd | İlk aidiyyəti part |
|---|---|---|---|
| Long rules / Short rules | Ayrı qayda dəstləri, 24 xana/15 daş invariantı, qanuni gediş, nəticə | Position + roll + move + ruleset → yeni state / rejection; render, saxlama, RNG, ödəniş yoxdur | P01.1–P01.4 |
| Match application | Növbə, zər istifadəsi, seriya, təsdiqlənməmiş undo, pause/revanş | Oyunçu niyyəti → domain nəticəsi; UI birbaşa state dəyişmir | P01.4, P02.3–P02.4, P03.5 |
| UI/UX və presentation | Portret/landşaft yerləşim, input, Azərbaycan mətni, əlçatanlıq | Read-only state → görüntü; input → niyyət. Görüntü/zər animasiyası qaydanı dəyişmir | P02.1–P02.5, P04.5–P04.7 |
| Art/audio/motion | Vahid 2D üslub, resurs ömrü, səs/vibrasiya parametrləri | Kosmetik görünüş; gediş, reytinq və RNG-yə müdaxilə yoxdur | P04.1–P04.4 |
| Offline bot | Long/short strategiyası, 3 səviyyə, yalnız qanuni seçim | Qayda motorunun gedişlərindən seçim; zəri dəyişmir, internet/generativ API tələb etmir | P03.1–P03.4 |
| Local persistence | Versiyalı save, atomik yazma, bərpa və pozulmuş save fallback | State snapshot; server reytinqi/valyutası üçün səlahiyyət mənbəyi deyil | P02.6 |
| Network adapter | Nakama SDK bağlantısı, niyyət ötürülməsi, snapshot qəbul | roll_request/move_proposal/end_turn; klient qalib/zər/xal faktı göndərə bilməz | P05.2–P05.4 |
| Authoritative match | Server RNG, TS validation, iştirakçı/turn icazəsi, request idempotency | Təsdiqli state/events; tamaşaçı yalnız oxuyur. Node.js asılılıqları avtomatik uyğun sayılmır | P05.1–P05.5 |
| Identity/social/moderation | Guest bağlama/bərpa, dost/status, privacy, report/block | İstifadəçi icazələri; sərbəst chat, səsli chat və push dəvəti yoxdur | P06.1–P06.3, P06.7 |
| Ratings/history/replay | Long/short ayrı skill reytinqi, liqalar, təsdiqli tarixçə | Yalnız ranked random nəticəsi reytinq dəyişir; friend/bot dəyişmir; replay AI təhlil etmir | P06.4–P06.6 |
| Progression/venues/catalog | Tapşırıq/uğur/streak, kosmetika və məkan açılması | Təsdiqli hadisə → irəliləyiş; kosmetika qayda üstünlüyü vermir | P07.1–P07.6, P07.9 |
| Economy/purchases/gifts | Bir kosmetik balans, ledger, inventar, receipt/refund və transfer | Server təsdiqi, idempotent tranzaksiya; mərc və pay-to-win yoxdur | P07.7–P07.8 |
| Operations/observability | Minimal məxfi olmayan log, backup/restore, health və yük ölçmə | Lokal POC əvvəl; VPS, port və canlı sistem üçün ayrıca razılıq | P05.1, P05.6, P08 |

Cədvəldəki sətirlər məntiqi məsuliyyətlərdir; hər sətir üçün ayrıca assembly,
paket, layihə və ya xidmət yaratmaq tələbi deyil. Əvvəl konkret part üçün ən kiçik
modul qurulur. Adapter/interface yalnız real sərhəddə lazım olduqda əlavə edilir;
gələcək ehtiyac adı ilə ümumi framework və ya boş abstraksiya yazılmır.

## Müqavilələr və məlumatın sahibi

- Qayda state-i ilə UI view-state ayrı saxlanacaq. Orientation yalnız görünüşü dəyişir.
- Qayda nəticəsi, seriyada partiya qələbəsi, bacarıq reytinqi və kosmetik balans ayrı anlayışlardır.
- Offline authoritative mənbə lokal match application-dır; online son qərar serverindir.
  Klientdə göstərilən mümkün gedişlər online təsdiq sayılmır.
- GDD event sahələri: match_id, player_id, turn_no, request_id, ruleset_version, server_ts.
  C# RuleSetVersion ilə wire ruleset_version eyni versiyanı bildirəcək.
- Snapshot + təsdiqli event log reconnect və replay üçün istifadə olunacaq.
  Server restart fallback-i ayrıca testlə müəyyənləşəcək; bərpa zəmanəti hələ verilmir.
- Save formatı, qayda dəsti və network müqaviləsi ayrı versiyalaşdırılacaq.
  Uyğunsuz versiya səssiz qəbul edilməyəcək; migration/rejection testi tələb olunur.
- Secret-lər environment/secret storage-da olacaq, klientə və Git-ə daxil edilməyəcək.
  Nakama Storage üstün tutulur; əlavə SQL cədvəli zərurət sübutu tələb edir.

## Test sərhədləri

Pure C# və TS eyni versiyalı fixture-lərlə yoxlanacaq: position + roll + move → expected state/rejection.
İlkin koordinat xəritəsi və 67 fixture P01.1-də hazırlanıb. P01.2 uzun mühərriki uyğun 26 nümunəni icra edir; digər 41 nümunə öz partını gözləyir.
Differential testlər online POC-də parity göstərəcək. Botlar simulyasiya, presentation
render/input, adapterlər integration, economy təkrar əməliyyat/receipt testləri keçirəcək.
[Test matrisi](TEST_MATRIX.md) hansı yoxlamaların hazırda real olduğunu ayırır.

## Cari bootstrap və gələcək fayl yerləri

P00.3-də unity/Assets/Game altında Runtime, Editor, Input, Scenes və Tests mövcuddur.
Runtime yalnız viewport/input texniki başlanğıcıdır, Editor build alətləridir; bunlar ayrı assembly-lərlə ayrılır.
Gələcək Unity modulları unity/Assets/Game altında Rules, Bots, UI, Scenes, Network,
Animation, Audio, Art və Tests istiqamətlərinə bölünə bilər.
Backend nakama/server, ortaq fixtures nakama/fixtures, backend testləri nakama/tests;
art mənbələri art/originals, export art/exported daxilində olacaq.
Dəqiq assembly/interface/paket adları müvafiq partda seçilib yoxlanacaq.

## P01.2 konkret uzun mühərrik sərhədi

unity/Assets/Game/Rules daxilində Nard.Rules.Long assembly-si noEngineReferences=true ilə
Unity-dən ayrıdır. LongPosition immutable vəziyyətdir; massivləri giriş/çıxışda kopyalayır.
LongNardeRules ilkin mövqe, verilmiş opening/roll, lokal addım analizi və tam qanuni növbələri verir.
Tam gediş generasiyası dərinliyi ən çox dörd zər addımıdır; bütün mənbələr və fərqli qalan
zər qiymətləri araşdırılır, sonra maksimum istifadə və yüksək zər şərti tətbiq edilir.
Equal-die token permutasiyası tək təmsil olunur; oyun baxımından fərqli yollar saxlanır.
TryApplyTurn bütöv təklifi atomik yoxlayır. Lokal TryApplyLocalStep yalnız lokal şərtləri yoxlayır;
UI və ya server onu təkbaşına tam növbə təsdiqi kimi istifadə etməməlidir.
ResultPoints yalnız uzun nərd normal/mars sorğusudur; seriya, mükafat və reytinq yaratmır.
İlk növbə flag-lərinin növbəsonu dəyişməsi və növbə sahibi seçimi gələcək application qatınındır.
Qısa mühərrik eyni state-in mode flag-i kimi əlavə edilmir; ayrıca modul olacaq.
