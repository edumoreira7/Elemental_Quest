# Tutorial curto e desafio final

Cena: `Assets/Scenes/Tutorial.unity`, no projeto `C:\Users\morei\Jogos Digitais\Elemental_Quest`.

Este mapa substitui as coordenadas dos guias anteriores. A extremidade direita passou de x344 para x232 (aproximadamente 32% menor), incluindo a nova arena. Foram retiradas as quatro repetições elementais, a combinação longa e o segundo atirador. Permanecem movimentação, seleção/mana, Terra, Gelo, salto duplo/planar, Fogo, um corpo a corpo e um atirador. A integração agora acontece no combate final, sem exigir usar todos os poderes para vencer.

| Trecho | Posição aproximada |
|---|---|
| Movimento | x0–49 |
| Introduções elementais | x49–143 |
| Corpo a corpo | x143–172, inimigo em x157 |
| Atirador e abrigo | x172–193, inimigo em x185 |
| Preparação/checkpoint | x193–202, bandeira em x196 |
| Arena do Guardião | x202–221, início do inimigo em x212 |
| Portal de conclusão | x225 |

## Combate

Um Guardião da Fase1 foi preservado em Prefab, com escala, animações, duas vidas, perseguição, ataque e dano originais. Não é um boss. O corredor é amplo; os apoios baixos em x201 e x218 permitem pular/recuar. Fogo ou Gelo podem causar dano como já acontecia; Terra e Ar continuam disponíveis como auxílio. Não foram criadas combinações obrigatórias ou novos ataques.

O inimigo só atua com o jogador na arena e enquadrado pela câmera. Ao recuar para a preparação, fica suspenso. Depois da derrota, o portal animado existente aparece. Tocá-lo mostra “Tutorial concluido!” e encerra o controle do personagem. Não declara vitória da campanha nem altera Fase1/Fase2.

## Recuperação

A bandeira usa `QuestCheckpoint` sem mudanças. Dano tira vida e faz piscar sem teleporte; queda retorna ao ponto seguro. Depois de ativar a bandeira, zerar vidas recarrega a cena com três vidas e retorna à preparação. Guardião e portal voltam ao estado inicial. Essa retomada vale na mesma sessão de Play; sair e entrar no Play começa o tutorial novamente. Não há salvamento permanente novo. Tutorial foi acrescentada à lista de cenas para permitir a recarga, preservando as entradas anteriores.

## Teste manual no Unity

1. Abra Tutorial e jogue o percurso encurtado. Confira que as seis introduções têm seus alvos/marcos e que as passagens entre os combates estão livres.
2. Atravesse a bandeira em x196: deve mudar para ativa. Observe o Guardião antes de se aproximar.
3. Teste A/D, Shift e Espaço para aproximar, recuar e saltar; use os elementos existentes. Ele deve sobreviver ao primeiro acerto e cair após dois acertos válidos.
4. Receba dano: uma vida a menos, piscar, sem teleporte. Zere as vidas deliberadamente: deve reaparecer junto à bandeira com três vidas e o inimigo restaurado.
5. Derrote o Guardião e siga à direita. O portal aparece; tocá-lo deve mostrar a conclusão e parar o controle. Não pode concluir antes da vitória.
6. Confira enquadramento, leitura dos avisos e conforto do combate com teclado. A automação posiciona o personagem entre testes, usa física/disparos reais e testa recarga; não substitui uma primeira jogada humana. Capturas da câmera não incluem OnGUI/HUD.

Os testes antigos de treino completo e segundo atirador registram o mapa anterior. Para a versão curta, executar `QuestFinalStageTest.Begin` com `-questFinalTest`, em cópia de validação.
