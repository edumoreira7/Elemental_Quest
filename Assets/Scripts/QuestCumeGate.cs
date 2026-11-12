using UnityEngine;
public class QuestCumeGate : MonoBehaviour
{
    public QuestCumeStage stage;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<QuestPlayer>()!=null)stage.EnterArena();
    }
}
