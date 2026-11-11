using UnityEngine;
public class QuestBossWave : MonoBehaviour
{
    QuestRuinBoss boss;
    float direcao,origem;
    public float velocidade=4.2f;
    public float alcance=6f;
    public void Inicializar(QuestRuinBoss origemBoss,float sentido){boss=origemBoss;direcao=sentido;origem=transform.position.x;}
    void FixedUpdate()
    {
        if(boss==null || boss.Atual==QuestRuinBoss.Estado.Derrotado){Destroy(gameObject);return;}
        if(QuestSession.Atual!=null&&QuestSession.Atual.Pausado)return;
        float passo=velocidade*Time.fixedDeltaTime;
        var hit=Physics2D.BoxCast(transform.position,new Vector2(.55f,.35f),0,Vector2.right*direcao,passo,1<<3);
        if(hit.collider!=null){Destroy(gameObject);return;}
        transform.position+=Vector3.right*direcao*passo;
        foreach(var colisor in Physics2D.OverlapBoxAll(transform.position,new Vector2(.45f,.4f),0)) {
            var jogador=colisor.GetComponent<QuestPlayer>();if(jogador!=null)jogador.Ferir();
        }
        if(Mathf.Abs(transform.position.x-origem)>=alcance || Mathf.Abs(transform.position.x)>7.8f)Destroy(gameObject);
    }
}
