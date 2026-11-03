# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P02.4`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P02.3`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P02.5`
- Növbəti parta icazə: `false`

P00.3 və Editor-un krem Game ekranı istifadəçi tərəfindən qəbul edilib.
Fiziki Android testi **NOT_RUN** olaraq ayrıca açıqdır; qəbul onun edildiyi demək deyil.
P01.1 və altı provisional qaydanın ilkin standart seçimi qəbul edilib.
P01.2 uzun və P01.3 qısa mühərrikləri qəbul edilib və qorunur.
P01.4 nəticə/seriya/matç müqaviləsi qəbul edilib və qorunur.
P02.1 texniki taxta qəbul edilib və əvvəlki testləri qorunur.
P02.2 seçim/marker/tap qəbul edilib; mühərrik və köhnə testlər qorunur.
P02.3 insan tərəfindən qəbul edilib və testləri qorunur.
P02.4 hazırdır: 749 EditMode, 17 PlayMode, 67/67 canonical nümunə və Android build/imza PASS.
115 Python testi və 15/15 struktur yoxlaması PASS; [P02.4 hesabatı](docs/reports/P02.4.md).
D031 son insan göstərişi ilə qoz/krem/naxışlı taxta və işləməli daş görünüşü Unity-yə tətbiq edilib.
Başqa final art/bot/online və P02.5 responsive yerləşim yoxdur.
Köhnə native Analytics/WebRequest bağlanış qəzası açıq riskdir; cari batch və ayrıca GUI exit=0, həll təsdiqlənməyib.
Azərbaycan üzrə müstəqil yerli oyunçu yoxlaması NOT_RUN; PROVISIONAL_RULE qeydləri saxlanır.
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Gələcək final taxtanın əsas [vizual referansı](docs/DECISIONS.md) D024-də saxlanıb.
Üfüqi taxta + şaquli matç dəstəyi qorunur; D031 ilə konkret taxta/daş görünüşü indi icazəlidir; başqa art və P02.5 yoxdur.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
