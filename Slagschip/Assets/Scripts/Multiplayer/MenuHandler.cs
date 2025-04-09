using SceneManagement;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;

[RequireComponent(typeof(UIDocument))]
public class MenuHandler : MonoBehaviour
{
    private UIDocument _document;

    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private SceneAsset loadingScene;

    [SerializeField] private UnityEngine.UI.Button quickPlayButton;
    [SerializeField] private TMP_InputField playCodeInputField;
    [SerializeField] private UnityEngine.UI.Button playCodeButton;
    [SerializeField] private TMP_InputField startSessionInputField;
    [SerializeField] private UnityEngine.UI.Button startSessionButton;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        _document.rootVisualElement.Query("quick-play-button").First().RegisterCallback<ClickEvent>(OnQuickPlay);
        _document.rootVisualElement.Query("play-code-input").First().RegisterCallback<InputEvent>(OnPlayCodeInputChanged);
        _document.rootVisualElement.Query("play-code-button").First().RegisterCallback<ClickEvent>(OnPlayCodeInputSubmitted);
        _document.rootVisualElement.Query("quit-button").First().RegisterCallback<ClickEvent>(OnQuit);
        _document.rootVisualElement.Query("start-session-button").First().RegisterCallback<ClickEvent>(OnStartSession);
    }

    private void OnQuickPlay(ClickEvent _event)
    {
        sceneLoader.LoadScene(loadingScene.name, false);

        quickPlayButton.onClick.Invoke();
    }

    private void OnPlayCodeInputChanged(InputEvent _event)
    {
        playCodeInputField.text = _event.newData;

        _document.rootVisualElement.Query("play-code-button").First().SetEnabled(!_event.newData.IsNullOrEmpty());
    }

    private void OnPlayCodeInputSubmitted(ClickEvent _event)
    {
        sceneLoader.LoadScene(loadingScene.name, false);

        playCodeButton.onClick.Invoke();
    }

    private void OnQuit(ClickEvent _event)
    {
        Application.Quit();
    }

    private void OnStartSession(ClickEvent _event)
    {
        startSessionInputField.text = Guid.NewGuid().ToString();

        sceneLoader.LoadScene(loadingScene.name, false);

        startSessionButton.onClick.Invoke();
    }
}
