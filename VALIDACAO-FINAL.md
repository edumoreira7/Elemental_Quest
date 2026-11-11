# Preparação do jogo para validação — 11/11/2026

O estado anterior à limpeza foi salvo no commit **e55d26b**. O menu novo e a limpeza ficam **sem commit**, para aprovação depois da jogada do usuário.

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
