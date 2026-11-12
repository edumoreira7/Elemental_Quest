# F4-L1 — Castelo do Eclipse

Desenvolvido com placeholders locais; compilação/importação e ativação curta em Play passaram. Travessia integral e QA pendentes.

QuestCastleStage monta CasteloDoEclipse no Play, reutilizando o padrão atual de QuestCumeStage/QuestForestStage: sessão, PatoMago, QuestPlayer, QuestCamera, terreno com um collider contínuo por trecho e encontros delimitados por seção/câmera. FasePrincipal e terreno contínuo reais foram inspecionados e preservados. Nenhuma cena anterior foi reconstruída. Em edição a cena exibe o objeto de montagem; entre em Play para ver o percurso.

## Rota
- x0–12: entrada segura; placa do castelo e contraste piso/decoração.
- x12–16: desafio isolado de Ar, salto extra/planar, pouso largo visível em x16.
- x23–31: corpo a corpo; x41–48: atirador; x59–66: forte.
- x53: checkpoint/respiro, espaço para recuperar mana.
- x55–59,y1: galeria opcional, apoio com Terra ou Ar; passagem inferior livre.
- x70–73: fenda curta com pouso visível; raízes marcadas x74 para Fogo.
- x76–87: combinação curta de corpo a corpo e atirador.
- x98: checkpoint seguro de preparação; x107: trigger físico para ArenaEclipse.

Seis pisos principais:0–12,16–33,36–50,52–70,73–89,92–112, topoy=-2; plataforma opcional totaliza sete. Maior vão4unidades; pousos principais>=12. Câmera horizontal tamanho5,5 com alvo+2 mantém o próximo pouso visível no enquadramento16:9 adotado. Checkpoints x18,53,98, estado próprio do Castelo. Derrota na futura arena retorna à preparação x98; Novo Jogo limpa também esse estado. Entrada na arena não exige100% de fragmentos nem concede Gelo ou C05.

Usa os poderes reais: Ar salto extra e planar1,2s, Terra bloco8s e Fogo em QuestElementTarget.Raizes. Nenhum vento novo, gelo sobre decoração, lava ou armadilha violeta foi criado. Raízes têm collider0,6×2,4; vitrais, colunas e sombras não têm collider. Placas e mensagem inicial distinguem piso claro sólido de decoração violeta, e fragmentos guiam a rota. Vinte posições únicas de fragmentos opcionais; total do HUD obtido da criação real. Cinco inimigos reutilizam QuestEnemy sem alterar comportamento, vida ou física dos prefabs.

## Placeholders explícitos
Sprites novos indisponíveis conforme informado pelo usuário. Corpo a corpo usa GuaxinimCorpoACorpo; atirador usa AtiradorTutorial; forte usa GuardiaoTutorial. Não foram chamados de skins finais de gato/corvo/urso. Piso usa pedra fria geométrica com juntas e borda clara; paredes violetas, molduras, travessas de vitrais apagados e colunas ficam atrás, com ordem de desenho negativa e sem colisão. Placas, fragmentos, raízes e entrada da arena usam formas locais. Pato, magia, checkpoint e arquétipos são recursos existentes. ArenaEclipse continua a estrutura S02; luta de Umbrius e arte final são tarefas próprias. Sem geração ou edição de sprites nesta tarefa.

## Checagens realizadas
GUIDs da cena resolvem uma vez. Unity compilou/importou e QuestCastleSmoke.Run ativou o percurso em Play por janela curta: jogador, cinco inimigos dos três arquétipos,20fragmentos/HUD, sete plataformas, uma raiz e ausência de lava/alvos extras. Marcador EQ_CASTLE_ACTIVATION_OK, log C:/Users/morei/Documents/Jogos Digitais/f4-l1-activation.log. Não foi simulada travessia, luta completa ou recompensa.

QA final: alcance dos saltos/mana, visibilidade de pousos e placas, escala e contraste em duas resoluções, raízes com input real, coleta, mortes/checkpoints, combinação de inimigos e transição real até arena. Hashes das seis cenas anteriores e todos os .meta preexistentes foram conferidos antes/depois da instalação, iguais; alterações locais anteriores preservadas. Sem build, commit ou push.
