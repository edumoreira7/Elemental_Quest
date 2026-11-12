# Auditoria de sprites — fases2/3/4 e Grã-Coruja

## Inventário confirmado
Comparados hashes PNG entre pacotes locais em Documents/Jogos Digitais/Sprites e Assets do projeto ativo, sem usar outra cópia de código. Todos os quadros dos16pacotes de personagens/efeitos pesquisados já têm cópia idêntica importada. Importado não significa que toda pose é executada: arrays e chamadas de criação foram inspecionados separadamente.

- Fase2: Cabra18, Coruja Arqueira30, Iaque19, Grã-Coruja46 e efeitos22. Skins ligadas aos prefabs da cena; projétil/efeito da arqueira e ações principais da boss ligados.
- Fase3: Raposa18, Lagarto30, Javali19, Salamandra56 e efeitos24. Skins, brasa/dissipação e duas ações do boss ligados. Andar da Salamandra é recurso opcional, não cria ação extra.
- Fase4: Gato18, Corvo30, Urso19, Umbrius66 e efeitos43. Skins e projétil ligados; orbe/onda/livro ligados. Andar e Aceitar_Asa de Umbrius ainda não são disparados pelo combate; Aceitar_Asa serve para a futura reconciliação. Não precisa gerar outro livro: BFX já fornece flutuação/queda/fechado.

## Já disponível, mas ainda não aproveitado em tudo
Pacote compartilhado Tutorial Recursos Interface e Arena, já em Resources: Marco-Saida, Raizes-Bloqueio, Plataforma-Elevada, decoração de pedras/raízes e peças de UI da vida do boss/mana. Nas fases novas, portais, raízes, plataformas e alguns HUDs ainda usam formas simples em vez de todas essas imagens. A bandeira animada de checkpoint, jogador, magias e seletor já são reaproveitados. Moldura/preenchimento da vida do boss podem substituir a caixa/texto provisórios sem gerar nova arte;13-Elemento-Indisponivel é desnecessário com quatro poderes disponíveis.

Os backgrounds atuais do Cume são reaproveitados do acervo local e já estão aplicados; cenário específico ainda pode substituí-los. ArenaCume/ArenaVulcao/ArenaEclipse continuam com chão/parede/portal geométricos, separados dos percursos: os tiles/backgrounds novos também precisam cobrir as arenas. Raízes/pedras naturais do tutorial não devem ser tratadas como decoração final do Castelo sem adaptação visual.

## O que falta além de tiles/backgrounds
1. Aplicar o acervo compartilhado às formas provisórias onde combinar: portais/saídas, raízes marcadas, placas/sinalização e acabamento do HUD de boss. É trabalho de integração, não exige novos personagens.
2. Arte de fragmento/coleta local e representação dos quatro cristais inteiros, caso se deseje substituir os losangos e marcas textuais atuais. Ícones Terra/Ar/Fogo/Gelo já existem e podem servir ao HUD; não é preciso gerar seletores novamente.
3. Props temáticos se não vierem nos tiles: plataformas suspensas/ruínas do Cume; bordas de lava congelável claramente marcadas e raízes carbonizadas no Vulcão; vitrais apagados/colunas/portais do Castelo. Manter distinção de piso/decoração e alvos reais.
4. Para as cutscenes futuras: Aurélia livre/aprisionada e composição da torre/restauração ainda não foram encontradas nesses pacotes. Irmãos, poses de Umbrius, livro e brilho/fumaça já existem. Não bloquear gameplay por isso; cutscenes foram adiadas pelo usuário.

Nenhum inimigo regular ou boss das fases2/3/4 precisa ser gerado do zero. Assets opcionais sem mecânica não justificam inventar ataques novos.

## Revisão da Grã-Coruja
Encontrados defeitos de integração no código: sprite de mergulho com pivô central reutilizava o transform físico dos pés, trocando âncora abruptamente; subida para preparação teleportava; direção do mergulho podia herdar a rajada anterior. Corrigido somente QuestTempestArena: visual em filho separado, física sempre ancorada nos pés, compensação constante0,70714286u para mergulho baseada no limite alpha inferior do último quadro; subida interpolada0,45s; direção recalculada pelo alvo travado. Dano do mergulho continua centrado0,9u acima dos pés com mesma caixa1,5×1,2. Efeitos passam a espelhar conforme direção. Sprites, PPU, .meta e fase1 não alterados.

Essa correção trata causas identificáveis, não certifica todos os problemas visuais relatados. A compilação de todos os scripts atuais com referências locais passou (apenas aviso de campo não usado preexistente em Umbrius); tentativa de Unity batch encerrou com código1 antes de carregar o Editor, portanto não houve teste em Play/inspeção visual final nesta revisão. Se persistir outra distorção, conferir as nove poses no Unity e comparar escala/âncora antes de regenerar imagens.

Arquivo técnico de contagem/hash: C:/Users/morei/Documents/Jogos Digitais/sprite-audit.json. Sem commit/push. QA integral continua pendente.


Atualização: tiles, backgrounds e props disponíveis do Cume e do Vulcão aplicados aos percursos e arenas. Consultar CENARIOS-ATUALIZACAO.md para o estado atual e limites de validação.
