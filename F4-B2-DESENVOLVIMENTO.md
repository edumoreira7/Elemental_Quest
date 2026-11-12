# F4-B2 — Umbrius

Desenvolvido; compilação/importação, referências e ativação básica em Play passaram. Batalha completa e balanceamento não validados.

QuestEclipseArena monta ArenaEclipse usando PatoMago, QuestSession/QuestPlayer, QuestCampaignScene e contratos atuais de região. QuestRuinBoss, QuestBossArena, QuestBossWave e QuestBossExit inspecionados e preservados; projéteis próprios evitam acoplar Umbrius ao golem. Somente duas ações, sem espelhar todos os elementos nem adicionar batalha com o livro.

## Encontro e tempos iniciais
Piso y=-2, largura18, paredes±9,3. Câmera ortográfica fixa tamanho5,5 com viewport16:9. Jogador começa x=-5 com ponto seguro. Ao perder todas as vidas, retorno existente à preparação x98 no Castelo, sem limpar cristais. Intro técnica C05.Intro vista uma vez,0,35s nas repetições contra1,5s inicial; Novo Jogo limpa flags.

| Etapa | Tempo e geometria | Dano |
|---|---|---|
| Aviso de disparo |1s, preparação de cajado e orientação|Nenhum|
| Execução do disparo |3,4s; nascimento após0,3s, duração0,34s|Nascimento visual não causa dano|
| Orbe em voo |6u/s, y=-1,15; caixa0,5×0,5 varrida pelo passo fixo|Somente voo; termina na parede/limite/3,6s|
| Recuperação curta |0,4s após projétil terminar|Nenhum|
| Janela vulnerável |2,2s; fecha ao primeiro acerto|Recebe uma magia|
| Aviso da onda |1s, pose baixa e faixa laranja de16u no chão; texto PULE|Nenhum|
| Execução da onda |3,4s; nascimento após0,3s por0,34s|Nascimento sem dano|
| Onda rasteira |5,5u/s; basey=-2, caixa0,9×0,45 centrada0,25acima do chão, varrida|Saltável; sem rastro persistente|
| Recuperação após onda |0,4s + janela2,2s|Nenhum|
| Receber dano |0,3s|Fecha janela e segue o outro ataque|
| Derrota |2s|Projétil interrompido, collider desligado; não há ataques adicionais|

Vida6, sem aumento de vida/dano por etapa. Ordem disparo → recuperação → onda → recuperação → repetir. Uma ação termina antes do aviso seguinte. Collider de recebimento0,9×1,5, offset(0;0,75); só aceita dano em Vulnerable. QuestPlayer.Ferir mantém a invulnerabilidade atual. Terra conserva seu bloco; cura por absorção permanece exclusiva do golem.

## Corpo, efeitos e grimório
LEIA-ME B1/BFX e contratos reais de animação foram inspecionados. Células320² copiadas dos quadros individuais, sem recorte ou reamostragem; Point, sem compressão/mipmaps, PPU150constante. Corpo, onda e fumaça usam pivô(0,5;0,1), apoioy288. Orbe e grimório usam centro(0,5;0,5). Escala visual aproximada do protagonista; revisão lado a lado fica para QA.

Preparação, liberação, recuperação, dano e derrota são estados separados. Nascimento/voo/impacto do orbe e surgimento/deslocamento/dissipação da onda ficam em objetos independentes. Grimório usa um único objeto separado com BFX Flutuar/Cair/Fechado; não duplica o livro das folhas B1. Após1s da derrota (entrada aproximada do sexto quadro), descida de0,6s do livro atéy=-1,8; mantém quadro fechado. Após2s, corpo assume pose purificada, ataques desligados e espaço livre para aproximação/reconciliação. Aceitar_Asa/Andar ficam disponíveis sem criar mecânicas novas. Arrays exatos em Assets/ElementalQuest/EclipseBoss/ImportManifest.json; Recuperacao e fumaça são reutilizadas em mais de um estado.

## Vitória
Usa QuestCampaignScene.NotifyGuardianDefeated/CompletePresentation e o conjunto único de QuestCampaign: Gelo uma vez, evento C05 compartilhado. Saída física chama LeaveArena(3) e leva a Epilogo. Mantém pré-requisitos Terra/Ar/Fogo; entrada direta serve para testar ativação/luta sem fabricar cristais anteriores, com aviso se faltar progresso. C05 narrativa/C06 continuam tarefas próprias, devem reutilizar a mesma recompensa e evento, sem conceder novamente. Não foram adicionadas falas paralelas ou final extra.

## Checagens
GUIDs da cena resolvem uma vez. Unity compilou/importou e QuestEclipseSmoke.Run confirmou arena com vida6, jogador, câmera ortográfica, arrays de corpo/orbe/onda/queda do livro, grimório independente e saída fechada inicialmente. EQ_ECLIPSE_ACTIVATION_OK em C:/Users/morei/Documents/Jogos Digitais/f4-b2-activation.log. Não representa vitória, queda do livro ou recompensa executadas em teste.

QA final: controles reais, avisos, esquiva/dano do orbe e onda, janelas, derrota/livro, recompensa idempotente, retorno após morte, introdução, saída/epílogo, escala e duas resoluções. Piso, paredes, marca e portal usam formas locais; corpo/efeitos/livro usam sprites disponíveis. Seis cenas anteriores, quatro scripts do golem e .meta preexistentes tiveram hashes preservados na instalação. Sem build, commit ou push.
