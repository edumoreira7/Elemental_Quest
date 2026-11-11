# Guardião das Ruínas — sprites integrados e dificuldade maior

Atualização de 11/11/2026. Cena: `Assets/Scenes/ArenaFloresta.unity`. Projeto: `C:\Users\morei\Jogos Digitais\Elemental_Quest`.

## Arte e animação

O modelo provisório de pedras foi substituído pelo prefab visual de `GuardiaoRuinasVisual/Generated/GuardiaoRuinas_Visual.prefab`. O pacote original foi preservado. O controlador de combate novo está em `GuardiaoRuinasIntegrado/GuardiaoCombate.controller` e usa os PNGs existentes de Idle, Attack_Slam, Attack_Shockwave, Attack_Charge, Vulnerable, Hurt e Death. Walk continua disponível no pacote, sem adicionar caminhada autônoma ao boss.

Os ataques foram divididos em preparação (primeiros quatro frames) e execução (quatro seguintes), mantendo os avisos e a lógica como responsáveis pelo dano. Não foram adicionados Animation Events de dano. O golpe atinge entre 0,08 e 0,24 s da execução, junto do frame de contato. Idle e Vulnerable mantêm o loop natural; os estados de ataque acompanham os tempos reais da lógica. A escala de 1,3 e o pivô dos pés são preservados ao virar.

Os efeitos Core_FX e Shockwave_FX estão conectados. A onda usa em loop somente os frames 1–4: o frame 5 do pacote é transparente e foi excluído durante sua fase ofensiva, evitando uma ameaça invisível. A largura da hitbox permanece menor que a largura dos quadros ativos.

## Novo balanceamento

| Parâmetro | Antes | Agora |
|---|---:|---:|
| Vida máxima | 4 | 6 |
| Introdução inicial | 2 s | 1,6 s |
| Preparação normal / avançada | 1,2 / 1 s | 0,95 / 0,8 s |
| Intervalo Idle normal / avançado | 1,5 / 1,1 s | 0,9 / 0,65 s |
| Investida normal / avançada | 0,7 s | 0,52 / 0,45 s |
| Velocidade da onda | 3,5 | 4,2 unidades/s |
| Alcance da onda | 5,5 | 6 unidades |
| Vulnerabilidade normal / avançada | 3,2 s | 2,6 / 2,2 s |

A segunda etapa começa ao sofrer dano até metade da vida e não é desfeita por cura. A sequência continua previsível: golpe → onda → investida, um ataque por vez. Um acerto por abertura permanece; sem usar Terra, seis aberturas bem aproveitadas vencem o boss. As três vidas, o controle do pato, os demais inimigos e as outras cenas não foram alterados.

## Terra fortalece o Guardião

Durante a batalha, o Guardião absorve qualquer bloco de Terra criado pelo pato na arena. A colisão é desativada e o bloco desaparece antes de virar uma plataforma/escudo. Um fragmento visual é puxado para o núcleo e, após 0,35 s, recupera **um ponto**, limitado ao máximo de seis. O núcleo pulsa e a interface avisa a cura.

Mesmo com vida cheia, o bloco é consumido; não existe cura acima do limite. A absorção não inicia outro ataque nem cancela os avisos atuais. Uma cura durante a investida não muda a duração já anunciada. Derrotado, o boss não absorve nem ressuscita.

Essa reação pertence somente a este boss: Terra continua funcionando normalmente na fase principal e no tutorial. Fogo, Gelo e rajada de Ar continuam causando um ponto ao acertar o núcleo aberto, pelo `ProjetilPoder` existente. Aumentar vida e cura não altera o dano dos poderes globalmente.

## Verificação

**67 verificacoes automaticas aprovadas**, marcador `EQ_BOSS_UPGRADE_TEST_OK` no log `boss-upgrade-validated.log` da copia Unity. Inclui sprites em combate, seis vidas, cura e limite de Terra, segunda etapa persistente, ataques reais, vitoria, morte, nova tentativa e preservacao de Terra fora da arena.

O teste usa posições controladas para isolar os padrões e aciona as conjurações reais. Verifica seis pontos de vida, dano com núcleo aberto, cura limitada, consumo de Terra, manutenção da segunda etapa após cura, escala ao virar, vitória, morte/retorno ao checkpoint e Terra funcionando fora da batalha. Não representa uma vitória humana com teclado. Capturas Camera.Render verificam o sprite na arena, mas não incluem HUD/OnGUI.

Os testes antigos de quatro vidas e corpo provisório são registros da versão anterior; para esta revisão, usar `QuestRuinBossUpgradeTest` com `-questBossUpgradeTest` na cópia de validação. A câmera fixa, o portal de conclusão e o retorno ao checkpoint x151 continuam existentes.

## Teste manual sugerido

1. Abra ArenaFloresta ou entre pelo portal da FasePrincipal. Confira animações, alinhamento dos pés e direção do golem.
2. Desvie dos três padrões e aproveite o núcleo aberto com 3 + J. Verifique seis acertos sem Terra e o ritmo mais rápido após metade da vida.
3. Depois de causar dano, use 1 + J: o bloco deve ser puxado para o boss, desaparecer e curar um ponto, com mensagem e pulso verde. Repita com a vida cheia para conferir o limite.
4. Teste Gelo e Ar como ataques e confira que os avisos de 0,8 s continuam legíveis. Veja a onda durante todo o percurso e confirme que seus frames não somem antes de cessar o dano.
5. Vença e entre no portal. Em outra tentativa, zere vidas e confirme retorno ao checkpoint x151; fora da arena, Terra deve criar um bloco normal.

Pendências: dificuldade percebida por jogadores, timing de esquiva no teclado, leitura do HUD e cura em diferentes resoluções e build independente. Nenhum commit automático.
