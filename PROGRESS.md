# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P01.4`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P01.3`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P02.1`
- Növbəti parta icazə: `false`

P00.3 və Editor-un krem Game ekranı istifadəçi tərəfindən qəbul edilib.
Fiziki Android testi **NOT_RUN** olaraq ayrıca açıqdır; qəbul onun edildiyi demək deyil.
P01.1 və altı provisional qaydanın ilkin standart seçimi qəbul edilib.
P01.2 uzun və P01.3 qısa mühərrikləri qəbul edilib və qorunur.
P01.4 nəticə/seriya/matç müqaviləsi hazırdır: 263 EditMode, 1 PlayMode, 91 Python testi PASS;
Android build PASS. 67 nümunədən 63 PASS, 4 undo NOT_RUN (P02.4).
Yalnız bu əhatə tamamlanıb; [cari hesabat](docs/reports/P01.4.md).
Azərbaycan üzrə müstəqil yerli oyunçu yoxlaması NOT_RUN; PROVISIONAL_RULE qeydləri saxlanır.
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
