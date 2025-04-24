using SceneManagement;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;

namespace UIHandlers
{
    [RequireComponent(typeof(UIDocument))]
    public class MenuHandler : MonoBehaviour
    {
        private UIDocument _document;

        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string loadingScene;

        [SerializeField] private UnityEngine.UI.Button quickPlayButton;
        [SerializeField] private TMP_InputField playCodeInputField;
        [SerializeField] private UnityEngine.UI.Button playCodeButton;
        [SerializeField] private TMP_InputField startSessionInputField;
        [SerializeField] private UnityEngine.UI.Button startSessionButton;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            _document.rootVisualElement.Query("make-session-button").First().RegisterCallback<ClickEvent>(OnMakeSession);
            _document.rootVisualElement.Query("quick-play-button").First().RegisterCallback<ClickEvent>(OnQuickPlay);
            _document.rootVisualElement.Query("play-code-input").First().RegisterCallback<InputEvent>(OnPlayCodeInputChanged);
            _document.rootVisualElement.Query("play-code-button").First().RegisterCallback<ClickEvent>(OnPlayCodeInputSubmitted);
            _document.rootVisualElement.Query("credits-button").First().RegisterCallback<ClickEvent>(OnCredits);
            _document.rootVisualElement.Query("quit-button").First().RegisterCallback<ClickEvent>(OnQuit);
            _document.rootVisualElement.Query("start-session-button").First().RegisterCallback<ClickEvent>(OnStartSession);
            _document.rootVisualElement.Query("cancel-button").First().RegisterCallback<ClickEvent>(OnCancel);
            _document.rootVisualElement.Query("back-button").First().RegisterCallback<ClickEvent>(OnBack);
            _document.rootVisualElement.Query("ok-button").First().RegisterCallback<ClickEvent>(TogglePlayCodeError);
        }

        private void OnMakeSession(ClickEvent _event)
        {
            _document.rootVisualElement.Query("start-screen").First().style.display = DisplayStyle.None;
            _document.rootVisualElement.Query("settings-screen").First().style.display = DisplayStyle.Flex;
        }

        private void OnQuickPlay(ClickEvent _event)
        {
            sceneLoader.LoadScene(loadingScene, false);

            quickPlayButton.onClick.Invoke();
        }

        private void OnPlayCodeInputChanged(InputEvent _event)
        {
            TextField _playCodeTextField = (TextField)_document.rootVisualElement.Query("play-code-input").First();

            _playCodeTextField.value = _event.newData.ToUpper();

            if (!Regex.IsMatch(_playCodeTextField.value, @"^[346-9A-HJ-NP-RTW-Y]{0,6}$"))
                _playCodeTextField.value = _event.previousData;

            playCodeInputField.text = _playCodeTextField.value;

            _document.rootVisualElement.Query("play-code-button").First().SetEnabled(!_playCodeTextField.value.IsNullOrEmpty());
        }

        private void OnPlayCodeInputSubmitted(ClickEvent _event)
        {
            playCodeButton.onClick.Invoke();
        }

        private void OnCredits(ClickEvent _event)
        {
            _document.rootVisualElement.Query("start-screen").First().style.display = DisplayStyle.None;
            _document.rootVisualElement.Query("credits-screen").First().style.display = DisplayStyle.Flex;
        }

        private void OnQuit(ClickEvent _event)
        {
            Application.Quit();
        }

        private void OnStartSession(ClickEvent _event)
        {
            startSessionInputField.text = Guid.NewGuid().ToString();

            sceneLoader.LoadScene(loadingScene, false);

            startSessionButton.onClick.Invoke();
        }

        private void OnCancel(ClickEvent _event)
        {
            _document.rootVisualElement.Query("settings-screen").First().style.display = DisplayStyle.None;
            _document.rootVisualElement.Query("start-screen").First().style.display = DisplayStyle.Flex;
        }

        private void OnBack(ClickEvent _event)
        {
            _document.rootVisualElement.Query("credits-screen").First().style.display = DisplayStyle.None;
            _document.rootVisualElement.Query("start-screen").First().style.display = DisplayStyle.Flex;
        }

        public void TogglePlayCodeError(bool _isVisible)
        {
            _document.rootVisualElement.Query("error-screen").First().style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void TogglePlayCodeError(ClickEvent _event)
        {
            _document.rootVisualElement.Query("error-screen").First().style.display = DisplayStyle.None;
        }
    }
}
