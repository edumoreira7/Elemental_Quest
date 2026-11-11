using UnityEngine;
public class QuestBossExit : MonoBehaviour
{
    public QuestBossArena arena;
    void OnTriggerEnter2D(Collider2D outro){if(outro.GetComponent<QuestPlayer>()!=null)arena.Concluir();}
}
