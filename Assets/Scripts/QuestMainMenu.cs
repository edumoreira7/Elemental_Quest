using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class QuestMainMenu : MonoBehaviour
{
    public GameObject controles;
    public Button jogar,fecharControles;
    bool opening;
    CanvasGroup menuGroup;
    void Start(){Time.timeScale=1;controles.SetActive(false);EventSystem.current.SetSelectedGameObject(jogar.gameObject);
        foreach(var label in GetComponentsInChildren<Text>(true)){if(label.text=="Entre a floresta e as ruínas.")label.text="Quatro regioes. Uma jornada.";if(label.text=="Quatro elementos.\nUm guardião à sua espera.")label.text="Quatro elementos.\nRecupere os cristais do reino.";}
    }
    void Update(){if(controles.activeSelf&&Input.GetKeyDown(KeyCode.Escape))FecharControles();}
    public void Jogar()
    {
        if(opening)return;QuestSession.PrepararNovaPartida();opening=true;
        menuGroup=GetComponent<CanvasGroup>();if(menuGroup==null)menuGroup=gameObject.AddComponent<CanvasGroup>();menuGroup.interactable=false;menuGroup.blocksRaycasts=false;
    }
    public void ContinuarAbertura()
    {
        if(!opening)return;opening=false;menuGroup.interactable=true;menuGroup.blocksRaycasts=true;
        QuestCampaign.MarkEvent("Campaign.Opening");UnityEngine.SceneManagement.SceneManager.LoadScene("FasePrincipal");
    }
    void OnGUI()
    {
        if(!opening)return;
        var previous=GUI.matrix;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2),Quaternion.identity,Vector3.one*scale);
        GUI.Box(new Rect(170,160,940,400),"");
        var text=new GUIStyle(GUI.skin.label){fontSize=25,wordWrap=true,alignment=TextAnchor.MiddleCenter};
        GUI.Label(new Rect(215,190,850,245),"ELEMENTAL QUEST\n\nUmbrius roubou os quatro cristais e aprisionou Aurelia.\nQuackerius leva no cajado uma faisca de cada elemento.\nRecupere os cristais e traga seu irmao de volta.",text);
        if(GUI.Button(new Rect(450,465,380,55),"INICIAR JORNADA"))ContinuarAbertura();
        GUI.matrix=previous;
    }
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

