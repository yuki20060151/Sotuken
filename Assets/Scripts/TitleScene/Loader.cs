using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    // どこからでもアクセスできるシングルトンインスタンス(唯一のクラスを証明)
    public static Loader Instance { get; private set; }

    Animator anim;

    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        anim = GetComponentInChildren<Animator>();
        anim.gameObject.SetActive(false);
    }

    //様々な場所から呼び出される
    public void OnLoad(string sceneName)
    {
        //プレイヤー操作をできなくする
        anim.gameObject.SetActive(true);
        StartCoroutine(LoadCoroutine(sceneName));
    }

    IEnumerator LoadCoroutine(string scene)
    {
        yield return new WaitForSeconds(2);//フェードアウト完了まで待機
        SceneManager.LoadSceneAsync(scene);//非同期でシーンをロード
        //プログレスバーが完了するまで待機
        anim.SetTrigger("IsComplete");//Triggerを起動してフェードインする
        yield return new WaitForSeconds(2);//フェードイン完了まで待機
        //プレイヤー操作を可能にする
        anim.gameObject.SetActive(false);
    }
}