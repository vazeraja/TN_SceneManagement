using System;
using System.Collections;
using System.Collections.Generic;
using ThunderNut.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReferenceDemo : MonoBehaviour {
    public SceneReference scene;

    private void Awake() {
        SceneManager.LoadScene(scene.sceneIndex);
    }
}
