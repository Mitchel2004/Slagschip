using SceneManagement;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIHandlers
{
    [RequireComponent(typeof(UIDocument))]
    public class MenuHandler : MonoBehaviour
    {
        private UIDocument _document;

        [SerializeField] private string loadingScene;

        [SerializeField] private UnityEngine.UI.Button quickPlayButton;
        [SerializeField] private TMP_InputField playCodeInputField;
        [SerializeField] private UnityEngine.UI.Button playCodeButton;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            _document.rootVisualElement.Query("quick-play-button").First().RegisterCallback<ClickEvent>(OnQuickPlay);
            _document.rootVisualElement.Query("play-code-input").First().RegisterCallback<InputEvent>(OnPlayCodeInputChanged);
            _document.rootVisualElement.Query("play-code-button").First().RegisterCallback<ClickEvent>(OnPlayCodeInputSubmitted);
            _document.rootVisualElement.Query("credits-button").First().RegisterCallback<ClickEvent>(OnCredits);
            _document.rootVisualElement.Query("quit-button").First().RegisterCallback<ClickEvent>(OnQuit);
            _document.rootVisualElement.Query("back-button").First().RegisterCallback<ClickEvent>(OnBack);
            _document.rootVisualElement.Query("ok-button").First().RegisterCallback<ClickEvent>(TogglePlayCodeError);
        }

        private void OnQuickPlay(ClickEvent _event)
        {
            SceneLoader.instance.LoadScene(loadingScene);

            quickPlayButton.onClick.Invoke();
        }

        private void OnPlayCodeInputChanged(InputEvent _event)
        {
            TextField _playCodeTextField = (TextField)_document.rootVisualElement.Query("play-code-input").First();

            _playCodeTextField.value = _event.newData.ToUpper();

            if (!Regex.IsMatch(_playCodeTextField.value, @"^[346-9A-HJ-NP-RTW-Y]{0,6}$"))
                _playCodeTextField.value = _event.previousData;

            playCodeInputField.text = _playCodeTextField.value;

            _document.rootVisualElement.Query("play-code-button").First().SetEnabled(_playCodeTextField.value.Length == 6);
        }

        private void OnPlayCodeInputSubmitted(ClickEvent _event)
        {
            playCodeButton.onClick.Invoke();
        }

        private void OnCredits(ClickEvent _event)
        {
            _document.rootVisualElement.Query("start-screen").First().style.visibility = Visibility.Hidden;
            _document.rootVisualElement.Query("credits-screen").First().style.visibility = Visibility.Visible;
            _document.rootVisualElement.Query("credits-holder").First().AddToClassList("scroll");
        }

        private void OnQuit(ClickEvent _event)
        {
            Application.Quit();
        }

        private void OnBack(ClickEvent _event)
        {
            _document.rootVisualElement.Query("credits-screen").First().style.visibility = Visibility.Hidden;
            _document.rootVisualElement.Query("start-screen").First().style.visibility = Visibility.Visible;
            _document.rootVisualElement.Query("credits-holder").First().RemoveFromClassList("scroll");
        }

        public void TogglePlayCodeError(bool _isVisible)
        {
            _document.rootVisualElement.Query("error-screen").First().style.visibility = _isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void TogglePlayCodeError(ClickEvent _event)
        {
            _document.rootVisualElement.Query("error-screen").First().style.visibility = Visibility.Hidden;
        }
    }
}
