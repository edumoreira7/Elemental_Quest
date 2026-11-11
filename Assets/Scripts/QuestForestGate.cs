using UnityEngine;
public class QuestForestGate : MonoBehaviour
{
    public QuestForestStage fase;
    void OnTriggerEnter2D(Collider2D outro)
    { if(outro.GetComponent<QuestPlayer>()!=null) fase.EntrarNaArena(); }
}
