# S02 — conexões de campanha

Parcial — cenas CumeDosVentos/ArenaCume, VulcaoCinzento/ArenaVulcao, CasteloDoEclipse/ArenaEclipse e Epilogo registradas. São estruturas navegáveis explícitas, sem percurso, luta ou arte novos. Prévia não concede recompensa.

QuestCampaign separa quatro cristais de fragmentos, reinicia no Novo Jogo e oferece callbacks idempotentes C02–C06. QuestCampaignScene recebe derrota, conclusão de apresentação e retorno à preparação; os controladores regionais futuros devem chamar NotifyGuardianDefeated e CompletePresentation. C00 deve usar CompletionEvent como ID único.

Saída existente do golem concede Terra uma vez e leva ao Cume, mantendo combate e retorno após derrota. Lacuna: C02 ainda deve mover a apresentação/recompensa para antes da ativação do portal conforme roteiro. Nenhuma fala paralela criada. C03–C06 e seus créditos aguardam tarefas próprias; a prévia do epílogo não declara vitória.

As quatro cenas originais e todos os metas existentes foram preservados. QA integral não executado.

Checagens rápidas executadas: compilação C# de todos os scripts de jogo passou; compilação de Editor e testes existentes passou, com avisos de API obsoleta nos testes. Os testes foram apenas compilados, não executados. Conferidos 11 registros únicos de cena e GUIDs de script das 7 cenas novas. A tentativa de abrir Unity em batch encerrou com código 1 antes de carregar o Editor; sem teste em Play ou navegação real. Log: C:/Users/morei/Documents/Jogos Digitais/s02-compile.log. QA continua pendente.
