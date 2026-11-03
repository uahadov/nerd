# Modulyar arxitektura — gələcək implementasiya sərhədləri

Əsas: [GDD §4, §5, §10](../NARD_Final_GDD_v1.0.md) və
[Master Planın ardıcıllığı](../NARD_Master_Icra_Plani_v2.md).
P00.2-də qəbul edilmiş modul sərhədləridir; P00.3 texniki bootstrap əlavə edib.
P01.2 uzun modulu qəbul edilib; P01.3 ShortPosition/ShortNardeRules ayrıca pure C# assembly-dir.
Bot/backend yoxdur. P02.1-də aşağıda göstərilən read-only texniki presentation var.
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
İlkin koordinat xəritəsi və 67 fixture P01.1-də hazırlanıb. Uzun mühərrik 26, qısa mühərrik 30 nümunəni icra edir; P01.4 matç modulu 7 seriya/reytinq nümunəsini icra edir. Yalnız 4 undo nümunəsi P02.4-ü gözləyir.
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
Qısa mühərrik eyni state-in mode flag-i deyil; aşağıda göstərilən ayrıca moduldur.

## P01.3 konkret qısa mühərrik sərhədi

Rules/Short daxilində ayrıca Nard.Rules.Short assembly-si noEngineReferences=true ilə qurulur.
Long assembly-yə reference yoxdur; qəbul edilmiş uzun runtime/test faylları dəyişməyib.
ShortPosition yalnız points/bar/off/roll, owner/phase/usedMask saxlayır və massivləri kopyalayır.
ShortMove.Bar=-1, Off=24 sentinel-dir; bar/off 24 xananın içində deyil. P0 azalan, P1 artan indekslə gedir.
ResolveOpening ilkin iki qiyməti saxlayır; qısa mühərrik təzə ilk roll yaratmır.
TryApplyLocalStep yalnız lokal şərt analizi; LegalTurns/TryApplyTurn tam maksimum/yüksək zər müqaviləsidir.
Bar boşalmadan başqa daş oynanmır; vurulan daş ayrıca rəqib bar sayına keçir.
ResultPoints yalnız hazır mövqenin 0/1/2/3 nəticə sorğusudur. Seriya, reytinq, event, server təsdiqi
və tətbiqin növbə dəyişməsi P01.4/sonrakı hissələrə aiddir; qısa modul bunu qabaqcadan qurmur.
P01.1 JSON-dakı first_turn/head_departures qısa qaydanın davranışını dəyişmir; uzun baş qaydası daşınmır.

## P01.4 konkret matç sərhədi

Rules/Match daxilində Nard.Match pure C# assembly-si yalnız iki qayda assembly-sinə bağlıdır.
MatchSession bir partiyanın dəyişməyən snapshot-udur; hər qəbul edilən əməl yeni snapshot qaytarır.
Axın: Created → Rolling → Moving → TurnCommitted → Rolling; qalib növbə Moving → Finished-dir.
Begin ilkin atışı açır. Bərabər ilkin atış Rolling-də qalır. Uzunda qalib təzə iki zər alır;
qısada ilkin iki qiymətlə Moving başlayır. Zərlər girişdir; modul RNG yaratmır.
TryCommitTurn tam növbəni mövcud mühərriklə atomik yoxlayır; rədd giriş snapshot-unu dəyişmir.
Bu lokal domain commit-dir, şəbəkə/server təsdiqi deyil. Uzunda ilk növbə flag-i yalnız qəbulda dəyişir.
AdvanceTurn növbə sahibini dəyişir; Rolling zamanı engine position əvvəlki committed snapshot olaraq
qalır. Cari növbə sahibi MatchSession.Player-dir; SupplyRoll yeni oyunçunun aktiv position-unu qurur.
Yalnız aktiv rejimin position-u mövcuddur; digər rejimin sahəsi null-dır.

GameResult bitmiş engine mövqeyindən qalib, WinKind və Points çıxarır; hazır xal qəbul edən açıq
konstruktor yoxdur. Normal=1, mars=2, yalnız qısada koks=3; qoşa zər xalı artırmır.
MatchSeries yalnız eyni rejim/RuleSetVersion üçün 1/3/5 best-of nəticələri saxlayır.
Seriya qələbələri və partiya xalları ayrıdır: mars/koks bir partiya qələbəsidir.
Təkrar GameId, başqa rejim/versiya və bitmiş seriyaya əlavə rədd edilir.
RuleContract hər rejimin mövcud versiyasını dəqiq tələb edir; səssiz fallback yoxdur.
RatingPolicy yalnız random + ranked + authoritative confirmation üçün eligibility qaytarır.
Confirmation gələcək etibarlı səlahiyyət mənbəyindən gəlməlidir; klient flag-i online sübut deyil.
Həqiqi reytinq, server autentifikasiyası, hadisə/saxlama, preview/undo, UI, bot və avtomatik
növbəti partiyanın başlanması bu modulda yoxdur. P02 funksiyaları ayrıca insan icazəsini gözləyir.

## P02.1 texniki presentation sərhədi

Nard.Presentation yalnız Nard.Match və iki pure qayda assembly-sinə bağlıdır; əks asılılıq yoxdur.
BoardSnapshot mövcud engine/match state-indən points/bar/off-un read-only surətini alır.
BoardGeometry yalnız board-global-v1 indeksini yerləşimə çevirir; gediş, məsafə və ev qaydası hesablamır.
BoardView snapshot-u SpriteRenderer/TextMesh ilə göstərir; açıq Display çağırışı yeni snapshot-u çəkir.
Köhnə görünüş əvvəl deaktiv edilir ki, eyni kadrda təkrar render gizli/təkrar daş yaratmasın.
Bir stack-in bütün 15 daşı ayrıca görünür; bar və off ayrıca render edilir. Uzunda bar həmişə sıfır gəlir.
Zər qiyməti, seçim, marker, atış düyməsi, növbə idarəsi və gameplay input-u bu qatda hələ yoxdur.
Qrafika kodla yaranır; gələcək sprite/skin dəyişməsi qayda mühərriklərini dəyişdirməyi tələb etmir.
TechnicalBoard ayrı scene-dir. Bir portret kompozisiya letterbox olunur; P02.5 responsive layout-u deyil.
Əvvəlki Bootstrap və EditorBuildSettings qorunur. P021Build yalnız açıq build üçün TechnicalBoard,
sonra Bootstrap scene siyahısını verir. P021Preview yalnız açıq CLI preview sorğusunda Game View çəkir.

## P02.2 seçim application qatı

Nard.Interaction pure C# qatıdır; Rules/Match-dan asılıdır, Unity-dən asılı deyil.
TapSelection ilkin state-in bütün LegalTurns yollarını alır, seçilmiş mənbənin yalnız növbəti
qanuni hədəflərini verir. Qəbuldan sonra uyğun qalan yolların prefiksini saxlayır; greedy lokal
addımla maksimum/yüksək zər qaydasını pozmağa yol vermir. Qayda hesablaması engine-də qalır.
Bütün qəbul edilmiş addımlar uncommitted preview-dur; UI bunu server qəbuluna və ya matç nəticəsinə çevirmir.
BoardTapInput → TapSelection → mövcud Long/Short engine. BoardView/BoardHitMap → yalnız geometry/render.
Input adapteri Mouse və primary Touchscreen press-i oxuyur; press ID-si ikinci dəfə işlənmir.
İki barmaq və drag hərəkəti gediş deyil. Rədd feedback-i bloklamayan sadə Azərbaycan mətnidir.
TapBoard ayrıca test scene-dir; sabit roll setup + aydın reset nümunəsi var. Atış/növbə dövrü,
undo, bot və online adapterləri qabaqcadan yazılmır. Final vizual referans D024 toxunulmazdır.
