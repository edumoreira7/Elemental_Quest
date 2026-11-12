# Cenários disponíveis — atualização atual

## Aplicado agora
- Fase2: background Cume_dos_Ventos_Background,19tiles64,24objetos128 e24props256 importados como sprites individuais. Percurso CumeDosVentos e ArenaCume agora usam fundo regional, chão/contornos/preenchimento e plataformas do pacote. Props selecionados: coluna, rocha, placa e Cristal do Ar visual ligado à saída da arena, só visível quando a saída é liberada. Outras peças ficam disponíveis no acervo; não foram inventadas rampas/ventos para usar toda imagem.
- Fase3: background Vulcao_Cinzento_Background,24tiles individuais e props disponíveis em Props/Vulcao Cinzento importados. VulcaoCinzento e ArenaVulcao usam fundo, basalto, plataformas, coluna queimada, basalto fissurado, placa e Cristal Fogo visual na saída liberada. Gêiseres/correntes/fumaça decorativos não receberam dano ou mecanismos automaticamente.

QuestRegionScenery roda depois da montagem de gameplay e substitui apenas desenho. Mantidos colliders contínuos, inimigos, poderes, posição dos pisos/checkpoints,20fragmentos em cada percurso e recompensas únicas. Props têm ordem negativa e nenhum collider. Os cristais de prop são visuais filhos da saída existente, não coletáveis duplicados. Os antigos backgrounds do Cume e geometria de fundo do Vulcão foram substituídos; pedras/colunas provisórias adicionais podem ainda pedir limpeza visual fina.

## Importação e montagem
PNG/fontes existentes preservados; somente cópias novas em Assets/ElementalQuest/RegionalScenery. Single, Point, sem mipmaps/compressão. Sem recorte por grade presumida: arquivos individuais, limites úteis medidos por alpha e gravados como Rect na cena; Sprite.Create aplica o recorte no runtime sem alterar pixels. Plataformas usam peças esquerda/centro/direita com altura visual0,25 e o collider existente. Tiles de chão usam peças de borda no extremo, centro no miolo e preenchimento abaixo. O encaixe visual do pacote do Vulcão não é garantido pelo autor e pede revisão em Play.

Backgrounds são imagens compostas: um painel acompanha a câmera cobrindo o viewport16:9, sem repetir bordas ou inventar camadas independentes de parallax. Percurso e arena recebem o mesmo recurso regional; fase1 preservada.

## Checagens
Compilação de todos os scripts atuais com referências reais passou; aviso de campo não usado preexistente em QuestEclipseArena. GUIDs das quatro cenas resolvem uma vez. Foi adicionada QuestScenerySmoke para conferir os quatro cenários e totais; a tentativa de Unity batch encerrou com código1 antes do Editor. Portanto nenhum teste em Play ou inspeção visual final desta atualização foi concluído. Log: C:/Users/morei/Documents/Jogos Digitais/regional-scenery-check.log. Não declarar cenários validados.

## Estado restante
- Fase2: recursos principais disponíveis e aplicados; acabamento de composição, cobertura das outras peças e revisão visual ainda pendentes.
- Fase3: os recursos encontrados até esta atualização foram aplicados, incluindo props que chegaram durante o trabalho. Arquivos gerados depois ainda precisam de outra conferência; não se assume pacote futuro como pronto.
- Fase4: novos tiles/background/props específicos ainda não encontrados; mantém cenário geométrico provisório e personagens já integrados.
- Compartilhados: ainda há placas/fragmentos/portais/HUDs provisórios, além de cutscenes adiadas. Não precisa gerar novamente personagens, bosses, livro ou efeitos já presentes.

Correção visual da Coruja da auditoria anterior foi preservada e compilada; confirmação visual ainda pendente. Sem build, commit ou push.
