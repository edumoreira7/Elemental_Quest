> **Versão atual — 11/11/2026:** sprites do pacote GuardiaoRuinasVisual integrados, seis pontos de vida, ataques acelerados e absorção de Terra que cura um ponto. Guia e testes atuais em `Assets/ElementalQuest/GuardiaoRuinasIntegrado/ATUALIZACAO-BOSS.md`. O conteúdo abaixo registra o protótipo anterior de quatro vidas e não descreve o balanceamento atual.

# Guardião das Ruínas — batalha da fase principal

Projeto: `C:\Users\morei\Jogos Digitais\Elemental_Quest`. Cena: `Assets/Scenes/ArenaFloresta.unity`. Implementação em 11/11/2026, sem commit automático.

## Arena e visual

Arena fechada com piso contínuo, paredes físicas em x±8, pato à esquerda e golem à direita. Câmera fixa, sem `QuestCamera`. O enquadramento 16:9 recebe margens em janelas de outra proporção: toda a largura jogável permanece visível, sem deslocar a câmera. Fundo de floresta/ruínas, colunas de pedra com musgo e portal reutilizam os recursos existentes.

O golem é **provisório**, montado com recortes de pedras/musgo e cristal verde dos assets existentes. Não há cartelas definitivas de animação do boss. Um Animator próprio contém onze clips/estados de pose para substituir depois: Idle; preparação e execução de cada ataque; recuperação; vulnerável; recebendo dano; derrotado. A lógica comanda explicitamente o estado e sincroniza a velocidade do clip com sua duração; curvas movem as partes visuais sem disputar a posição usada pela investida.

## Ataques e aberturas

| Padrão fixo | Aviso e execução | Resposta e abertura |
|---|---|---|
| Golpe de pedra | Um braço sobe; marca laranja indica 2,8 unidades à frente; preparação de 1,2 s e impacto baixo por 0,2 s | Recuar para fora da marca ou pular. Recupera por 0,55 s e expõe núcleo por até 3,2 s |
| Onda de choque | Dois braços sobem; marca/seta indicam direção; uma onda a 3,5 unidades/s percorre até 5,5 unidades | Pular para passar por cima. Há recuperação antes do próximo padrão; não abre o núcleo nesta ação |
| Investida curta | Preparação com inclinação, corredor marcado e seta; direção travada antes de avançar; até 3,2 unidades em 0,7 s, limitada pela arena | Sair do corredor, recuar ou pular; após recuperar, núcleo fica aberto por até 3,2 s |

A sequência é golpe → onda → investida, sem ataques simultâneos ou inimigos menores. Ao chegar a dois pontos de vida, a preparação passa de 1,2 para 1 s e o intervalo Idle de 1,5 para 1,1 s. Velocidade/alcance da onda e da investida não aumentam.

## Dano e estados

O boss começa com quatro pontos de vida. Aceita um acerto por abertura do núcleo, portanto exige quatro oportunidades bem-sucedidas. Ao receber dano, entra em reação por 0,35 s e recuperação; tiros adicionais não removem mais vida nessa mesma abertura.

O receptor identifica o `ProjetilPoder` existente, como fazem os inimigos atuais. Fogo, Gelo e a rajada de Ar usam esse componente; o dano vale um ponto. Terra continua criando bloco e não recebeu ataque novo: pode servir de proteção contra a onda, que respeita sólidos da camada de chão. A área receptora inclui o corpo enquanto o núcleo está aberto para que os tiros horizontais do pato funcionem sem criar mira vertical. Fora dessa abertura, os projéteis são consumidos sem dano.

Estados lógicos: Idle → Preparação → Execução → Recuperação → Vulnerável (golpe/investida) → Recebendo dano/Recuperação, ou retorno a Idle se o jogador não acertar. Derrotado interrompe a sequência, remove ondas/áreas ofensivas e desliga o receptor de colisão.

Ataques causam dano por `QuestPlayer.Ferir`, preservando as três vidas, o piscar e a invulnerabilidade existentes. Não foi alterado o controlador do pato nem o sistema de projéteis/poderes.

## Vitória, derrota e transições

O portal de FasePrincipal em x162 continua levando a ArenaFloresta. O aviso provisório de “boss ainda não implementado” foi retirado da cena.

Ao zerar vidas no combate, `QuestSession` encaminha apenas a arena configurada para o retorno existente `QuestForestStage.VoltarAPreparacao`: FasePrincipal recarrega com três vidas junto ao checkpoint x151. Entrar novamente no portal cria outro Guardião com quatro pontos de vida. Perder uma vida sem zerar continua causando piscar, sem teleporte; não há salvamento permanente novo.

Ao derrotar o boss, a animação de colapso dura 1,2 s, uma indicação de vitória aparece e o portal direito é liberado. Entrar nele apresenta “Fase concluída” e permite voltar ao menu. A vitória não declara que a campanha inteira terminou.

## Testes executados

**44 verificacoes de combate/transicoes aprovadas**, marcador `EQ_BOSS_TEST_OK` em `boss-validated.log`. Teste visual posterior de poses e sinais: `EQ_BOSS_VISUAL_OK` em `boss-poses-final.log`. **30 verificacoes de regressao do tutorial aprovadas**, `EQ_FINAL_TEST_OK` em `boss-tutorial-regression.log`, incluindo o fluxo de morte fora da arena.

Automação executada em cópia Unity: `C:\Users\morei\Documents\Codex\eq-magic-validation-20261108`. O teste posiciona o personagem para isolar as respostas, aplica um salto físico normal, conjura Fogo com a animação/eventos existentes e percorre novamente o portal a partir do checkpoint. Uma onda e um projétil adicionais são usados **somente no teste** para verificar dano e bloqueio de acertos repetidos; não aparecem na sequência normal do jogo. Isso não equivale a uma batalha completa vencida por uma pessoa no teclado.

Capturas Camera.Render confirmam composição do cenário, poses, marca de impacto, núcleo e onda; não incluem HUD/OnGUI. A exceção de índice `UnityEditor.Search` já conhecida é filtrada, enquanto erros de jogo fazem o teste falhar.

## Roteiro manual no Editor

1. Abrir FasePrincipal, chegar ao checkpoint x151 e entrar no portal x162; ou abrir ArenaFloresta diretamente para testar os padrões. Confirmar boss à direita, barra de vida visível e cenário inteiro no enquadramento em 16:9 e 4:3.
2. Permanecer perto da região marcada no golpe. Verificar braço levantando, seta e marca laranja; escapar recuando e depois repetir escapando com Espaço. Receber um golpe deliberadamente: perder somente uma vida, piscar e continuar no local.
3. Na onda, observar os dois braços e esperar a aproximação para pular. Confirmar velocidade legível e ausência de ondas sobrepostas. Testar um bloco de Terra preparado antes do disparo para interceptá-la.
4. Na investida, observar a seta e mudar de lado após o aviso: o boss deve continuar na direção anunciada, percorrer apenas uma distância curta e parar para recuperar.
5. Atirar com 3 + J quando o núcleo estiver protegido: não perder vida. Repetir com o núcleo verde aberto: remover um ponto. Verificar também projéteis de Gelo e Ar com seus controles existentes; Terra não é ofensiva.
6. Repetir aberturas até os quatro acertos. Confirmar a leve aceleração após a metade da vida, ausência de ataque enquanto vulnerável, reação ao dano e animação de derrota.
7. Após vencer, tocar o golem e aguardar: não pode causar dano. Entrar no portal liberado, conferir “Fase concluída” e usar o botão de menu.
8. Em outra tentativa, zerar vidas: voltar à preparação da fase, com três vidas. Entrar novamente e confirmar boss restaurado. Testar pausa/continuação durante preparação, onda e vulnerabilidade.

Pendências: cartelas definitivas do golem, legibilidade do HUD em janelas pequenas, sensação dos três desvios com teclado, equilíbrio com jogadores iniciantes e teste em build independente. O protótipo funcional não substitui essas validações humanas.
