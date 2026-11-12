# F3-L1/F3-L2 â€” VulcÃ£o Cinzento

F3-L1 desenvolvido. F3-L2 parcial: Raposa das Cinzas e Lagarto de Brasa integrados; Javali de Basalto em produÃ§Ã£o conforme informado pelo usuÃ¡rio, ainda sem sprites para integrar. Forte usa GuardiaoTutorial original como placeholder. Salamandra e luta sÃ£o F3-B2, fora desta entrega.

## Percurso e poderes reais
VulcaoCinzento monta o percurso em Play via QuestVolcanoStage, usando o mesmo padrÃ£o de sessÃ£o/jogador/cÃ¢mera, plataformas com colliders contÃ­nuos e encontros delimitados do Cume/floresta. Nenhuma cena anterior foi reconstruÃ­da. Seis pisos principais: x0â€“12,14â€“33,36â€“50,52â€“70,72â€“89,92â€“112; todos topoy=-2. Bordas claras delimitam basalto sÃ³lido, decoraÃ§Ã£o escura nÃ£o tem collider. VÃ£os principais de2â€“3u mantÃªm o prÃ³ximo pouso visÃ­vel. Piso alto opcional x55â€“59, y1: acesso por bloco temporÃ¡rio Terra ou Ar, passagem inferior livre.

Entrada segura â†’ lava marcada isolada x12â€“14 â†’ corpo a corpo x23â€“31 â†’ atirador x41â€“48 â†’ checkpoint/respiro x53 â†’ forte x59â€“66 â†’ raÃ­zes carbonizadas x68 â†’ lava marcada x70â€“72 â†’ combinaÃ§Ã£o curta x76â€“87 â†’ checkpoint/preparaÃ§Ã£o x98 â†’ trigger fÃ­sico x107 para ArenaVulcao. Checkpoints adicionais x18 e53. Retorno apÃ³s derrota da arena usa x98; Novo Jogo limpa somente o estado local correspondente junto aos demais progressos. NÃ£o entrega Fogo nem dispara C04 ao entrar.

As duas superfÃ­cies de lava usam QuestElementTarget.Tipo.Lava jÃ¡ existente, BoxCollider2D trigger2Ã—0,3 centrado em y=-2,15. Gelo torna piso por8s e pode ser reconjurado; jogador tem Ã¡rea segura para regenerar mana. Nenhum congelamento genÃ©rico de cenÃ¡rio ou vento novo. RaÃ­zes usam Tipo.Raizes, collider0,6Ã—2,4, removidas por Fogo. Ar permanece salto extra/planar1,2s; Terra bloco8s. Placas explicam seleÃ§Ã£o1â€“4/J, piso seguro e alvos. Fissuras de fundo sÃ£o exclusivamente decorativas.

20 posiÃ§Ãµes Ãºnicas de fragmentos opcionais, lista Ãºnica compartilhada pela criaÃ§Ã£o e total do HUD; sem relaÃ§Ã£o com os quatro cristais narrativos. Mantidos cinco inimigos e comportamento QuestEnemy; ataques sÃ³ permitidos na seÃ§Ã£o e no enquadramento.

## Arte e importaÃ§Ã£o
Importadas cÃ³pias de36quadros de personagem: Raposa4/6/4/1/3 e Lagarto4/6/4/1/3. Arrays parado/andar/atacar/dano/morte do QuestEnemy, sem Animator novo e sem novos eventos de disparo.12quadros extras para Brasa_Projetil/Dissipacao. ProjÃ©til reutiliza o contrato de QuestEnemyShot e feedback visual existente, com velocidade/tempo/collider preservados.

Quadros individuais256Â² jÃ¡ preparados; sem recorte ou reamostragem. Conferidos LEIA-ME, importer existente, arrays, escala e base do collider antes de importar. Point, sem compressÃ£o/mipmaps; PPUconstante por personagem calibrado pela silhueta do idle e altura original. PivÃ´ ajustado para base visual em+0,05local; pÃ©s y232 contado do topo. Valores exatos em Assets/ElementalQuest/VolcanoVisual/ImportManifest.json. Vida, tipo, velocidades, patrulha e fÃ­sica dos templates foram preservados. Sprites originais nÃ£o alterados.

Placeholders listados: forte/GuardiaoTutorial atÃ© Javali; basalto, cinzas, fissuras, raÃ­zes, lava marcada, fragmentos e placas em formas locais; ArenaVulcao ainda Ã© estruturaS02 atÃ© F3-B2. NÃ£o hÃ¡ fundo especÃ­fico do vulcÃ£o entre os backgrounds disponÃ­veis consultados, portanto nÃ£o foi usado o cÃ©u azul do Cume como ambientaÃ§Ã£o final.

## Checagens
CompilaÃ§Ã£o C# dos scripts de jogo com referÃªncias locais passou. GUIDs da cena/prefabs resolvem uma vez. Campos de comportamento e blocos Rigidbody2D/BoxCollider2D das duas skins comparados aos templates, iguais. Hashes das seis cenas anteriores e todos os .meta preexistentes permaneceram iguais durante a instalaÃ§Ã£o. Fase1 e trabalho local da Coruja preservados.

Checagem curta em Play passou (EQ_VOLCANO_ACTIVATION_OK): jogador, cinco inimigos, duas skins,20fragmentos e total do HUD, duas lavas inicialmente trigger e solidificaÃ§Ã£o por chamada real de Aplicar(Gelo), uma raiz. O teste nÃ£o lanÃ§ou magia por input e nÃ£o percorreu a fase.

Log: C:/Users/morei/Documents/Jogos Digitais/f3-activation.log. QA final pendente: travessia real, mana/alcance, temporizaÃ§Ã£o de8s, dano real de lava, raÃ­zes, saltos, retorno, arena, escala visual/animaÃ§Ãµes e contraste em duas resoluÃ§Ãµes. Sem build, commit ou push.

## Conclusão de F3-L2 — Javali integrado
Importados 19 quadros individuais já preparados, sem recortar/reamostrar: Parado4, Andar6, Ataque5, Dano1, Derrota3. LEIA-ME, imagem idle, importador e arrays reais do GuardiaoTutorial foram inspecionados. Point, sem compressão/mipmaps; células256²; PPU169,3055618constante, pivô(0,5;0,05677626), base visual em+0,05local. Transform continua escala1,5. Dados exatos em VolcanoVisual/ImportManifest.json.

JavalideBasalto.prefab é exclusivo do Vulcão e preserva Transform, Rigidbody2D, BoxCollider2D, tipo, vida e parâmetros de movimento/ataque do forte original. A cena agora referencia esse prefab. Os três arrays de comportamento continuam sendo conduzidos pelo QuestEnemy, sem Animator ou eventos novos. Quatro cenas originais e .meta preexistentes tiveram hashes conferidos durante a instalação e permaneceram iguais.

Checagem curta refeita no Unity: compilação/importação e Play passaram com saída0; marcador EQ_VOLCANO_ACTIVATION_OK confirmou cinco inimigos, três skins (incluindo Javali com4quadros idle/5ataque),20fragmentos/HUD, duas lavas marcadas congeláveis e uma raiz. Log: C:/Users/morei/Documents/Jogos Digitais/f3-l2-final-activation.log. Não representa travessia, combate completo ou revisão visual da animação; QA geral continua pendente. Salamandra permanece fora de F3-L2. Sem commit/push.
