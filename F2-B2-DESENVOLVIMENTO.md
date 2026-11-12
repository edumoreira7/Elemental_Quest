# F2-B2 â€” GrÃ£-Coruja das Tempestades

Desenvolvido; ativaÃ§Ã£o bÃ¡sica testada em Play automatizado. Batalha completa e balanceamento nÃ£o validados.

## ImplementaÃ§Ã£o
QuestTempestArena monta a luta na ArenaCume, reaproveitando PatoMago, QuestSession, QuestPlayer, QuestCampaignScene e o retorno QuestCumeStage.ReturnToPreparation. QuestRuinBoss, QuestBossArena, QuestBossWave e QuestBossExit foram inspecionados e preservados. A rajada tem controlador prÃ³prio, pois o contrato de QuestBossWave depende diretamente do golem. NÃ£o altera fase 1 ou os seus sprites.

Camera ortogrÃ¡fica fixa, tamanho 5,5, viewport 16:9 com barras conforme aspecto. Piso y=-2, largura 18; paredes xÂ±9,3. Jogador inicia x=-5 com checkpoint seguro na arena. Ao perder todas as vidas, polÃ­tica existente restaura trÃªs vidas/pontos da tentativa e retorna ao checkpoint de preparaÃ§Ã£o x98 no Cume. A introduÃ§Ã£o tÃ©cnica C03.Intro Ã© marcada uma vez; nova tentativa conserva flags e cristais, Novo Jogo limpa o estado de campanha.

## Duas aÃ§Ãµes e tempos iniciais
| Etapa | DuraÃ§Ã£o/valor | Dano |
|---|---|---|
| IntroduÃ§Ã£o |1,5s;0,35s apÃ³s jÃ¡ vista|Nenhum|
| Preparar rajada |1s, pose prÃ³pria e texto PULE|Nenhum|
| LanÃ§ar rajada |execuÃ§Ã£o3,2s; nascimento inicia0,3s apÃ³s comeÃ§ar e dura0,4s|Nascimento nÃ£o causa dano|
| Voo da rajada |6u/s; termina na parede/limite ou apÃ³s3,5s|Caixa0,8Ã—0,45 em y=-1,65, varrida pelo passo fixo; saltÃ¡vel|
| Preparar mergulho |1,1s|Marca2,4u no chÃ£o; alvo copia x do jogador uma vez, limitado aÂ±6,8; nÃ£o persegue apÃ³s anunciar|
| Mergulho |0,65s desde y1,8 atÃ© alvo marcado|Caixa1,5Ã—1,2, apenas na execuÃ§Ã£o|
| Pouso |0,6s, poeira e animaÃ§Ã£o prÃ³prias|Nenhum|
| VulnerÃ¡vel no chÃ£o |2,4s|Aceita uma magia; ao acertar fecha janela|
| Dano recebido |0,3s|Nenhum; retoma aviso da rajada|
| LibertaÃ§Ã£o |1,2s; brilho0,6s|Todo dano do boss desligado; libera recompensa/saÃ­da ao terminar|

Vida 6, dano usa QuestPlayer.Ferir e sua invulnerabilidade existente. O ritmo alterna rajada e mergulho, com janela de magia apÃ³s pousar; nÃ£o hÃ¡ escalada por aumento de vida/dano ou ataques adicionais. Collider de recebimento1,5Ã—1,8, offset(0;0,9) em poses terrestres, central durante mergulho. Magias fora da janela sÃ£o consumidas sem dano. Terra mantÃ©m seu bloco normal; nÃ£o foi copiada a absorÃ§Ã£o exclusiva do golem.

## Arte B1/BFX
68 quadros PNG existentes importados sem recorte/reamostragem: 46 corpo e 22 efeitos. Nove arrays de corpo, cinco de efeitos. CÃ©lulas512Â², Point, sem compressÃ£o/mipmaps; PPU 140 constante. PivÃ´ terrestre(0,5;49/512), chÃ£o y462 contado do topo; mergulho usa centro(0,5;0,5), com Ã¢ncora fÃ­sica compensada durante voo. Nascimento/voo/dissipaÃ§Ã£o e brilho usam centro; poeira usa chÃ£o. AnimaÃ§Ãµes de preparaÃ§Ã£o, execuÃ§Ã£o, pouso, vulnerabilidade, dano e libertaÃ§Ã£o sÃ£o dirigidas pelo estado; efeitos permanecem separados do corpo. Manifesto exato em Assets/ElementalQuest/TempestBoss/ImportManifest.json. Ajuste visual fino entre poses fica para QA.

## VitÃ³ria, campanha e saÃ­da
ApÃ³s libertaÃ§Ã£o chama o mesmo contrato QuestCampaignScene.NotifyGuardianDefeated/CompletePresentation, que entrega Ar por conjunto Ãºnico e registra C03. A saÃ­da fÃ­sica chama LeaveArena(1) para VulcaoCinzento; sem botÃ£o de prÃ©via nesta arena. MantÃ©m prÃ©-requisito Terra: entrar diretamente em ArenaCume serve para testar combate, sem fabricar progresso anterior; se faltar Terra, a recompensa e a saÃ­da de campanha permanecem fechadas, com aviso e menu pela pausa. VitÃ³ria nÃ£o limpa checkpoint ou cristais anteriores. C03 narrativa continua tarefa prÃ³pria: deve usar esse mesmo evento/recompensa, sem concessÃ£o paralela. NÃ£o foram criadas falas adicionais.

## Checagens executadas
- CompilaÃ§Ã£o C# com referÃªncias reais passou, inclusive apÃ³s ajuste que oculta o voo enquanto nasce a rajada.
- Unity abriu o projeto correto, importou os assets e compilou com saÃ­da 0. Avisos preexistentes de API obsoleta nos testes de combate.
- ConferÃªncia estÃ¡tica: GUIDs da ArenaCume resolvem uma vez e 14 arrays tÃªm as contagens esperadas (68 quadros).
- QuestTempestSmoke.Run executou uma ativaÃ§Ã£o curta em Play e confirmou arena com vida 6, jogador, cÃ¢mera ortogrÃ¡fica, arrays e saÃ­da inicialmente fechada. Log f2-b2-activation.log, marcador EQ_TEMPEST_ACTIVATION_OK, saÃ­da 0. Nenhuma batalha foi vencida pelo teste.

Logs: C:/Users/morei/Documents/Jogos Digitais/f2-b2-unity.log e f2-b2-activation.log. Teste permanece em Assets/Editor/QuestTempestSmoke.cs para reproduÃ§Ã£o. NÃ£o testa inputs humanos, dano real da rajada/mergulho, vitÃ³ria, repetiÃ§Ã£o de recompensa, esquiva, alcance de magia ou proporÃ§Ã£o visual. Esses casos e duas resoluÃ§Ãµes ficam para QA final. Sem build, commit ou push nesta tarefa.

Hashes das quatro cenas originais, dos quatro scripts do boss florestal e dos .meta preexistentes foram conferidos antes/depois da instalaÃ§Ã£o, preservados. Apenas ArenaCume, novos assets/scripts de GrÃ£-Coruja, esta nota e a linha F2-B2 foram trabalhados.

Revisão de integração visual: ver AUDITORIA-SPRITES.md. Âncora física/visual, subida e direção do mergulho corrigidas; nenhum PNG ou .meta alterado. A revisão atual ainda não foi testada em Play.
