# F3-B2 — Salamandra de Magma

Desenvolvido; compilação, referências e ativação básica em Play passaram. Batalha completa, controles humanos e balanceamento pendentes para QA.

QuestMagmaArena monta ArenaVulcao com PatoMago, QuestSession/QuestPlayer e QuestCampaignScene. Os contratos QuestRuinBoss, QuestBossArena, QuestBossWave e QuestBossExit foram lidos e preservados. O projétil parabólico tem controlador próprio, sem reaproveitar a dependência da onda com o golem. Câmera fixa ortográfica5,5, viewport16:9, piso y=-2 de18unidades e paredes±9,3. Jogador começa x=-5 em checkpoint seguro; perder todas as vidas retorna à preparação x98 do Vulcão pelo contrato existente, restaurando a tentativa sem limpar cristais. Intro técnica C04.Intro não se repete após vista; Novo Jogo limpa o estado.

## Tempos e hitboxes iniciais
| Etapa | Parâmetro | Dano |
|---|---|---|
| Introdução |1,5s;0,35s em repetição|Nenhum|
| Aviso de cuspe |1s, pose de garganta e marca do destino no chão|Nenhum|
| Cuspir |2,2s; efeito nasce0,3s após início|Nenhum dano do nascimento|
| Nascimento |0,4s|Visual separado, voo oculto até terminar|
| Voo |1,15s, interpola origem até destino capturado no aviso, arco com altura adicional1,8|Caixa0,6×0,6 varrida pelo deslocamento; impacto termina o projétil, sem poça persistente ou explosão adicional|
| Aviso de avanço |1,1s; marca fixa, direção capturada no início|Nenhum|
| Avançar |0,75s,3,2u limitadas a x±6|Caixa2,1×1,1 centrada em pés+(0;0,55), apenas durante execução|
| Frear |0,6s; cinza0,34s|Nenhum|
| Vulnerável |2,4s no chão|Aceita uma magia, fecha a janela ao acertar|
| Receber dano |0,3s|Recomeça o aviso do cuspe|
| Libertação |1,2s; brilho0,6s|Collider de dano recebido desligado, projétil interrompido, recompensa/saída após animação|

Vida6; dano recebido pelo jogador usa QuestPlayer.Ferir e sua invulnerabilidade existente. Collider de recebimento2,6×1,2 com offset(0;0,6), âncora terrestre comum a todas as ações. Não existe aumento de vida/dano por etapa, ataque extra, propagação de lava ou mecânica exclusiva para animação Andar. A lista Andar foi importada como recurso disponível; as duas ações são somente cuspe e avanço. Terra não cura a Salamandra: a absorção continua exclusiva do golem.

## Animações e efeitos
80quadros existentes copiados sem recortar, reamostrar ou alterar fontes:56corpo e24efeitos, células512×320. Point, sem compressão/mipmaps, PPU110constante. Corpo e cinza usam pivô(0,60546875;0,1), compatível com apoio(310;288) informado nas notas; magma e brilho usam centro(0,5;0,5). Sem ajuste dinâmico de escala por quadro. Arrays de preparação, execução, frenagem, vulnerabilidade, dano e libertação seguem os tempos de estado; efeitos de nascimento/voo/impacto são objetos distintos. Manifesto em Assets/ElementalQuest/MagmaBoss/ImportManifest.json.

## Recompensa e integração
Libertação usa QuestCampaignScene.NotifyGuardianDefeated/CompletePresentation: Fogo por conjunto único e evento C04 compartilhado. Saída física leva ao CasteloDoEclipse via LeaveArena(2), sem botão de pular luta. Requer Terra e Ar anteriores, conforme campanha. Entrada direta permite ativar/testar a luta, mas não fabrica cristais prévios: faltando pré-requisitos, há aviso e o menu segue acessível pela pausa. C04 narrativa continua tarefa própria e deve reutilizar o mesmo evento/recompensa, sem conceder Fogo novamente. Sem falas paralelas nesta tarefa.

## Checagens executadas
Compilador C# com referências locais passou. GUIDs da ArenaVulcao resolvem uma vez. Unity importou/compilou e QuestMagmaSmoke.Run confirmou em Play: arena com vida6, jogador, câmera ortográfica, arrays carregados e saída inicialmente fechada; marcador EQ_MAGMA_ACTIVATION_OK. Log: C:/Users/morei/Documents/Jogos Digitais/f3-b2-activation.log. O teste não simula a vitória nem percorre a campanha.

QA pendente: trajetória/dano com inputs reais, salto/recuo do avanço, sincronização visual, janela de magia, vitória/recompensa única, retorno após derrota, transições, proporção dos sprites e duas resoluções. Piso, paredes, marcas e portal usam formas locais provisórias; o corpo e os cinco efeitos são arte final disponível. Nenhum build, commit ou push nesta tarefa.

Hashes da fase1, arenas anteriores, quatro scripts do golem, controlador da Coruja e .meta preexistentes permaneceram iguais na instalação. Alterações locais das tarefas anteriores preservadas.
