using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public GameObject characterImageObject;
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButtonObj;
    public GameObject dialogueArrow;

    [Header("Choice UI")]
    public GameObject choicesParent;
    public List<Button> choiceButtons;

    [Header("CSV Data")]
    public TextAsset csvFile;

    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    [Header("Choice CSV Data")]
    public DetectiveDialogue Detective;
    public DealerDialogue Dealer;
    public AstronomerDialogue Astronomer;
    public MurdererDialogue Murderer;
    public ProphetDialogue Prophet;
    public SoundEngineerDialogue SoundEngineer;
    public ArtistDialogue Artist;

    [System.Serializable]
    public class DialogueLine
    {
        public string id;
        public string speakerName;
        public string dialogueText;
        public string characterImage;
        public bool showCharacterImage;
        public bool showDialogueBox;
        public bool showChoices;
        public string nextDialogueID;
        public bool waitPlayerAction;
        public string actionToTrigger;
        public bool disableNextButton;
        public string effectTrigger;
        public bool arrowActive;
        public bool isTutorialEnd;

        public DialogueLine(string[] values)
        {
            id = values[0];
            speakerName = values[1].Trim('"');
            dialogueText = values[2].Trim('"');
            characterImage = values[3];
            showCharacterImage = values[4].ToLower() == "true";
            showDialogueBox = values[5].ToLower() == "true";
            showChoices = values[6].ToLower() == "true";
            nextDialogueID = values[7];
            waitPlayerAction = values[8].ToLower() == "true";
            actionToTrigger = values[9];
            disableNextButton = values[10].ToLower() == "true";
            effectTrigger = values[11];
            arrowActive = values[12].ToLower() == "true";
            isTutorialEnd = values[13].ToLower() == "true";
        }
    }

    private void Start()
    {
        LoadCSV();

        if (nextButtonObj != null)
        {
            Button btn = nextButtonObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnNextButtonClicked);
            else
                Debug.LogWarning("nextButtonObj에 Button 컴포넌트가 없습니다.");
        }

        if (dialogueLines.Count == 0)
        {
            Debug.LogError("Dialogue lines are empty!");
            return;
        }

        currentLineIndex = 0;
        ShowDialogue(dialogueLines[currentLineIndex]);
    }

    private void LoadCSV()
    {
        dialogueLines.Clear();
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 14)
            {
                DialogueLine line = new DialogueLine(values);
                dialogueLines.Add(line);
            }
            else
            {
                Debug.LogWarning($"Line {i} has insufficient columns: {lines[i]}");
            }
        }
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current);
        return result.ToArray();
    }

    private void ShowDialogue(DialogueLine line)
    {
        currentLineIndex = dialogueLines.IndexOf(line);

        characterImageObject.SetActive(line.showCharacterImage);
        dialogueBox.SetActive(line.showDialogueBox);
        speakerNameText.text = line.speakerName;

        if (!string.IsNullOrEmpty(line.characterImage))
            LoadSprite(line.characterImage);
        else
            characterImage.sprite = null;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // 타이핑 코루틴을 시작하고, 타이핑이 끝난 후 선택지를 표시할지 여부를 전달합니다.
        typingCoroutine = StartCoroutine(TypeText(line.dialogueText, line.showChoices));

        if (nextButtonObj != null)
            nextButtonObj.SetActive(!line.disableNextButton);

        if (dialogueArrow != null)
            dialogueArrow.SetActive(line.arrowActive);

        // 새로운 대화 라인이 시작될 때 항상 선택지를 숨깁니다.
        if (choicesParent != null)
            choicesParent.SetActive(false);

        if (!string.IsNullOrEmpty(line.actionToTrigger))
            Debug.Log($"Trigger Event: {line.actionToTrigger}");
    }

    private void LoadSprite(string targetSpriteName)
    {
        const string spriteSheetName = "11인 모음";
        Sprite[] sprites = Resources.LoadAll<Sprite>("Character/" + spriteSheetName);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("스프라이트 시트를 불러오지 못했습니다! 경로: Character/" + spriteSheetName);
            return;
        }

        Sprite found = System.Array.Find(sprites, s => s.name == targetSpriteName);

        if (found != null && characterImage != null)
        {
            characterImage.sprite = found;
            Debug.Log("스프라이트 불러오기 성공: " + found.name);
        }
        else
        {
            Debug.LogError(targetSpriteName + " 스프라이트를 찾지 못했습니다.");
        }
    }

    IEnumerator TypeText(string text, bool showChoicesAfterTyping)
    {
        isTyping = true;

        string cleanedText = text.Replace("\\n", "\n").Replace(@"\n", "\n");

        dialogueText.text = "";

        int charCount = 0;
        foreach (char letter in cleanedText)
        {
            dialogueText.text += letter;
            charCount++;

            if (charCount % 2 == 0 && SFX_Manager.Instance != null && SFX_Manager.Instance.TypingSound != null)
            {
                SFX_Manager.Instance.TypingSound.PlayOneShot(SFX_Manager.Instance.TypingSound.clip, 1.0f);
            }

            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;

        // 타이핑이 완료된 후, 선택지를 표시해야 하는 라인이라면 선택지를 활성화합니다.
        if (showChoicesAfterTyping)
        {
            choicesParent.SetActive(true);
            SetupChoices();
        }
    }


    private void OnNextButtonClicked()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueLines[currentLineIndex].dialogueText.Replace("\\n", "\n").Replace(@"\n", "\n");
            isTyping = false;

            // 타이핑이 중단되었고, 이 대화 라인이 선택지를 보여줘야 한다면 지금 선택지를 보여줍니다.
            if (dialogueLines[currentLineIndex].showChoices)
            {
                choicesParent.SetActive(true);
                SetupChoices();
            }
        }
        else
        {
            // 현재 선택지가 활성화되어 있지 않을 때만 다음 라인으로 진행합니다.
            // 선택지가 활성화되어 있다면 플레이어가 선택을 해야 합니다.
            if (!dialogueLines[currentLineIndex].showChoices)
            {
                GoToNextLine();
            }
            else
            {
                Debug.Log("선택지가 활성화되어 있습니다. 플레이어는 선택을 해야 합니다.");
            }
        }
    }

    private void GoToNextLine()
    {
        string nextID = dialogueLines[currentLineIndex].nextDialogueID;
        if (!string.IsNullOrEmpty(nextID))
            GoToID(nextID);
        else
            Debug.Log("대화의 끝이거나 다음 ID가 없습니다.");
    }

    private void GoToID(string id)
    {
        int foundIndex = dialogueLines.FindIndex(x => x.id == id);
        if (foundIndex != -1)
        {
            currentLineIndex = foundIndex;
            ShowDialogue(dialogueLines[currentLineIndex]);
        }
        else
        {
            Debug.LogWarning($"Dialogue ID를 찾을 수 없습니다: {id}");
        }
    }

    private void SetupChoices()
    {
        HideAllChoices();

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i >= 7) break; // 안전장치: 7개까지만 처리

            Button btn = choiceButtons[i];
            btn.onClick.RemoveAllListeners();

            bool activateButton = false;

            switch (i)
            {
                case 0:
                    btn.onClick.AddListener(() =>
                    {
                        Detective.ShowLine("Choice1_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Detective);
                        Debug.Log("탐정");
                    });
                    activateButton = true;
                    break;
                case 1:
                    btn.onClick.AddListener(() =>
                    {
                        Dealer.ShowLine("Choice2_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Dealer);
                        Debug.Log("딜러");
                    });
                    activateButton = true;
                    break;
                case 2:
                    btn.onClick.AddListener(() =>
                    {
                        Astronomer.ShowLine("Choice3_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Astronomer);
                    });
                    activateButton = true;
                    break;
                case 3:
                    btn.onClick.AddListener(() =>
                    {
                        Murderer.ShowLine("Choice4_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Murderer);
                        Debug.Log("살인자");
                    });
                    activateButton = true;
                    break;
                case 4:
                    btn.onClick.AddListener(() =>
                    {
                        Prophet.ShowLine("Choice5_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Prophet);
                        Debug.Log("예언자");
                    });
                    activateButton = true;
                    break;
                case 5:
                    btn.onClick.AddListener(() =>
                    {
                        SoundEngineer.ShowLine("Choice6_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(SoundEngineer);
                        Debug.Log("사운드 엔지니어");
                    });
                    activateButton = true;
                    break;
                case 6:
                    btn.onClick.AddListener(() =>
                    {
                        Artist.ShowLine("Choice7_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Artist);
                        Debug.Log("화가");
                    });
                    activateButton = true;
                    break;
            }

            btn.gameObject.SetActive(activateButton);
        }

        // 디버그: 활성화된 버튼 출력
        int activeCount = 0;
        for (int i = 0; i < choicesParent.transform.childCount; i++)
        {
            GameObject child = choicesParent.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                activeCount++;
                Debug.Log($"[활성화] 버튼 {i}: {child.name}");
            }
            else
            {
                Debug.Log($"[비활성화] 버튼 {i}: {child.name}");
            }
        }
        Debug.Log($"총 활성화된 버튼 수: {activeCount}");
    }

    private void HideAllChoices()
    {
        if (choiceButtons == null) return;
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    private void SetupNextButton(MonoBehaviour characterDialogue)
    {
        if (characterDialogue == null) return;

        var nextBtnField = characterDialogue.GetType().GetField("nextButtonObj");
        var onNextMethod = characterDialogue.GetType().GetMethod("OnNextButtonClicked");

        if (nextBtnField != null && onNextMethod != null)
        {
            GameObject nextBtnObj = nextBtnField.GetValue(characterDialogue) as GameObject;
            if (nextBtnObj != null)
            {
                Button nextBtn = nextBtnObj.GetComponent<Button>();
                if (nextBtn != null)
                {
                    nextBtn.onClick.RemoveAllListeners();
                    nextBtn.onClick.AddListener(() => onNextMethod.Invoke(characterDialogue, null));
                }
            }
        }
    }
}
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public GameObject characterImageObject;
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButtonObj;
    public GameObject dialogueArrow;

    [Header("Choice UI")]
    public GameObject choicesParent;
    public List<Button> choiceButtons;

    [Header("CSV Data")]
    public TextAsset csvFile;

    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    [Header("Choice CSV Data")]
    public DetectiveDialogue Detective;
    public DealerDialogue Dealer;
    public AstronomerDialogue Astronomer;
    public MurdererDialogue Murderer;
    public ProphetDialogue Prophet;
    public SoundEngineerDialogue SoundEngineer;
    public ArtistDialogue Artist;

    [System.Serializable]
    public class DialogueLine
    {
        public string id;
        public string speakerName;
        public string dialogueText;
        public string characterImage;
        public bool showCharacterImage;
        public bool showDialogueBox;
        public bool showChoices;
        public string nextDialogueID;
        public bool waitPlayerAction;
        public string actionToTrigger;
        public bool disableNextButton;
        public string effectTrigger;
        public bool arrowActive;
        public bool isTutorialEnd;

        public DialogueLine(string[] values)
        {
            id = values[0];
            speakerName = values[1].Trim('"');
            dialogueText = values[2].Trim('"');
            characterImage = values[3];
            showCharacterImage = values[4].ToLower() == "true";
            showDialogueBox = values[5].ToLower() == "true";
            showChoices = values[6].ToLower() == "true";
            nextDialogueID = values[7];
            waitPlayerAction = values[8].ToLower() == "true";
            actionToTrigger = values[9];
            disableNextButton = values[10].ToLower() == "true";
            effectTrigger = values[11];
            arrowActive = values[12].ToLower() == "true";
            isTutorialEnd = values[13].ToLower() == "true";
        }
    }

    private void Start()
    {
        LoadCSV();

        if (nextButtonObj != null)
        {
            Button btn = nextButtonObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnNextButtonClicked);
            else
                Debug.LogWarning("nextButtonObj에 Button 컴포넌트가 없습니다.");
        }

        if (dialogueLines.Count == 0)
        {
            Debug.LogError("Dialogue lines are empty!");
            return;
        }

        currentLineIndex = 0;
        ShowDialogue(dialogueLines[currentLineIndex]);
    }

    private void LoadCSV()
    {
        dialogueLines.Clear();
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 14)
            {
                DialogueLine line = new DialogueLine(values);
                dialogueLines.Add(line);
            }
            else
            {
                Debug.LogWarning($"Line {i} has insufficient columns: {lines[i]}");
            }
        }
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current);
        return result.ToArray();
    }

    private void ShowDialogue(DialogueLine line)
    {
        currentLineIndex = dialogueLines.IndexOf(line);

        characterImageObject.SetActive(line.showCharacterImage);
        dialogueBox.SetActive(line.showDialogueBox);
        speakerNameText.text = line.speakerName;

        if (!string.IsNullOrEmpty(line.characterImage))
            LoadSprite(line.characterImage);
        else
            characterImage.sprite = null;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.dialogueText));

        if (nextButtonObj != null)
            nextButtonObj.SetActive(!line.disableNextButton);

        if (dialogueArrow != null)
            dialogueArrow.SetActive(line.arrowActive);

        if (choicesParent != null)
            choicesParent.SetActive(line.showChoices);

        if (line.showChoices)
            SetupChoices();
        else
            HideAllChoices();

        if (!string.IsNullOrEmpty(line.actionToTrigger))
            Debug.Log($"Trigger Event: {line.actionToTrigger}");
    }

    private void LoadSprite(string targetSpriteName)
    {
        const string spriteSheetName = "11인 모음";
        Sprite[] sprites = Resources.LoadAll<Sprite>("Character/" + spriteSheetName);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("스프라이트 시트를 불러오지 못했습니다! 경로: Character/" + spriteSheetName);
            return;
        }

        Sprite found = System.Array.Find(sprites, s => s.name == targetSpriteName);

        if (found != null && characterImage != null)
        {
            characterImage.sprite = found;
            Debug.Log("스프라이트 불러오기 성공: " + found.name);
        }
        else
        {
            Debug.LogError(targetSpriteName + " 스프라이트를 찾지 못했습니다.");
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        //  \n 문자열을 실제 줄바꿈 문자로 변환
        string cleanedText = text.Replace("\\n", "\n").Replace(@"\n", "\n");

        dialogueText.text = "";

        int charCount = 0;
        foreach (char letter in cleanedText)
        {
            dialogueText.text += letter;
            charCount++;

            if (charCount % 2 == 0 && SFX_Manager.Instance != null && SFX_Manager.Instance.TypingSound != null)
            {
                SFX_Manager.Instance.TypingSound.PlayOneShot(SFX_Manager.Instance.TypingSound.clip, 1.0f);
            }

            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }


    private void OnNextButtonClicked()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueLines[currentLineIndex].dialogueText;
            isTyping = false;
        }
        else
        {
            GoToNextLine();
        }
    }

    private void GoToNextLine()
    {
        string nextID = dialogueLines[currentLineIndex].nextDialogueID;
        if (!string.IsNullOrEmpty(nextID))
            GoToID(nextID);
        else
            Debug.Log("End of dialogue or no next ID.");
    }

    private void GoToID(string id)
    {
        int foundIndex = dialogueLines.FindIndex(x => x.id == id);
        if (foundIndex != -1)
        {
            currentLineIndex = foundIndex;
            ShowDialogue(dialogueLines[currentLineIndex]);
        }
        else
        {
            Debug.LogWarning($"Dialogue ID not found: {id}");
        }
    }

    private void SetupChoices()
    {
        HideAllChoices();

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i >= 7) break; // 안전장치: 7개까지만 처리

            Button btn = choiceButtons[i];
            btn.onClick.RemoveAllListeners();

            bool activateButton = false;

            switch (i)
            {
                case 0:
                    btn.onClick.AddListener(() =>
                    {
                        Detective.ShowLine("Choice1_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Detective);
                    });
                    activateButton = true;
                    break;
                case 1:
                    btn.onClick.AddListener(() =>
                    {
                        Dealer.ShowLine("Choice2_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Dealer);
                    });
                    activateButton = true;
                    break;
                case 2:
                    btn.onClick.AddListener(() =>
                    {
                        Astronomer.ShowLine("Choice3_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Astronomer);
                    });
                    activateButton = true;
                    break;
                case 3:
                    btn.onClick.AddListener(() =>
                    {
                        Murderer.ShowLine("Choice4_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Murderer);
                    });
                    activateButton = true;
                    break;
                case 4:
                    btn.onClick.AddListener(() =>
                    {
                        Prophet.ShowLine("Choice5_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Prophet);
                    });
                    activateButton = true;
                    break;
                case 5:
                    btn.onClick.AddListener(() =>
                    {
                        SoundEngineer.ShowLine("Choice6_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(SoundEngineer);
                    });
                    activateButton = true;
                    break;
                case 6:
                    btn.onClick.AddListener(() =>
                    {
                        Artist.ShowLine("Choice7_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Artist);
                    });
                    activateButton = true;
                    break;
            }

            btn.gameObject.SetActive(activateButton);
        }

        // 디버그: 활성화된 버튼 출력
        int activeCount = 0;
        for (int i = 0; i < choicesParent.transform.childCount; i++)
        {
            GameObject child = choicesParent.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                activeCount++;
                Debug.Log($"[활성화] 버튼 {i}: {child.name}");
            }
            else
            {
                Debug.Log($"[비활성화] 버튼 {i}: {child.name}");
            }
        }
        Debug.Log($"총 활성화된 버튼 수: {activeCount}");
    }

    private void HideAllChoices()
    {
        if (choiceButtons == null) return;
        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    private void SetupNextButton(MonoBehaviour characterDialogue)
    {
        if (characterDialogue == null) return;

        var nextBtnField = characterDialogue.GetType().GetField("nextButtonObj");
        var onNextMethod = characterDialogue.GetType().GetMethod("OnNextButtonClicked");

        if (nextBtnField != null && onNextMethod != null)
        {
            GameObject nextBtnObj = nextBtnField.GetValue(characterDialogue) as GameObject;
            if (nextBtnObj != null)
            {
                Button nextBtn = nextBtnObj.GetComponent<Button>();
                if (nextBtn != null)
                {
                    nextBtn.onClick.RemoveAllListeners();
                    nextBtn.onClick.AddListener(() => onNextMethod.Invoke(characterDialogue, null));
                }
            }
        }
    }
}*/
