# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P02.5`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P02.4`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P02.6`
- Növbəti parta icazə: `false`

P00.3 və Editor-un krem Game ekranı istifadəçi tərəfindən qəbul edilib.
Fiziki Android testi **NOT_RUN** olaraq ayrıca açıqdır; qəbul onun edildiyi demək deyil.
P01.1 və altı provisional qaydanın ilkin standart seçimi qəbul edilib.
P01.2 uzun və P01.3 qısa mühərrikləri qəbul edilib və qorunur.
P01.4 nəticə/seriya/matç müqaviləsi qəbul edilib və qorunur.
P02.1 texniki taxta qəbul edilib və əvvəlki testləri qorunur.
P02.2 seçim/marker/tap qəbul edilib; mühərrik və köhnə testlər qorunur.
P02.3 insan tərəfindən qəbul edilib və testləri qorunur.
P02.4 funksional nəticələri: 749 EditMode, 17 PlayMode, 67/67 canonical nümunə və Android build/imza PASS.
P02.4 funksiyaları insan tərəfindən qəbul edilib; rədd edilmiş layout D032 brifi üzrə yalnız P02.5-də işlənir.
D033 P02.5-ə açıq insan icazəsidir. D034 P02.5 portret/HUD düzəlişi: 779 EditMode, 24 PlayMode, Android build/imza və ayrıca Editor input/render PASS; insan görünüş qəbulu gözlənilir.
P02.5: 118 Python testi və 15/15 struktur yoxlaması PASS; [cari hesabat](docs/reports/P02.5.md).
D031 son insan göstərişi ilə qoz/krem/naxışlı taxta və işləməli daş görünüşü Unity-yə tətbiq edilib.
P02.5-də iki responsive yerləşim var; başqa final art/bot/online və P02.6 yoxdur.
Köhnə native Analytics/WebRequest bağlanış qəzası açıq riskdir; cari batch və ayrıca GUI exit=0, həll təsdiqlənməyib.
Azərbaycan üzrə müstəqil yerli oyunçu yoxlaması NOT_RUN; PROVISIONAL_RULE qeydləri saxlanır.
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Gələcək final taxtanın əsas [vizual referansı](docs/DECISIONS.md) D024-də saxlanıb.
Üfüqi taxta + şaquli matç dəstəyi qorunur; D031 ilə konkret taxta/daş görünüşü indi icazəlidir; yalnız P02.5 layout işi icazəlidir, başqa art və P02.6 yoxdur.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
