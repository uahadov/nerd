# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P01.1`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P00.3`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P01.2`
- Növbəti parta icazə: `false`

P00.3 və Editor-un krem Game ekranı istifadəçi tərəfindən qəbul edilib.
Fiziki Android testi **NOT_RUN** olaraq ayrıca açıqdır; qəbul onun edildiyi demək deyil.
P01.1 qayda sənədləri və 67 fixture nümunəsi hazırdır; insan rəyi gözlənilir.
Uzun yerli variant yoxlaması açıqdır. Real qayda motoru yazılmayıb.
Cari nəticələr [P01.1 hesabatındadır](docs/reports/P01.1.md).
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
