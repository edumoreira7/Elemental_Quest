# Elemental Quest

Jogo de plataforma 2D para Windows, em Unity 6000.5.8f1.

Abra o projeto no Unity Hub e carregue `Assets/Scenes/Inicio.unity`. Entre no Play para validar o menu.

## Conteúdo do jogo

- **Inicio:** menu com Jogar Aventura, Tutorial, Controles e Sair; navegação por mouse e pelo sistema de foco do Unity.
- **Tutorial:** aprendizagem de movimento, elementos e combate; ao terminar, pode seguir para a aventura.
- **FasePrincipal:** floresta com exploração, plataformas, desafios elementais, inimigos e checkpoints.
- **ArenaFloresta:** Guardião das Ruínas com sprites próprios, seis vidas, três padrões e absorção de Terra que recupera vida.

O botão Jogar inicia a FasePrincipal. O tutorial é opcional e tem acesso separado. A saída da fase leva à arena; vencer libera a conclusão. A campanha completa de quatro regiões não está sendo declarada concluída por esta versão.

As antigas Fase1, Fase2, SampleScene, Final e cenas de template/laboratório foram retiradas do conteúdo ativo. Ferramentas antigas e assets sem dependência nas quatro cenas foram arquivados fora do projeto, com seus arquivos `.meta`. Recursos carregados por `Resources` e configurações de renderização foram preservados.

Os testes de fluxo ficam em `Assets/Tests`, sob `UNITY_EDITOR`, e não entram no executável. O importador dos efeitos de magia continua em `Assets/Editor` para preservar leitura das texturas.

Veja [COMO_JOGAR.md](COMO_JOGAR.md) e [VALIDACAO-FINAL.md](VALIDACAO-FINAL.md). A limpeza e o menu aguardam validação do usuário antes do próximo commit.
