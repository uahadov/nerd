# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P02.2`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P02.1`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P02.3`
- Növbəti parta icazə: `false`

P00.3 və Editor-un krem Game ekranı istifadəçi tərəfindən qəbul edilib.
Fiziki Android testi **NOT_RUN** olaraq ayrıca açıqdır; qəbul onun edildiyi demək deyil.
P01.1 və altı provisional qaydanın ilkin standart seçimi qəbul edilib.
P01.2 uzun və P01.3 qısa mühərrikləri qəbul edilib və qorunur.
P01.4 nəticə/seriya/matç müqaviləsi qəbul edilib və qorunur.
P02.1 texniki taxta qəbul edilib və əvvəlki testləri qorunur.
P02.2 seçim/marker/tap gedişi hazırdır: 356 EditMode, 7 PlayMode, 105 Python testi və Android build PASS; [cari hesabat](docs/reports/P02.2.md).
Sabit zər nümunəsidir; atış və yeni növbə dövrü hələ yoxdur.
Əvvəldən açıq Editor-un native Analytics bağlanış qəzası açıq qeyddir; P02.2 ayrıca GUI input/preview exit=0, səbəb və həll təsdiqlənməyib.
Azərbaycan üzrə müstəqil yerli oyunçu yoxlaması NOT_RUN; PROVISIONAL_RULE qeydləri saxlanır.
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Gələcək final taxtanın əsas [vizual referansı](docs/DECISIONS.md) D024-də saxlanıb.
Üfüqi taxta + şaquli matç dəstəyi qorunur; hazırda art/kod dəyişikliyinə icazə yoxdur.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
