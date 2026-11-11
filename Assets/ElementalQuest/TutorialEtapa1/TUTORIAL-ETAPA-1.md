# Tutorial — etapa 1: movimento, corrida e pulo

Abra `Assets/Scenes/Tutorial.unity` no Unity e entre em Play. A cena é independente nesta etapa: Inicio/Fase1/Fase2 e a sequência atual da campanha não foram substituídos. Ainda não foi incluída nos Build Settings.

## Percurso
1. Nascimento e chão reto: A/D ou setas, sem inimigos ou perigos.
2. Trecho aberto: Shift esquerdo + direção para correr; cristais espaçados e setas incentivam seguir à direita. Correr é apresentado sem cronômetro nem punição.
3. Obstáculo de 0,6 unidade de altura: Espaço para saltar.
4. Três plataformas de 3 unidades, com elevações progressivas de 0,6 unidade e vãos de 1,5 unidade. O chão continua sob todas elas: errar o salto não mata.
5. Trilha final à direita, reservada para conectar a próxima área. Não há troca de cena ou desafio adicional nesta etapa.

Uma mensagem contextual aparece por vez, no rodapé, apenas no trecho correspondente. A trilha usa cristais coletáveis existentes, sem criar moedas novas. A coleta não é condição para sair da área.

## Como testar manualmente
- Andar para os dois lados e soltar a direção: confirmar movimento e parada.
- Segurar Shift esquerdo enquanto anda no trecho aberto: confirmar aumento de velocidade.
- Pular o obstáculo baixo com Espaço, sem usar elementos.
- Subir as três plataformas com direção e pulo comum. Para a segunda e terceira, aproximar-se da borda antes de saltar.
- Errar um salto de propósito: cair no chão seguro, sem perder vida, e tentar de novo.
- Voltar entre trechos: conferir que só uma instrução aparece e que ela corresponde à região atual.
- Seguir após a terceira plataforma até a trilha final. Esta é a parada desta etapa; não há inimigos nem desafios elementais.

## Preservação
- PatoController, poderes, dano e regras existentes não foram alterados nesta etapa.
- Reutilizados prefab do Pato com a mesma escala da Fase1, sprites do terreno, cristais e três camadas do background existente.
- Terreno em Tilemap; colisão contínua e plataformas separadas, na camada de chão usada pelo personagem.
- Nenhuma reconstrução de Fase1/Fase2 ou do projeto inteiro.

## Validação automatizada
O teste isolado usa os valores reais do prefab e chama a rotina física existente de movimento; aplica o impulso de pulo existente para verificar a geometria do percurso. Não injeta teclas do usuário. A resposta do teclado e a leitura das mensagens ainda devem ser conferidas no Play.

Entrada técnica: `QuestTutorialBuilder.CriarEValidar` com `-questTutorialTest`, em cópia isolada. Sucesso: `EQ_TUTORIAL_STAGE1_OK`. O construtor recusa sobrescrever Tutorial.unity e recusa cenas com alterações não salvas.
