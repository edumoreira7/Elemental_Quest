# F2-L1 â€” Cume dos Ventos Partidos

Desenvolvido; nÃ£o validado em Play. ImplementaÃ§Ã£o restrita ao percurso. CumeDosVentos agora monta jogador, cÃ¢mera, sessÃ£o, terreno e encontros no Awake por QuestCumeStage; os parÃ¢metros e prefabs ficam serializados na cena. Abra Assets/Scenes/CumeDosVentos.unity e entre em Play. O cenÃ¡rio aparece no Play; o Editor fora do Play mostra apenas o objeto de montagem.

## Rota
- x0â€“12: entrada segura, orientaÃ§Ã£o e seleÃ§Ã£o de Ar.
- x12â€“16: desafio isolado de salto extra/planar; pouso largo visÃ­vel.
- x23â€“31: corpo a corpo; x41â€“48: atirador; x59â€“66: forte.
- x55â€“59: plataforma alta opcional, acessÃ­vel com Terra/Ar; passagem livre por baixo.
- x76â€“87: combinaÃ§Ã£o curta de corpo a corpo e atirador.
- x18,53,98: checkpoints e locais seguros para recuperar mana.
- x107: trigger fÃ­sico para ArenaCume, sem exigir todos os fragmentos.

Seis trechos principais tÃªm um collider contÃ­nuo por trecho, camada 3 e borda clara; sete plataformas ao todo contando a opcional. VÃ£os de 2â€“4 unidades, pousos principais de pelo menos 12 unidades; cÃ¢mera horizontal mantÃ©m o prÃ³ximo pouso Ã  vista em 16:9. O jogador conserva andar 4/correr 7, impulso 10 e gravidade 3 do prefab. Ar usa o salto extra e planar de 1,2s existentes; mana regenera no chÃ£o. NÃ£o foi criado vento, lava, congelamento ou outro poder.

20 fragmentos locais opcionais, placas, mensagem inicial, ruÃ­nas claras e montanhas provisÃ³rias orientam a travessia. Checkpoint do Cume Ã© separado da floresta. Novo Jogo limpa esse estado; retorno da ArenaCume usa a preparaÃ§Ã£o x98. NÃ£o concede Ar nem dispara C03 ao entrar na arena.

## Arte disponÃ­vel e placeholders
Encontradas Cabra Montesa, Coruja Arqueira e Iaque Guardiao em C:/Users/morei/Documents/Jogos Digitais/Sprites, incluindo folhas e quadros. NÃ£o foram regenerados ou alterados. A aplicaÃ§Ã£o e calibraÃ§Ã£o completa dessas skins Ã© F2-L2. Este percurso usa provisoriamente os prefabs GuaxinimCorpoACorpo, AtiradorTutorial e GuardiaoTutorial nos trÃªs arquÃ©tipos, com cinco inimigos ao todo. Terreno reaproveita Sprite_00 do terreno contÃ­nuo da fase atual; montanhas, picos, ruÃ­nas, bordas e fragmentos usam formas simples locais. ArenaCume continua a estrutura S02: boss e luta pertencem a F2-B2. NÃ£o houve busca ou geraÃ§Ã£o de novo boss nesta tarefa.

## Checagens rÃ¡pidas
CompilaÃ§Ã£o C# dos scripts de jogo com referÃªncias reais do projeto passou. ReferÃªncias GUID da cena e componentes dos quatro prefabs conferidos; geometria estÃ¡tica: maior vÃ£o 4, pousos principais >=12. Isso nÃ£o Ã© teste de colisÃ£o ou de travessia.

Unity em batch encerrou com cÃ³digo 1 antes de carregar o Editor; sem execuÃ§Ã£o em Play. Log: C:/Users/morei/Documents/Jogos Digitais/f2-l1-unity.log. Pendentes para QA: travessia real, alcance com mana, disparos, mortes/checkpoints, transiÃ§Ã£o real Ã  arena, leitura de placas, enquadramentos em duas resoluÃ§Ãµes e equilÃ­brio. Nenhum build/commit/push.

As quatro cenas originais e 359 arquivos .meta existentes tiveram hashes conferidos antes/depois da instalaÃ§Ã£o, sem alteraÃ§Ã£o por esta tarefa. AlteraÃ§Ãµes locais anteriores foram preservadas.


Atualização F2-L2: os cinco inimigos agora usam as três skins locais; fundos de céu/montanhas/ruínas disponíveis e flecha da coruja integrados. As descrições de placeholders acima registram o estado de F2-L1; ver F2-L2-DESENVOLVIMENTO.md para o estado atual.
