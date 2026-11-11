# Tutorial — etapa 3: primeiro perigo real

Abra `Assets/Scenes/Tutorial.unity`. Depois do treino elemental e da combinação final, continue à direita pela mesma cena.

## Encontro
- Aproximação segura: x223–232. Pare perto de x231 e observe o Guaxinim, identificável antes do contato e pelo aviso visual.
- Patrulha: um único Guaxinim corpo a corpo, entre x235 e x239,5. Corredor plano, sem novos saltos obrigatórios ou outros inimigos.
- Combate e recuo: use o espaço anterior e posterior à patrulha. O jogo ensina atacar de longe e sair do alcance do golpe.
- Conclusão: derrote o inimigo e avance para além de x252. O marco fica verde e aparece Encontro concluido. A continuação é somente uma saída segura; nenhuma próxima área foi implementada.

## Validação manual
1. Entre em Play e chegue à área depois do treino elemental. Para testar diretamente, durante o Play pause o jogo, selecione o Pato e coloque o Transform em X=229, Y=-2,3; retome e espere a câmera acompanhar. A edição no Play não altera o spawn salvo.
2. Fique no trecho seguro e observe pelo menos uma ida e volta: o Guaxinim deve permanecer adiante, sem causar dano à distância.
3. Aproxime-se com A/D ou setas, observe o aviso/animação do golpe e recue antes de entrar em contato. A vida deve permanecer igual se sair do alcance a tempo.
4. Em uma tentativa separada, permita um contato: o HUD deve perder uma vida, o Pato deve piscar e permanecer no local. Recuar durante a invulnerabilidade deve evitar dano repetido imediato. Não há retorno ao checkpoint por esse contato.
5. Selecione Fogo (3), fique voltado à direita e use J a uma distância segura. A magia deve acertar, a animação de morte deve ocorrer e o Guaxinim deve desaparecer, somando os pontos existentes. A vida e velocidade do inimigo foram mantidas da configuração original.
6. Opcionalmente, use Terra para separar a aproximação ou Ar para saltar e evitar contato. Os poderes permanecem com o comportamento existente; o próprio bloco também bloqueia projéteis, então não tente atirar através dele.
7. Após a derrota, siga até o marco da saída. Verifique a mensagem de conclusão e a ausência de novo inimigo ou obstáculo difícil.
8. Pare e reinicie o Play para repetir o encontro. Confira também caminhar/pular nas áreas anteriores e as passagens de Terra/Gelo/Ar/Fogo após a troca visual do terreno.

## Terreno
- Fonte preservada: `C:\Users\morei\Documents\Jogos Digitais\Terreno Continuo`.
- Atlas copiado para `Assets/ElementalQuest/TerrenoContinuo`, recortado na grade de 128px conforme JSON fornecido, com filtro Point, transparência e sem compressão.
- Chão usa conexões dos autotiles; plataformas usam peças de centro e pontas. Material próprio evita trocar a textura dos personagens. Compensada a margem visual superior do atlas sem mover as colisões.
- Dimensões das plataformas, alturas e colisões anteriores preservadas. Rampas do pacote foram importadas como peças, mas não colocadas neste primeiro combate plano. Os tiles antigos não foram sobrescritos e as outras fases não foram reestilizadas.

## Preservação e testes
- Não havia Prefab separado de Guaxinim nos assets. Foi salvo um Prefab a partir do inimigo já configurado em Fase1, mantendo scripts, quadros, vida, velocidade, ataque e colisores. Somente posição e limites de patrulha da instância do tutorial foram ajustados.
- QuestEnemy.cs e a IA global não foram alterados. Ataque, dano, piscar, invulnerabilidade, pontuação e morte são os sistemas existentes.
- QuestTutorialCombat só controla instruções e conclusão local. Há uma mensagem contextual por vez, no painel já existente.
- Teste isolado verifica um único inimigo, terreno plano, patrulha, visibilidade, golpe evitado por recuo, dano ao jogador, invulnerabilidade/piscar, magia real acertando o inimigo, derrota, pontuação e saída. O teste posiciona o personagem para isolar situações; teclado e leitura do encontro devem ser conferidos manualmente.
