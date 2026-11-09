# Personagens atualizados e arte do tutorial

Originais preservados em `C:\Users\morei\Documents\Jogos Digitais\Sprites\Sprites Personagens atualizados`.
Esta pasta contém cópias organizadas por personagem: Cartelas para referência e Quadros para importação. Não substitui ainda sprites usados por cenas, Prefabs ou animações.

## Estado da importação
- Pato Mago: cartela e quadros extraídos do ZIP.
- Atirador: cartela e quadros extraídos do ZIP.
- Guaxinim: cartela e quadros individuais fornecidos.
- Sapo: cartela e quadros extraídos do ZIP.
- Não deduzir ações apenas do número do quadro. Conferir visualmente ordem, transparência, alinhamento dos pés, escala e pivô antes de substituir as referências.
- A cartela do Pato inclui um projétil separado na sequência de conjuração; não usar esse quadro como corpo do personagem.
- A cartela do Guaxinim mostra um efeito de golpe cortado entre células; revisar os quadros individuais antes de montar a animação.
- Configuração proposta no Unity: Sprite (2D and UI), Single para cada quadro, Filter Mode Point, Compression None; pixels por unidade consistente com o tamanho atual em cena. Validar o resultado antes de fixar esse valor.

## Produzir agora para o tutorial
1. Terra: bloco sólido com topo legível e efeito separado de surgimento. A superfície precisa servir de plataforma e bloquear tiros.
2. Gelo: efeito de congelamento e superfície de água congelada com centro repetível e extremidades. Água normal deve ser distinguível do piso seguro.
3. Ar: efeito separado de impulso de segundo salto e vento de planar. Não desenhar outro Pato dentro do efeito.
4. Fogo: projétil e impacto separados; barreira orgânica inteira e sua destruição.
5. Interface: ícones Terra, Ar, Fogo e Gelo, na mesma dimensão e estilo.

Gelo sobre lava também será necessário no Vulcão; produzir após o conjunto do Pântano. Vida coletável e checkpoint podem usar visuais provisórios enquanto as mecânicas são testadas.

## Padrão para as novas imagens
- PNG com transparência real; sem fundo quadriculado desenhado, legendas ou cenário.
- Vista lateral, estilo e contorno coerentes com o Pato; projéteis apontando à direita.
- Quadros de uma mesma ação com dimensões iguais, pivô estável e margem para o efeito inteiro, sem cortes.
- Separar por elemento e função: Terra/Bloco, Terra/Surgimento, Gelo/Superficie, Gelo/Congelamento, Ar/Impulso, Ar/Planar, Fogo/Projetil, Fogo/Impacto, Fogo/Barreira.
- Superfícies precisam encaixar horizontalmente sem frestas. Quantidade de quadros é escolha de produção, não requisito do professor.

Validação em Play e substituição dos personagens nas animações ainda pendentes.
