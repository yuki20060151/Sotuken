using UnityEngine;

public class TitleButtonAction : MonoBehaviour
{
    public void OnStart(string selectScene)
    {
        Loader.Instance.OnLoad(selectScene);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}