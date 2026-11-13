# Ajustes para validação — fases 1 a 4

## Fase 1
- Onda do Guardião com desenho 45% maior, preservando colisões e velocidade.
- Avisos de ataque apenas por direção/altura (`^`, `v`, `<`, `>`), além da faixa no chão. Frases retiradas; mantido o aviso de absorção de Terra e regeneração.

## Fase 2 — travessias do Ar
- Seis ilhas principais e uma plataforma opcional. Pisos: 0–12 (-2), 18–30 (-0,6), 40–52 (-2), 55–69 (-2), 77–89 (-0,7), 98–112 (-2). Plataforma opcional: 56–60 (+1).
- Primeiro vão apresenta salto duplo; o seguinte é uma descida longa para planar. O trecho final combina os dois sob pressão dos atiradores.
- Ar selecionado ao entrar. Shift corre; Espaço no ar executa salto extra; segurar J na descida plana. Planar deixou de iniciar a animação de ataque que interrompia o movimento.
- Atiradores em x=48 e x=87 ficam parados e disparam para a esquerda a cada 2,4 s quando o encontro está ativo e visível. Não seguem nem miram o jogador.
- Checkpoints em x=21, 56 e 100; alturas calculadas pelo piso. Câmera antecipa mais a visão para mostrar o pouso em janelas estreitas.
- Coruja: barra verde no estilo da fase 1. Rajada voltou à escala visual 1 (26% menor que 1,35); dano e tamanho da área de acerto mantidos.

## Fase 3 — gelo sobre lava e barreiras de Fogo
- Lava larga entre x=12–21 e x=70–76, congelável por 8 s. Há pisos de respiro antes e depois; o jogador pode optar por uma travessia aérea mais exigente.
- Elevação intermediária em x=38–50, rota alta opcional em x=55–59 e barreira de raízes em x=68.
- Checkpoints em x=23, 53 e 98, com altura de spawn correspondente ao piso.
- Desenho dos tiros dos inimigos atiradores 30% menor; movimento e colisões preservados.
- Usado o prop de raízes carbonizadas já disponível, ligado à barreira existente. Fogo remove barreira e arte juntos.

## Fase 4 — galerias e combate
- Pisos alternados: 0–14 (-2), 18–34 (-1), 38–50 (-2), 52–70 (-2), 73–89 (-0,6), 93–112 (-2). Galeria opcional: 55–59 (+1).
- Ritmo de subida, corredor de combate e novo salto; barreira de Fogo no acesso à galeria final. Checkpoints em x=20, 53 e 98.
- Pato em escala 0,72 no percurso e arena, seguindo a referência das fases anteriores; inimigos mantêm os tamanhos dos prefabs.
- Desenho dos tiros dos atiradores 40% maior. Mecânica de Umbrius preservada.

## Props e saídas
- Cópias de sete recursos existentes de `Documents/Jogos Digitais/Props` em `Assets/Resources/LevelPolish`: obelisco, bandeira, pilar, tronco, raízes, estátua e tocha.
- Transparência original dos PNGs preservada; limites úteis medidos para definir escala e apoio no chão, sem cortar galhos ou brilhos. Props decorativos sem colisão e atrás dos atores.
- Entradas de arena e saídas dos bosses 2, 3 e 4 reutilizam a animação do portal da fase 1. O visual não contém um segundo gatilho; continuam valendo os controles de transição e liberação existentes.
- Cada percurso mantém 20 fragmentos; os pontos foram redistribuídos pelas novas rotas.

## Líquidos animados integrados
- Água nos dois trechos da fase 1 e lava nos dois cruzamentos da fase 3, com oito quadros a 8 FPS.
- Superfícies, extremidades e preenchimento sincronizados. A ponte de gelo aparece por cima; congelamento e descongelamento de 8 s conferidos em Play com os colisores originais.

## Recursos aguardados
- Novos sprites dos bosses derrotados das fases 2 e 3.
- Não foram geradas novas imagens nesta tarefa. As raízes aplicadas são do acervo existente.

## Validação
- Compilação com as referências reais do Unity.
- Sondas de física sobre os cinco vãos de cada percurso, usando as dimensões do pato e os parâmetros reais de corrida, gravidade, salto extra e planar.
- No Cume, a sonda confirma vãos que não passam com salto simples e trechos que precisam de planar além do salto duplo.
- Conferência de 20 fragmentos, proporção 0,72, atiradores fixos, congelamento da lava, props e portais.
- Capturas em Play dos três percursos e quatro arenas. Isso não substitui a validação do jogador para ritmo, dificuldade e sensação dos controles.

Os scripts de conferência ficam no menu **Tools > Elemental Quest > Conferir ajustes de gameplay**. Resultados e capturas locais: `Documents/Jogos Digitais/GameplayPolish`.


