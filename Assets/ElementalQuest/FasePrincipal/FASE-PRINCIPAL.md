# Fase principal — Caminho das Ruínas

Projeto: `C:\Users\morei\Jogos Digitais\Elemental_Quest`. Cenas novas: **FasePrincipal** e **ArenaFloresta**. As cenas anteriores permanecem preservadas; a entrada pelo menu antigo não foi redirecionada. Para jogar esta etapa, abrir FasePrincipal diretamente no Editor.

## Percurso

| Região | Estrutura e ritmo |
|---|---|
| x0–24 | Clareira de orientação, cristais e primeira subida em dois degraus |
| x24–44 | Descida para vale, plataformas suspensas e rota alta de coleta; primeiro corpo a corpo |
| x44–60 | Descanso, bandeira/vida e atirador com abrigo baixo e espaço para contra-ataque |
| x60–84 | Rota alta com bloco de Terra e água congelável; piso inferior permite recuperar erro sem queda infinita |
| x84–108 | Copas para salto extra de Ar, pouso inferior, raízes para Fogo e segundo checkpoint |
| x108–148 | Subida com corpo a corpo, raízes, segunda água, abrigo/atirador e Guardião resistente, distribuídos em encontros distintos |
| x148–162 | Trecho seguro, terceiro checkpoint, vida e portal das ruínas |

São cinco inimigos no total: dois corpo a corpo, dois atiradores e um Guardião existente. Todos instanciam os Prefabs atuais; vida, animações, perseguição, projéteis e dano globais foram preservados. Um componente local limita a atuação dos inimigos ao próprio trecho e ao enquadramento, evitando disparos novos fora da câmera. Os intervalos não se sobrepõem. Projéteis já lançados continuam com seu comportamento original.

O terreno usa Tilemaps com o atlas existente e colliders sólidos na camada de chão. Plataformas, subidas e descidas variam as alturas; três camadas existentes de fundo acompanham a câmera por parallax. Cristais animados, setas, símbolos elementais, água, raízes, bandeiras e portal reutilizam a arte disponível. Não foram geradas novas imagens.

## Elementos e orientação

Controles preservados: A/D ou setas, Shift esquerdo, Espaço, 1–4 e J. Terra abre uma rota alta de coleta; Gelo cria passagem na água; Ar permite explorar plataformas altas; Fogo abre raízes. Rotas altas são opcionais: a fase incentiva escolhas sem criar uma combinação obrigatória ou poder novo. Planar permanece disponível conforme o sistema original.

Orientação direta: objetivos contextuais curtos, um trecho por vez. Orientação indireta: trilha de cristais, setas amarelas, alturas das plataformas, símbolos e contraste de objetos interativos. Sequenciamento: início calmo → exploração expandida → perigos individuais → aplicações elementais → combinação com inimigos → descanso e preparação. Não se repetem todas as instruções extensas do tutorial.

## Checkpoints e transição

Bandeiras em x46, x105 e x151 usam `QuestCheckpoint`. Vidas coletáveis ao lado dos descansos usam `QuestLifePickup`. Queda retorna ao ponto seguro; dano faz piscar sem teleporte. Zerar vidas recarrega FasePrincipal com três vidas e retoma o último checkpoint na mesma sessão de Play. Inimigos e coletáveis são restaurados pela recarga, seguindo a regra da sessão existente. Sair/entrar no Play não oferece salvamento permanente.

O portal em x162 usa a arte/animação existente, com uma transição local para **ArenaFloresta**, sem depender de coletar todos os cristais e sem declarar vitória da campanha. A cena agora contém o **Guardião das Ruínas**, com câmera fixa e combate próprio. Zerar vidas na arena retorna à preparação em x151; vencer libera o portal de conclusão da fase. Consulte `Assets/ElementalQuest/GuardiaoRuinas/BOSS-GUARDIAO.md` para padrões, vulnerabilidade e testes do boss.

## Evidência e limites

A evidência de percurso abaixo registra a etapa anterior ao boss. A validação da batalha atual, da câmera fixa e do retorno após derrota está no guia `GuardiaoRuinas/BOSS-GUARDIAO.md`.

Execucao automatica aprovada: **76 verificacoes**, marcador `EQ_FOREST_TEST_OK` em `forest-test5.log`. Inclui rota fisica, quatro elementos, cinco inimigos, checkpoints, morte com descarregamento da tentativa antiga, transicao para ArenaFloresta sem boss e retorno a preparacao. Ajuste posterior somente nas emendas visuais dos Tilemaps, inspecionado em `forest-visual-final.log` com `EQ_FOREST_VISUAL_OK`; essas capturas usam posicionamento para inspecao e nao sao outra travessia completa.

Os testes rodam numa cópia em `C:\Users\morei\Documents\Codex\eq-magic-validation-20261108`. O roteiro aciona o controlador e a física existentes, conjura magias com eventos de animação e testa contatos/recarregamento/transição. Não substitui uma jogada humana com teclado. Capturas Camera.Render não incluem o HUD/OnGUI. A exceção conhecida de `UnityEditor.Search` é filtrada; erros de jogo são tratados como falhas.

## Conferência manual no Unity

1. Abrir `Assets/Scenes/FasePrincipal.unity`, entrar no Play e percorrer desde o início. Conferir ritmo, subidas/descidas, alinhamento de piso e plataformas, câmera e emendas do parallax.
2. Seguir os cristais e também experimentar a rota alta em x26–33. Testar corrida e aterrissagem sem mover o jogador pelo Inspector.
3. Observar o corpo a corpo antes de atacar. No atirador, subir no abrigo, reconhecer intervalo/direção, descer e contra-atacar. Tentar recuar para fora de cada encontro.
4. Usar Terra junto ao símbolo em x65, Gelo na primeira água, Ar no salto extra nas copas e Fogo nas raízes. Testar mana insuficiente, uma colocação errada do bloco e degelo. Confirmar a recuperação pelo piso inferior. Testar planar segurando J durante queda, que permanece verificação de entrada real.
5. Completar o trecho combinado sem atrair excesso de ameaças. Confirmar conforto do combate contra Guardião e espaço para recuar.
6. Ativar cada bandeira. Depois da de x151, zerar vidas deliberadamente: a cena deve recarregar junto à preparação. Conferir vidas, inimigos restaurados e ponto seguro.
7. Entrar no portal x162: deve carregar ArenaFloresta com o Guardião e câmera fixa. Zerar vidas no combate deve retornar à preparação; vencer libera a conclusão. Conferir Pause/Escape e HUD em janela menor e 16:9.

Pendências: sensação de controle, clareza para um jogador novo, legibilidade final da interface, planar com teclado e build independente. Não houve commit automático.
