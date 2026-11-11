using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class QuestMainMenu : MonoBehaviour
{
    public GameObject controles;
    public Button jogar,fecharControles;
    void Start(){Time.timeScale=1;controles.SetActive(false);EventSystem.current.SetSelectedGameObject(jogar.gameObject);}
    void Update(){if(controles.activeSelf&&Input.GetKeyDown(KeyCode.Escape))FecharControles();}
    public void Jogar(){QuestSession.NovoJogo();}
    public void Tutorial(){QuestSession.IniciarTutorial();}
    public void AbrirControles(){controles.SetActive(true);EventSystem.current.SetSelectedGameObject(fecharControles.gameObject);}
    public void FecharControles(){controles.SetActive(false);EventSystem.current.SetSelectedGameObject(jogar.gameObject);}
    public void Sair(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
}
