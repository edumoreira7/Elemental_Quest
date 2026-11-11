# Elemental Quest — relatório de revisão do tutorial

Data: 11/11/2026 (revisão iniciada em 10/11). Projeto: `C:\Users\morei\Jogos Digitais\Elemental_Quest`. Cena: `Assets/Scenes/Tutorial.unity`.

## Alterações desta revisão

- Separados cartões de controles e instruções de objetivo. Cartões contextuais curtos aparecem uma vez por assunto, por até três segundos, e suprimem a dica inferior durante sua exibição.
- Precisado o aviso do salto extra: usar novamente o salto no alto, em vez de antecipá-lo junto ao primeiro impulso.
- Corrigida a orientação após ativar a bandeira: informa checkpoint salvo, em vez de continuar pedindo ativação.
- Corrigida a lista de cenas gravada: Tutorial estava ausente, e a recarga ao zerar vidas podia falhar. Agora a cena fica incluída no arquivo persistido, preservando as entradas anteriores. O reteste exige descarregar a tentativa antiga e carregar outra, em vez de apenas conferir vidas/posição. As afirmações anteriores de respawn são substituídas por esta evidência mais rigorosa.
- Reutilizada uma vida coletável em x194, antes da bandeira, para preparar a arena sem gerar arte ou mudar o sistema de vida.
- Tela final adaptada à largura disponível e com retorno ao menu por Enter ou botão. Sair da conclusão limpa a retomada local.
- Testes de movimento e elementos atualizados para o mapa curto; teste integral percorre as áreas em sequência com a física existente. Sem recursos gráficos novos e sem mudanças na IA, nos poderes, na movimentação ou nos outros níveis.

## Evidências automáticas

| Execução em cópia Unity | Verificações aprovadas | Log |
|---|---:|---|
| Travessia continua do inicio ao menu | 114 | `tutorial-review-validated.log` |
| Movimento, corrida, obstaculo e plataformas | 27 | `tutorial-review-movement.log` |
| Elementos e reconhecimento dos seis marcos | 27 | `tutorial-review-training.log` |
| Combates, dano, queda, morte, checkpoint e portal | 30 | `tutorial-review-recovery.log` |

As execuções usam Unity Editor em modo automatizado, em `C:\Users\morei\Documents\Codex\eq-magic-validation-20261108`. Não equivalem a uma pessoa jogando com teclado.

Na travessia integral, comandos de deslocamento são aplicados ao controlador existente; saltos usam seu impulso, e magias usam a conjuração/eventos existentes. O teste verifica apoio antes de saltar. Não reposiciona o personagem entre as áreas. O planar não é forçado nem declarado aprovado: o percurso verifica o pouso seguro, e o ato de segurar J continua manual. No teste isolado dos elementos, a telemetria de planar é simulada somente para verificar o reconhecimento do marco; esse resultado não valida a entrada de teclado nem a física do planar.

Capturas da câmera foram usadas para verificar terreno, plataformas, contraste, posição dos objetos, inimigos e composição do background. A cobertura horizontal do parallax é verificada nos pontos de captura, não em todas as resoluções possíveis. Camera.Render não captura HUD/OnGUI; legibilidade e sobreposição da interface continuam no roteiro manual.

Limite do ambiente: aparece nos logs a exceção já conhecida do índice `UnityEditor.Search`, fora dos scripts de jogo. Foi explicitamente excluída do capturador de falhas, como nos testes anteriores; não houve correção desse componente do Editor. Não interpretar os resultados como um Console integralmente sem mensagens.

Os testes antigos de etapas longas e segundo atirador são históricos e não devem servir como prova do mapa atual. O teste final e os de movimento/elementos revistos correspondem à versão curta.

## Percurso e softlocks

O piso contínuo permite recuperar tentativas nas plataformas e superfícies de treino. As três áreas de combate estão conectadas e a arena só libera o portal após a derrota do Guardião. Checkpoint em x196, arena x202–221 e portal x225. A retomada após zerar vidas vale na mesma sessão de Play; sair e entrar no Play inicia novamente. Não há salvamento permanente novo.

A aprovação automática demonstra uma rota executável e os casos de falha listados nos logs; não prova ausência de todos os softlocks possíveis nem compreensão humana. Alguns marcos seguros podem ser contornados: não existe prova de domínio de todos os elementos apenas por alcançar o portal. A fase não contém boss e não conclui a campanha de quatro fases.

## Validação final manual no Unity Editor

Abra `Assets/Scenes/Tutorial.unity` e entre no Play. Faça uma primeira passagem sem usar o Inspector para mover o personagem.

- [ ] Andar com A/D ou setas; correr com Shift esquerdo; pular com Espaço. Confirmar alturas, aterrissagem, colisões e transições sem prender o personagem.
- [ ] Observar que cada cartão aparece no momento relevante, desaparece e não se repete ao voltar. Não deve dividir a tela com a dica inferior. Conferir HUD em 16:9 e em janela menor.
- [ ] Selecionar 1 Terra e J junto ao símbolo; esperar materializar, subir no bloco e alcançar o marco. Repetir uma colocação errada e corrigir sem travar.
- [ ] Selecionar 4 Gelo, congelar a água, atravessar e tentar novamente depois do degelo. Conferir segurança do piso inferior.
- [ ] Selecionar 2 Ar, usar segundo Espaço perto do alto do salto; na queda segurar J. Verificar queda mais lenta, limite de planar, consumo de mana e marco reconhecido. Esta é a principal pendência de entrada real.
- [ ] Selecionar 3 Fogo e J contra as raízes. Conferir abertura, feedback e mana/regeneração.
- [ ] Observar e vencer o corpo a corpo; depois observar o atirador, usar abrigo, saltar sobre tiro e contra-atacar. Nenhum ataque novo deve partir de inimigo fora da câmera.
- [ ] Ativar bandeira x196. Receber dano: uma vida a menos, piscar, sem teleporte. Zerar vidas depois da bandeira: retornar perto dela com três vidas e o Guardião restaurado.
- [ ] Vencer Guardião com movimento, esquiva e magia. Portal deve aparecer após sua derrota; tocá-lo mostra conclusão e encerra controle. Enter/botão retorna ao menu.
- [ ] Testar Escape/continuar, câmera/parallax, emendas visuais, leitura dos perigos e Console sem erros de jogo. Validar também em 16:9 e numa janela menor.

Para avaliar a orientação da aula, observar uma pessoa sem explicar a rota. Registrar onde hesitou, quais controles descobriu, tentativas por obstáculo e se reconheceu a conclusão. Sem jogadores observados, taxa de sucesso e intuitividade permanecem não medidas. Não marcar T3 como concluída antes desse registro.

## Pendências de entrega

Jogada humana completa, entrada real do planar, interface em diferentes tamanhos, avaliação com iniciante e execução em build independente. Nenhuma dessas verificações deve ser marcada como concluída por existir um teste automatizado ou uma captura da câmera. Outras fases, bosses e promessas da campanha ficam fora desta revisão.
