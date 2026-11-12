# F2-B2 — Grã-Coruja das Tempestades

Desenvolvido; ativação básica testada em Play automatizado. Batalha completa e balanceamento não validados.

## Implementação
QuestTempestArena monta a luta na ArenaCume, reaproveitando PatoMago, QuestSession, QuestPlayer, QuestCampaignScene e o retorno QuestCumeStage.ReturnToPreparation. QuestRuinBoss, QuestBossArena, QuestBossWave e QuestBossExit foram inspecionados e preservados. A rajada tem controlador próprio, pois o contrato de QuestBossWave depende diretamente do golem. Não altera fase 1 ou os seus sprites.

Camera ortográfica fixa, tamanho 5,5, viewport 16:9 com barras conforme aspecto. Piso y=-2, largura 18; paredes x±9,3. Jogador inicia x=-5 com checkpoint seguro na arena. Ao perder todas as vidas, política existente restaura três vidas/pontos da tentativa e retorna ao checkpoint de preparação x98 no Cume. A introdução técnica C03.Intro é marcada uma vez; nova tentativa conserva flags e cristais, Novo Jogo limpa o estado de campanha.

## Duas ações e tempos iniciais
| Etapa | Duração/valor | Dano |
|---|---|---|
| Introdução |1,5s;0,35s após já vista|Nenhum|
| Preparar rajada |1s, pose própria e texto PULE|Nenhum|
| Lançar rajada |execução3,2s; nascimento inicia0,3s após começar e dura0,4s|Nascimento não causa dano|
| Voo da rajada |6u/s; termina na parede/limite ou após3,5s|Caixa0,8×0,45 em y=-1,65, varrida pelo passo fixo; saltável|
| Preparar mergulho |1,1s|Marca2,4u no chão; alvo copia x do jogador uma vez, limitado a±6,8; não persegue após anunciar|
| Mergulho |0,65s desde y1,8 até alvo marcado|Caixa1,5×1,2, apenas na execução|
| Pouso |0,6s, poeira e animação próprias|Nenhum|
| Vulnerável no chão |2,4s|Aceita uma magia; ao acertar fecha janela|
| Dano recebido |0,3s|Nenhum; retoma aviso da rajada|
| Libertação |1,2s; brilho0,6s|Todo dano do boss desligado; libera recompensa/saída ao terminar|

Vida 6, dano usa QuestPlayer.Ferir e sua invulnerabilidade existente. O ritmo alterna rajada e mergulho, com janela de magia após pousar; não há escalada por aumento de vida/dano ou ataques adicionais. Collider de recebimento1,5×1,8, offset(0;0,9) em poses terrestres, central durante mergulho. Magias fora da janela são consumidas sem dano. Terra mantém seu bloco normal; não foi copiada a absorção exclusiva do golem.

## Arte B1/BFX
68 quadros PNG existentes importados sem recorte/reamostragem: 46 corpo e 22 efeitos. Nove arrays de corpo, cinco de efeitos. Células512², Point, sem compressão/mipmaps; PPU 140 constante. Pivô terrestre(0,5;49/512), chão y462 contado do topo; mergulho usa centro(0,5;0,5), com âncora física compensada durante voo. Nascimento/voo/dissipação e brilho usam centro; poeira usa chão. Animações de preparação, execução, pouso, vulnerabilidade, dano e libertação são dirigidas pelo estado; efeitos permanecem separados do corpo. Manifesto exato em Assets/ElementalQuest/TempestBoss/ImportManifest.json. Ajuste visual fino entre poses fica para QA.

## Vitória, campanha e saída
Após libertação chama o mesmo contrato QuestCampaignScene.NotifyGuardianDefeated/CompletePresentation, que entrega Ar por conjunto único e registra C03. A saída física chama LeaveArena(1) para VulcaoCinzento; sem botão de prévia nesta arena. Mantém pré-requisito Terra: entrar diretamente em ArenaCume serve para testar combate, sem fabricar progresso anterior; se faltar Terra, a recompensa e a saída de campanha permanecem fechadas, com aviso e menu pela pausa. Vitória não limpa checkpoint ou cristais anteriores. C03 narrativa continua tarefa própria: deve usar esse mesmo evento/recompensa, sem concessão paralela. Não foram criadas falas adicionais.

## Checagens executadas
- Compilação C# com referências reais passou, inclusive após ajuste que oculta o voo enquanto nasce a rajada.
- Unity abriu o projeto correto, importou os assets e compilou com saída 0. Avisos preexistentes de API obsoleta nos testes de combate.
- Conferência estática: GUIDs da ArenaCume resolvem uma vez e 14 arrays têm as contagens esperadas (68 quadros).
- QuestTempestSmoke.Run executou uma ativação curta em Play e confirmou arena com vida 6, jogador, câmera ortográfica, arrays e saída inicialmente fechada. Log f2-b2-activation.log, marcador EQ_TEMPEST_ACTIVATION_OK, saída 0. Nenhuma batalha foi vencida pelo teste.

Logs: C:/Users/morei/Documents/Jogos Digitais/f2-b2-unity.log e f2-b2-activation.log. Teste permanece em Assets/Editor/QuestTempestSmoke.cs para reprodução. Não testa inputs humanos, dano real da rajada/mergulho, vitória, repetição de recompensa, esquiva, alcance de magia ou proporção visual. Esses casos e duas resoluções ficam para QA final. Sem build, commit ou push nesta tarefa.

Hashes das quatro cenas originais, dos quatro scripts do boss florestal e dos .meta preexistentes foram conferidos antes/depois da instalação, preservados. Apenas ArenaCume, novos assets/scripts de Grã-Coruja, esta nota e a linha F2-B2 foram trabalhados.
