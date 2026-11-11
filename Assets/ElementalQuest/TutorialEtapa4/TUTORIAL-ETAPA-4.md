# Tutorial — encontro com atirador

Projeto: `C:\Users\morei\Jogos Digitais\Elemental_Quest`. Cena: `Assets/Scenes/Tutorial.unity`.

Depois do corpo a corpo, a área começa em x270. O primeiro atirador está em x283; o abrigo baixo fica entre x277 e x279,5. Observe, pule para o abrigo, espere o disparo e desça para contra-atacar. Os tiros existentes colidem com o terreno. Terra também pode ser usada como proteção, conforme seu sistema existente.

A expansão tem um degrau em x293, abrigo em x297 e um segundo atirador em x305. Ele só é ativado depois da remoção do primeiro e quando estiver enquadrado pela câmera. Ambos reutilizam a configuração do atirador da Fase1, salva como Prefab; IA, projétil e dano globais foram preservados.

## Validar no Unity

1. Abra Tutorial fora do Play e inicie o jogo. Percorra as áreas anteriores.
2. Ao chegar perto de x275,5, observe o inimigo antes de avançar. Não deve haver disparo inesperado fora da câmera.
3. Use Espaço para subir no abrigo. Veja direção e intervalo dos tiros; a cobertura deve interceptá-los.
4. Espere um disparo, desça à direita e ataque com 3 (Fogo) e J. Também teste pular sobre um projétil no chão.
5. Receba um tiro deliberadamente: deve perder uma vida e piscar, sem retornar ao checkpoint. Recue para não perder todas as vidas.
6. Derrote o primeiro e avance. O segundo deve aparecer sozinho. Combine pulo no degrau/abrigo, desvio e contra-ataque.
7. Depois da segunda derrota, siga até x321–324. O marco fica verde e indica a entrada do desafio final. Esta etapa não constrói o desafio final.

O teste automatizado posiciona o personagem entre pontos e usa disparos reais. A sensação dos controles, o desvio com teclado e a leitura dos avisos precisam de uma jogada manual. Capturas automáticas da câmera não incluem o HUD desenhado por OnGUI.
