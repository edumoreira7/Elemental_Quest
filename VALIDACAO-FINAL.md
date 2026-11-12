# Preparação do jogo para validação — 11/11/2026

O estado anterior à limpeza foi salvo no commit **e55d26b**. O menu novo e a limpeza foram aprovados, commitados em **729df3d** e enviados para a main. Os ajustes de combate descritos abaixo ficam sem novo commit, para validação.

## Conteúdo ativo

A lista de cenas contém somente Inicio, Tutorial, FasePrincipal e ArenaFloresta. Jogar Aventura abre a fase atual; Tutorial é separado e sua conclusão oferece continuar para a aventura. Controles usa um modal, e Sair encerra o jogo. O menu reutiliza a floresta, o pato, o golem e os ícones elementais existentes; nenhuma imagem nova foi necessária.

Fases antigas de laboratório, templates, testes históricos, ferramentas de montagem anteriores e arquivos sem dependência nas cenas finais foram retirados de Assets. Foram preservados os recursos de carregamento dinâmico em Resources, as configurações de renderização, Prefabs e animações utilizados, o importador dos efeitos e o teste de fluxo atual, que só compila no Editor.

**Arquivados:** 618 arquivos, incluindo seus `.meta`, em:
`C:\Users\morei\Documents\Codex\EQ-archive-final-20261111-e55d26b`

O arquivo `manifesto.json` dessa pasta registra caminhos originais e hashes. Esse arquivo e o commit anterior permitem recuperar o material de desenvolvimento sem manter cenas de laboratório no projeto ativo. Arquivos pessoais de `.vscode` não foram apagados; a pasta foi adicionada ao `.gitignore`.

## Testes automáticos

Passaram **18 verificações** na cópia limpa do Unity, marcador `EQ_RELEASE_FLOW_OK` em `C:\Users\morei\Documents\Codex\eq-magic-validation-20261108\release-clean-validated.log`.

Cobertura: botões com ações persistidas, foco inicial, abertura/fechamento dos controles, menu → fase principal, limpeza de pontos/vidas/checkpoint ao iniciar nova partida, acesso ao tutorial, continuação após conclusão preparada no teste, recarga real após morte, fase → arena do boss com seis vidas, morte do boss → preparação e retorno ao menu com tempo restaurado. Também foi confirmada a ausência física das cenas antigas em Assets.

O estado de conclusão do tutorial foi preparado no teste somente para exercitar o fluxo de interface; esta execução não representa outra travessia completa nem outra batalha vencida. As capturas do menu incluem o Canvas; os comandos dos botões foram acionados por seus eventos, não por uma pessoa no mouse/teclado. A exceção conhecida de `UnityEditor.Search` é filtrada e não foi corrigida nesta tarefa.

Build Windows gerado com sucesso em `C:\Users\morei\Documents\Codex\EQ-final-preview-e55d26b-20261111\ElementalQuest.exe`. Geracao validada; a jogada humana no executavel ainda nao foi realizada. Log: `release-windows-build.log`. A pasta inteira do executavel deve ser mantida junta.

## Roteiro de aprovação manual

1. Sair do Play, reabrir `Assets/Scenes/Inicio.unity` e iniciar novamente. Conferir aparência, textos, foco/realce, mouse, setas e Enter.
2. Abrir Controles; fechar por Voltar e Escape. Conferir também em 16:9 e janela menor. Testar Sair: no Editor, encerra o Play; no executável, fecha a aplicação.
3. Jogar Aventura deve começar no início da fase, com três vidas e zero pontos. Depois de ativar um checkpoint, voltar ao menu e iniciar outra partida: não pode herdar o checkpoint anterior.
4. Abrir Tutorial, concluir e usar Jogar Aventura/Enter. Conferir que inicia a fase principal, sem voltar às fases antigas.
5. Percorrer a fase, morrer após checkpoint, entrar na arena, testar o boss e sua absorção de Terra, vencer e voltar ao menu.

A jogada humana, a leitura final da interface, os controles reais e o equilíbrio do boss ainda aguardam aprovação. Só depois dessa revisão deve ser feito o próximo commit.

## Revisão dos inimigos e do Guardião — 11/11/2026

Os cinco inimigos da fase principal e os três do tutorial permanecem ativos e visíveis desde o carregamento da cena. Eles patrulham suas áreas mesmo à distância. O enquadramento e a seção do encontro controlam somente a permissão de atacar e perseguir; não escondem mais os objetos nem desativam sua animação. Disparos fora da câmera continuam bloqueados.

As oito cartelas do Guardião receberam escala de importação calibrada pela silhueta do primeiro quadro e pivôs alinhados aos pés. Uma escala constante por cartela preserva a mudança de pose durante os golpes e a queda da derrota, sem redimensionamento dinâmico do corpo a cada quadro. As imagens e os colliders originais foram preservados.

O Guardião executa a sequência onda baixa → onda alta → investida baixa → golpe no chão. Só então abre o núcleo. Na segunda metade da vida acrescenta outra onda alta e outra baixa, totalizando seis ataques antes da abertura. Não existe repouso longo entre ataques: há preparação legível e recuperação de 0,18 s, reduzida para 0,12 s na segunda metade. Avisos duram 0,75/0,60 s; ondas percorrem a arena a 6/7,2 unidades por segundo. Uma onda termina antes do ataque seguinte. A janela de vulnerabilidade dura 2,2/1,8 s e fecha ao receber um acerto, retomando a sequência. A vida continua em seis pontos; a dificuldade aumentou pelo padrão de esquiva e pelo ritmo. A absorção e cura por Terra continuam existentes.

Orientação contextual: ataque baixo pede pulo; ataque alto pede ficar no chão. O ataque alto reutiliza a animação e o efeito da onda, com altura e aviso próprios, sem novos controles.

**Executado no Unity em cópia isolada:** 18 verificações de combate/arte, incluindo inimigos ativos, patrulha à distância, bloqueio de ataques fora de câmera, escala de referência das oito animações, dano real da onda baixa, imunidade ao ataque alto no chão, pulo evitando a baixa e sendo atingido pela alta, rejeição de magia com núcleo fechado, dano com núcleo aberto, sequências de quatro e seis ataques, derrota e liberação do portal. As posições de chão/pulo e a vida para segunda etapa/vitória foram preparadas pelo teste; isso não representa uma batalha vencida manualmente. Log: `combat-revision-validated.log`, marcador `EQ_COMBAT_REVISION_OK 18`.

**Regressão executada:** outras 18 verificações de menu, acesso ao tutorial, nova partida, checkpoints, morte, transição para a arena e retorno ao menu; log `combat-revision-flow.log`, marcador `EQ_RELEASE_FLOW_OK`. Capturas reais do boss parado, atacando e vulnerável também foram conferidas. Logs e capturas estão em `C:\Users\morei\Documents\Codex\eq-magic-validation-20261108`.

**Validação manual pendente:** sair do Play, reabrir FasePrincipal e observar os inimigos já posicionados; entrar em ArenaFloresta, comparar o tamanho entre poses, alternar pulo nas ondas baixas e chão nas altas, conferir a abertura após quatro/seis ataques e testar Terra, morte e vitória com controles reais. O julgamento final de dificuldade depende dessa jogada. O executável Windows mencionado anteriormente é anterior a esta revisão; testar estes ajustes no Editor. Nenhum novo commit ou push foi feito nesta etapa.
