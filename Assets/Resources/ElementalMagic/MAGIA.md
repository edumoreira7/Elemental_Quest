# Integração dos elementos — 08/11/2026

Controles: 1 Terra, 2 Ar, 3 Fogo, 4 Gelo; J conjura. Ar: Espaço no ar faz o salto extra; segurar J durante a queda permite planar por até 1,2 s. Todos estão disponíveis desde o início; Fogo começa selecionado para preservar o ataque anterior.

Valores iniciais: mana 100, regeneração 12/s; Terra 25 e 1,5 s, Ar 10 e 0,3 s, Fogo 15 e 0,4 s, Gelo 30 e 2 s. Custos/recargas vêm da nota de mecânicas como parâmetros iniciais, ainda sem reconferência do PDF. Decisões de implementação para teste: um bloco por jogador por 8 s; planar consome 20 mana/s além da regeneração e tem limite de 1,2 s por aterrissagem; congelamento/resfriamento dura 8 s. Não atribuir esses novos valores ao professor.

Terra cria bloco à frente, rejeita posição ocupada e devolve mana nesse caso; colisão na camada 3 permite pisar e interceptar tiros existentes. Não cria outro bloco dentro do jogador.

Fogo e Gelo usam o evento existente da animação de conjuração, com reserva do elemento/custo para evitar troca no meio do lançamento e conjuração duplicada. Projéteis mantêm o componente ProjetilPoder para compatibilidade com inimigos existentes.

Gelo atinge projéteis/alvos e também procura água/lava no alcance à frente dos pés, necessário para superfície abaixo da linha horizontal do cajado. Água/lava congeladas tornam-se chão; degelo remove esse piso. Fogo desativa a colisão de raízes e reproduz destruição. Lava não foi adicionada ao Pântano: componente disponível para teste e para o Vulcão.

Assets: cópias em Assets/Resources/ElementalMagic; 32 imagens do kit e 9 cartelas novas do Pato. Originais preservados. Importador usa Point, sem compressão, sem redimensionar NPOT. Recorte por células horizontais; escala visual e colisão configuradas separadamente. Ponte com emendas e alinhamento das poses ainda precisam de inspeção visual no Unity.

HUD final reservado ao material que o usuário está gerando. QuestElementPowers expõe Atual, Mana e manaMaxima para integração. Não foi construída uma nova interface final.

Menu de preparação: Elemental Quest > Magia > Adicionar trecho de teste na Fase1. Recusa cenas não salvas, não reconstrói terreno e evita duplicação do grupo. Trecho de teste não equivale ao mapa tutorial T1 nem à conclusão de G.

Teste automatizado isolado: QuestMagicPlayTest.Begin com -questMagicTest; marcador de sucesso EQ_MAGIC_TEST_OK. Cobre mana, bloqueio de troca durante conjuração, bloco sem sobreposição, regeneração, salto extra limitado, água/lava e raízes. Validação manual de controles, planar, colisão real do tiro com bloco e percurso completo continua obrigatória.

Resultado: 14 checagens passaram antes e depois da montagem das cenas na cópia isolada. Objetos integrados ao projeto: água em x16 e raízes em x29 na Fase1; checkpoint e vida nas duas fases atuais. Objetos antigos preservados. Ainda falta QA visual e jogada completa; o HUD final será ligado ao material que o usuário está gerando.
