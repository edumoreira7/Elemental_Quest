# S03 — HUD e fechamento da campanha

Desenvolvido no escopo solicitado: sem cutscenes, que serão feitas depois pelo usuário. Abertura/encerramento são telas curtas de texto e botões, não sequências cinematográficas. C01/C06 não são marcadas como vistas por essas telas.

QuestMainMenu atualiza o convite para quatro regiões. Jogar prepara Nova Partida e mostra o resumo da missão; Iniciar Jornada carrega FasePrincipal. CanvasGroup bloqueia os botões do menu enquanto a abertura está visível; ao continuar, restaura interação. Tutorial permanece separado; NovoJogo direto mantém entrada direta na floresta, inclusive no botão do epílogo. Não foram regravadas cenas nem eventos persistentes dos botões.

QuestSession/QuestHudArt mostram região1–4 e título, fragmentos locais somente em cenas com totalItens>0, encontro do guardião em arenas sem coleta, pontos e cristais da campanha0–4 com nomes Terra/Ar/Fogo/Gelo e marca individual. Mana, corações, números1–4, destaque do elemento selecionado e os poderes existentes foram preservados. Nenhum cristal é concedido pelo HUD ou pela coleta de fragmentos. Layout usa a escala1280×720 atual; revisão visual em duas resoluções fica para QA.

Epilogo agora mostra texto curto de restauração e reencontro, botão Menu e Novo Jogo. Só conclui com quatro cristais e C05 já concluído. Sem progresso suficiente mostra instrução, sem conceder recompensa. QuestCampaign separa Campaign.Ending da flag C06: a campanha termina sem afirmar que a futura cutscene foi vista. Venceu e Complete são atualizados pela mesma rotina idempotente; Novo Jogo limpa cristais, eventos, pontos/vidas e checkpoints das quatro regiões. Menu preserva o estado até iniciar Nova Partida.

Checagem de navegação em Play automatizado passou: Inicio/Jogar/abertura → FasePrincipal com sessão e poderes → quatro recompensas preparadas diretamente pelo teste → Epilogo com conclusão e sem C06 vista → NovoJogo limpo na floresta. Marcador EQ_CAMPAIGN_HUD_OK, log C:/Users/morei/Documents/Jogos Digitais/s03-navigation.log. Essa preparação de cristais não é batalha vencida ou jornada jogada. Unity compilou os scripts. A verificação não certifica interação humana, aparência final, controles em duas resoluções ou campanha completa.

Quatro cenas originais e .meta existentes tiveram hashes preservados na instalação. QuestMainMenu, QuestSession, QuestHudArt, QuestCampaign e QuestCampaignScene alterados de forma localizada; checagem em Assets/Editor/QuestCampaignHudSmoke.cs. Sem arte, dublagem, cutscene, créditos inventados, build, commit ou push.

Pendentes em tarefas próprias/QA: C00/C01–C06 cinematográficas, créditos/fontes finais, revisão visual completa, controles reais, narrativa das saídas e jornada completa.
