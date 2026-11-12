# Arte atual disponível e necessidades reais

Não gerar novamente pacotes que já existem. Esta revisão separa arquivos encontrados de montagem final aprovada.

| Pacote | Cume | Vulcão | Castelo |
|---|---:|---:|---:|
| Background | 1 | 1 | 1 |
| Tilesets | 19 | 24 | 24 |
| Props | 24 | 24 | 24 |

O Cume também tem24objetos128 do tileset. Todos os inimigos regulares e bosses das três regiões, suas animações e efeitos principais já estão disponíveis/importados. Gelo, mana, seletor, jogador e checkpoint usam o acervo existente.

## Já temos: falta aplicar/revisar corretamente
- Cume/Vulcão: arte regional já ligada por QuestRegionScenery ao percurso e arena, mas montagem visual ainda não aprovada. Rever encaixes, base dos props, contraste e limites de chão. Não considerar a compilação como aprovação estética.
- Castelo: novo background,24tiles e24props encontrados agora em Documents/Jogos Digitais. Ainda não importados/aplicados nesta revisão. Props incluem portal aberto, janela gótica, placa, cristais, altares, bandeiras e tochas. Não pedir que sejam gerados novamente.
- Background é imagem de fundo composta, nunca terreno ou plataforma. Os tiles individuais têm margens; o próprio LEIA-ME do Vulcão/Castelo não garante encaixe pixel a pixel. É necessário montar amostra visual e ajustar limites/conexões antes de chamar o chão de final. Fundos atuais não são camadas de parallax separadas.
- Props são estáticos; tochas, bandeiras e portais não devem ser declarados animados. Não precisam de mecânica de dano/vento só por aparecerem na arte.

## O que ainda precisamos além de montagem
1. Confirmar cobertura das quatro arenas/telas de percurso com o acervo novo. Não é obrigatório gerar background exclusivo de arena se o existente enquadrar bem; primeiro compor com ele.
2. Fragmento coletável próprio se o losango atual não servir. Os cristais de Ar/Fogo e cristais/props do Castelo já existem; reutilizar em vez de solicitar todos novamente. Identificação dos quatro no HUD já usa texto/ícones existentes.
3. Aplicar placas, portais, raízes/alvos e molduras de vida do boss disponíveis; substituir formas provisórias. Só solicitar peça inédita após demonstrar ausência no pacote.
4. Futuros alvos congeláveis/raízes precisam de leitura visual de estados, não apenas decoração de lava. Reusar efeitos de gelo/fogo existentes onde funcionarem.
5. Para cutscenes adiadas: Aurélia e torre/restauração ainda não encontradas nesses pacotes. Irmãos, livro e poses/efeitos já existem. Créditos finais/licenças e telas de pausa/derrota/conclusão precisam acabamento, não necessariamente novos sprites.

## Correções de proporção desta revisão
Fase3 e ArenaVulcao usavam PatoMago em escala1,0; agora pai com escala0,72 antes de Awake, igual à fase1 e à correção do Cume. Não alterado prefab original ou tamanhos dos inimigos.

BrasaVulcao conserva Transform0,6 e CircleCollider2D original. QuestAnimation ganhou escala visual padrão1, ativada em2 somente nesse prefab; SpriteRenderer usa desenho Sliced/FullRect maior, sem alterar collider, velocidade5, duração3s ou dano. Os outros projéteis/animações mantêm padrão1. Conferir visualmente o tamanho contra o Lagarto e o contraste no cenário; dissipação permanece no tamanho anterior.

Checagens: referências e compilação serão registradas após execução. Nenhuma conferência visual completa ou travessia nesta revisão. Nenhum PNG/.meta alterado. Sem commit/push; notas apenas no projeto, sem Obsidian.

Resultado: compilação dos scripts atuais passou, apenas aviso preexistente de campo não usado. Tentativa de ativação curta encerrou com código 1 antes de carregar o Editor; nenhuma validação visual ou em Play desta correção. Log: C:/Users/morei/Documents/Jogos Digitais/f3-scale-review.log. Background do Castelo e nomes dos 24 props foram inspecionados nesta revisão.
