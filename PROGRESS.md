# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P00.3`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P00.2`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P01.1`
- Növbəti parta icazə: `false`

P00.2 istifadəçinin 2026-11-03 tarixli açıq mesajı ilə qəbul edilib.
Yalnız P00.3: Unity 6000.6.7f1 minimal 2D layihəsi, Android alətləri, ilkin build/test infrastrukturu.
Real nəticələr və məhdudiyyətlər [cari hesabatdadır](docs/reports/P00.3.md).
Nərd qaydaları, UI/UX ekranları, bot və multiplayer yaradılmır.
Mövcud origin-ə push qadağandır; private görünüş avtomatik icazə deyil.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də open_items daxilindədir. Cari hissənin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil.
