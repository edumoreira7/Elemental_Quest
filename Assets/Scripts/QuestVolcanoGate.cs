using UnityEngine;
public class QuestVolcanoGate : MonoBehaviour
{
    public QuestVolcanoStage stage;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<QuestPlayer>()!=null)stage.EnterArena();
    }
}
