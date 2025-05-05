using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManagera : MonoBehaviour
{
    // Bu metot, parametre olarak gelen sahne index'ine göre sahneyi yükler.
    public void LoadLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
