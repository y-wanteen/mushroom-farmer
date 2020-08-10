#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorSceneManagement : EditorWindow
{

    /// --------------------- MENU / LOADING ---------------------

    [MenuItem("Editor Level Loading/Title Menu", false, 0)]
    static void LoadMenu()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/TitleMenu.unity");
    }

    [MenuItem("Editor Level Loading/Loading Screen", false, 0)]
    static void LoadLoading()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/LoadingScreen.unity");
    }

    /// --------------------- LEVELS ---------------------

    [MenuItem("Editor Level Loading/Main Scene", false, 0)]
    static void LoadIntro()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
    }
}

#endif
