Líquidos animados integrados

Fonte: Documents/Jogos Digitais/Tilesets/Liquids_Animated.
80 quadros PNG importados em Assets/Resources/AnimatedLiquids. Single, 32 PPU, Point, sem compressão/mipmaps, Full Rect. As variações adicionais de topo ficam disponíveis no acervo; os tanques usam Surface, Fill e caps esquerda/direita.
QuestAnimatedLiquid usa relógio comum Time.time a 8 FPS, respeitando a pausa, sem fases aleatórias. Células quadradas ajustadas uniformemente à largura do tanque, preenchimento até abaixo da câmera e nenhum collider visual adicional. Materiais URP separados por sequência.
Aplicado somente a QuestElementTarget de água/lava em FasePrincipal e VulcaoCinzento. Regras de dano, congelamento e descongelamento mantidas; animação fica sob a ponte de gelo durante o congelamento.
Compilação passou. Conferência em Play passou nas duas superfícies de cada fase: animação avançando, materiais corretos, colisores sem alteração e descongelamento após 8 s. Capturas e relatório local em Documents/Jogos Digitais/LiquidsIntegration. Não foi simulada uma partida completa.
