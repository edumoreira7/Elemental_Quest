# F4-L2 â€” skins do Castelo do Eclipse

Parcial â€” Gato das Sombras e Corvo Ocultista integrados; Urso de Armadura Eclipsada ainda em produÃ§Ã£o, conforme confirmado pelo usuÃ¡rio. Forte permanece GuardiaoTutorial, explicitamente provisÃ³rio. NÃ£o gerar arte nem declarar trÃªs skins prontas.

LEIA-ME, quadros idle, arrays e fÃ­sica dos templates foram inspecionados. Importados36quadros de corpo individuais256Â², sem recorte/reamostragem: cada personagem4Parado/6Andar/4Ataque/1Dano/3Derrota. Point, sem compressÃ£o/mipmaps, pivÃ´ e PPU constantes por personagem, calculados pela altura alpha do idle e altura do renderer original. Base visual alinhada ao pÃ© do collider em+0,05local, referÃªncia y232 contada do topo. Valores exatos em Assets/ElementalQuest/CastleVisual/ImportManifest.json.

Prefabs Gato das Sombras e Corvo Ocultista exclusivos do Castelo preservam tipo, vida, velocidades, perseguiÃ§Ã£o, Transform, Rigidbody2D e BoxCollider2D dos arquÃ©tipos originais. QuestEnemy continua dirigindo os cinco arrays; nenhum Animator ou Animation Event novo, nenhum disparo duplicado. As duas apariÃ§Ãµes de corpo a corpo e de atirador no percurso recebem as skins pelas referÃªncias da cena.

SombraCastelo usa os seis quadros de Sombra_Projetil e seis de Dissipacao disponÃ­veis; PPU180 e pivÃ´ central. Conserva QuestEnemyShot, collider, velocidade/lifetime e feedback visual do projÃ©til existente. Tempos reais de combate foram preservados, sem ajustar a IA Ã s sugestÃµes de FPS dos arquivos de arte.

Pedra fria clara e bordas de pouso, vitrais apagados, sombras violetas e decoraÃ§Ã£o sem collider permanecem como em F4-L1. Nenhuma arte especÃ­fica adicional do castelo estava disponÃ­vel nessa pasta; nÃ£o foram alterados pixels ou usadas skins de outra regiÃ£o como substituta da arte final do Urso.20fragmentos mantidos em lista Ãºnica, HUD baseado na criaÃ§Ã£o real.

Checagens: comparados campos de comportamento e blocos integrais de Transform/Rigidbody2D/BoxCollider2D das duas skins, iguais aos templates. GUIDs da cena e novos prefabs resolvem uma vez. Unity compilou/importou e checagem curta em Play passou, confirmando jogador, cinco inimigos/trÃªs arquÃ©tipos, Gato/Corvo e projÃ©til referenciados,20fragmentos/HUD, sete plataformas e uma raiz. Marcador EQ_CASTLE_ACTIVATION_OK em C:/Users/morei/Documents/Jogos Digitais/f4-l2-activation.log. NÃ£o houve travessia nem combate completo.

Placeholders restantes: acabamento geomÃ©trico do piso/vitrais/colunas, placas, fragmentos, raÃ­zes e portal. Umbrius e arena sÃ£o tarefas prÃ³prias. QA final pendente: animaÃ§Ãµes e pÃ©s em movimento, contraste sobre cenÃ¡rio real em duas resoluÃ§Ãµes, disparo/dissipaÃ§Ã£o e travessia completa.

Fase1, QuestEnemy, QuestCastleStage e todos os .meta preexistentes tiveram hashes conferidos na instalaÃ§Ã£o, iguais. Sem build, commit ou push.

## Conclusão — Urso integrado
19 quadros individuais320×320, sem recorte ou reamostragem: Parado4, Andar6, Ataque5, Dano1, Derrota3. LEIA-ME, idle, arrays, importer e física do forte original inspecionados. A célula maior adiciona margem transparente, não muda a escala por pose. Point, sem compressão/mipmaps, PPU199,1203777constante e pivô(0,5;0,06576244), base visual alinhada em+0,05local; referência de chão y288. Transform continua escala1,5. Valores exatos no ImportManifest.json.

UrsoArmaduraEclipsada.prefab é exclusivo do Castelo e substitui o forte provisório na cena. Transform, Rigidbody2D, BoxCollider2D e campos de comportamento foram comparados ao original e preservados. QuestEnemy e todos os .meta preexistentes permaneceram iguais na instalação, assim como as quatro cenas originais.

Compilação/importação e nova ativação curta em Play passaram com saída0 e EQ_CASTLE_ACTIVATION_OK, confirmando três skins, cinco inimigos, Urso com cinco quadros de ataque e20fragmentos/HUD. Referências GUID da cena e prefab do Urso resolvem uma vez. Log: C:/Users/morei/Documents/Jogos Digitais/f4-l2-final-activation.log. Não representa combate ou travessia completos; conferir animações/contraste em QA. Sem build, commit ou push. A luta de Umbrius e o encerramento narrativo seguem tarefas próprias.
