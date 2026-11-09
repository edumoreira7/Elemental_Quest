# Elemental Quest — Tutorial, recursos, interface e arena

37 PNGs separados: Checkpoint(3), Recursos(4), Interface(13), Treino(7), Arena(10).
Fundos com canal alfa. Animações em uma única linha, da esquerda para a direita. Células finais vazias em efeitos que se dissipam são intencionais.

## Integração e limites da entrega

Arte criada com a ferramenta integrada de imagens. O checkpoint e os novos cristais foram desenhados para este pacote, conforme autorização; não havia sprites de portal/cristais na pasta.
Os arquivos ainda não foram normalizados para a mesma grade, recorte, escala e pivô. Estados do mesmo objeto mantêm o desenho de referência, mas posições e limites visíveis podem variar. Conferir e alinhar ao importar.
Molduras e preenchimentos estão em arquivos separados; ajustar os retângulos de preenchimento às aberturas das molduras no Unity. As aberturas foram conferidas quanto à transparência.
O piso foi desenhado como kit modular, mas altura, recorte e bordas ainda precisam de ajuste para repetição sem emendas. Não há validação de tiling pixel a pixel ou montagem jogável.
Os loops foram inspecionados como sequências estáticas; reprodução e transições no motor ainda precisam ser conferidas.
Ícones foram gerados em resolução alta com aparência pixelada; conferir leitura após definir tamanho no jogo. Filtro Point preserva melhor os degraus visuais.

## Relação de arquivos

- Checkpoint/01-Bandeira-Inativa.png — 4 quadros solicitados
- Checkpoint/02-Ativacao.png — 6 quadros solicitados
- Checkpoint/03-Bandeira-Ativa.png — 4 quadros solicitados
- Recursos/01-Gota-Vital.png — 6 quadros solicitados
- Recursos/02-Recuperacao-Mana.png — 6 quadros solicitados
- Recursos/03-Coleta.png — 6 quadros solicitados
- Recursos/04-Reaparecimento.png — 8 quadros solicitados
- Interface/01-Icone-Terra.png — estático
- Interface/02-Icone-Ar.png — estático
- Interface/03-Icone-Fogo.png — estático
- Interface/04-Icone-Gelo.png — estático
- Interface/05-Coracao-Cheio.png — estático
- Interface/06-Coracao-Vazio.png — estático
- Interface/07-Icone-Mana.png — estático
- Interface/08-Moldura-Mana.png — estático
- Interface/09-Preenchimento-Mana.png — estático
- Interface/10-Moldura-Vida-Boss.png — estático
- Interface/11-Preenchimento-Vida-Boss.png — estático
- Interface/12-Elemento-Selecionado.png — estático
- Interface/13-Elemento-Indisponivel.png — estático
- Treino/01-Alvo-Intacto.png — estático
- Treino/02-Alvo-Atingido.png — estático
- Treino/03-Raizes-Bloqueio.png — estático
- Treino/04-Mecanismo-Vento-Parado.png — estático
- Treino/05-Mecanismo-Vento-Ativado.png — estático
- Treino/06-Plataforma-Elevada.png — estático
- Treino/07-Poca-Congelamento.png — estático
- Arena/01-Piso-Centro.png — estático
- Arena/02-Piso-Borda-Esquerda.png — estático
- Arena/03-Piso-Borda-Direita.png — estático
- Arena/04-Plataforma-Baixa.png — estático
- Arena/05-Entrada-Raizes-Fechada.png — estático
- Arena/06-Entrada-Raizes-Aberta.png — estático
- Arena/07-Marco-Saida.png — estático
- Arena/08-Decoracao-Raizes.png — estático
- Arena/09-Decoracao-Pedras.png — estático
- Arena/10-Decoracao-Vegetacao.png — estático

Prompts completos em Prompts.md; dados dos arquivos em Conferencia.csv.

