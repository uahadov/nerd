# NƏRD. — irəliləyiş və agentə ötürmə

Əsas status: [STATE.json](docs/STATE.json). Bu fayl onun yoxlanan insan xülasəsidir.

- Aktiv part: `P00.2`
- Status: `AWAITING_HUMAN_APPROVAL`
- Son insan təsdiqi: `P00.1`
- Cari partın insan təsdiqi: `false`
- Növbəti part: `P00.3`
- Növbəti parta icazə: `false`

P00.1 istifadəçinin 2026-11-03 tarixli açıq mesajı ilə qəbul edilib.
P00.2 üçün repo, agent müqaviləsi, inventarlar, modul sərhədləri və yoxlama sistemi hazırlanıb.
Faktiki testlər, Git/remote vəziyyəti və açıq məsələlər [cari hesabatdadır](docs/reports/P00.2.md).
P00.2 yekun auditi üçün açıq icazə var; cari hissənin qəbulu yoxdur.
Mövcud origin-ə push qadağandır; gələcək remote yalnız private və ayrıca icazə ilədir.
Unity layihəsi və oyun kodu yoxdur; Android alətləri quraşdırılmayıb.

Agent dəyişdikdə AGENTS → STATE → bu fayl → cari hesabat → mənbələr sırası ilə oxu.
Açıq məsələlər STATE-də `open_items` daxilindədir. Cari işin insan rəyini gözlə;
yeni modelə keçmək yeni parta icazə deyil. Yarımçıq/düzəliş işini yalnız cari partda davam etdir.
