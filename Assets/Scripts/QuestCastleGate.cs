using UnityEngine;
public class QuestCastleGate : MonoBehaviour
{
    public QuestCastleStage stage;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<QuestPlayer>()!=null)stage.EnterArena();
    }
}
