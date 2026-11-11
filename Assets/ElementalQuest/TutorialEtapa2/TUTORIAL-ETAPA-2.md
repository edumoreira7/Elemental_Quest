# Tutorial — etapa 2: elementos sem perigo

Abra `Assets/Scenes/Tutorial.unity` e entre em Play. A área nova está conectada à direita do primeiro trecho: depois das plataformas iniciais, siga pela trilha. Não há troca de cena.

## Teste manual, em ordem
1. **Troca:** pressione 1–4 e confira o elemento selecionado no HUD. Uma dica por vez; mana regenera sem coletar item. Todos os poderes continuam disponíveis desde o início.
2. **Terra (1 + J):** fique junto ao símbolo de Terra no chão, vire à direita e crie o bloco. Espere sua formação, pule primeiro e avance durante a subida para subir nele. Depois salte até o marco. Ele fica verde e mostra OK. O bloco tem a altura original; não foi alterada a força de salto. Se conjurar perto demais da plataforma, volte ao símbolo e refaça.
3. **Gelo (4 + J):** suba os degraus até a margem, congele a poça à frente e caminhe até o marco na outra margem. Errar ou deixar descongelar deve levar ao piso inferior sem perder vida. A água desta área de aprendizado não causa dano.
4. **Ar (2):** use Espaço para pular e pressione Espaço de novo no ar para alcançar o marco alto. Depois, mantendo Ar selecionado, avance pela borda e **segure J ao cair** até a plataforma de pouso. Não basta tocar J nem apenas pular até a chegada: é preciso planar por pelo menos 0,2s para o marco registrar a ação. Abaixo há piso seguro para repetir.
5. **Fogo (3 + J):** atire nas raízes e atravesse a passagem. Gelo/Ar não destroem esse alvo.
6. **Expansão:** repita Terra em um marco de outra altura, Gelo em uma poça menor, Ar em outra subida e Fogo em nova passagem. As dicas usam os mesmos controles e não introduzem função nova.
7. **Combinação final:** Terra para subir ao acesso, Gelo para criar a passagem, Ar para alcançar a plataforma mais alta e Fogo para abrir a saída. Alcance o marco final: deve mostrar OK e a dica Treino concluido.

## Reteste e falhas seguras
- Tente atingir um marco sem executar o poder correspondente: ele não deve ficar verde só pela proximidade.
- Caia de uma plataforma, atravesse a poça sem congelar e deixe o gelo desaparecer: nenhuma dessas tentativas deve tirar vida nesta área.
- Gaste mana e espere: a regeneração original permanece. A dica avisa quando falta mana para o elemento atual.
- O cenário não bloqueia os poderes nem exige voltar por um marco anterior para completar a combinação final. Cada marco registra seu próprio exercício; o final exige o uso dos quatro elementos na própria atividade.
- Continue usando os controles existentes: A/D ou setas, Shift esquerdo, Espaço, 1–4 e J.

## Orientação e limites
- Introduções isoladas, depois expansão e integração; nenhum inimigo, lava ou perigo de morte foi acrescentado.
- Orientação direta: dicas contextuais curtas e feedback de conclusão no mesmo painel. Orientação indireta: setas, alturas/posições das plataformas, símbolos dos elementos e marcos verdes com OK.
- Reutilizados efeitos, projéteis, bloco, raízes, gelo, ícones e terreno existentes. A única adaptação de segurança ignora o contato jogador/água enquanto a poça de treino está descongelada; quando congelada, a colisão é restaurada para caminhar. QuestElementTarget não foi alterado.
- QuestElementPowers recebeu apenas propriedades de leitura para acompanhar ações, custo e bloco; custos, duração, controles e execução das habilidades não mudaram.
- No Tutorial foi desativado o aviso genérico de procurar portal após coletar cristais. As outras fases mantêm o comportamento padrão.
- A cena continua independente dos Build Settings nesta etapa. Nenhuma campanha/fase anterior foi reconstruída.

## Validação
O teste isolado verifica os saltos, passagens, uso real de Terra/Gelo/Ar duplo/Fogo, marcos, água e degelo seguros, expansões e integração. Posiciona o personagem entre estações para separar os casos. **Não injeta teclado real; a telemetria de planar é simulada somente para testar a detecção de conclusão.** Conferir o gesto e a sensação de planar segurando J no Play é obrigatório.
