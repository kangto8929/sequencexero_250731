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
                Debug.LogWarning("nextButtonObj�� Button ������Ʈ�� �����ϴ�.");
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

        // Ÿ���� �ڷ�ƾ�� �����ϰ�, Ÿ������ ���� �� �������� ǥ������ ���θ� �����մϴ�.
        typingCoroutine = StartCoroutine(TypeText(line.dialogueText, line.showChoices));

        if (nextButtonObj != null)
            nextButtonObj.SetActive(!line.disableNextButton);

        if (dialogueArrow != null)
            dialogueArrow.SetActive(line.arrowActive);

        // ���ο� ��ȭ ������ ���۵� �� �׻� �������� ����ϴ�.
        if (choicesParent != null)
            choicesParent.SetActive(false);

        if (!string.IsNullOrEmpty(line.actionToTrigger))
            Debug.Log($"Trigger Event: {line.actionToTrigger}");
    }

    private void LoadSprite(string targetSpriteName)
    {
        const string spriteSheetName = "11�� ����";
        Sprite[] sprites = Resources.LoadAll<Sprite>("Character/" + spriteSheetName);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("��������Ʈ ��Ʈ�� �ҷ����� ���߽��ϴ�! ���: Character/" + spriteSheetName);
            return;
        }

        Sprite found = System.Array.Find(sprites, s => s.name == targetSpriteName);

        if (found != null && characterImage != null)
        {
            characterImage.sprite = found;
            Debug.Log("��������Ʈ �ҷ����� ����: " + found.name);
        }
        else
        {
            Debug.LogError(targetSpriteName + " ��������Ʈ�� ã�� ���߽��ϴ�.");
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

        // Ÿ������ �Ϸ�� ��, �������� ǥ���ؾ� �ϴ� �����̶�� �������� Ȱ��ȭ�մϴ�.
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

            // Ÿ������ �ߴܵǾ���, �� ��ȭ ������ �������� ������� �Ѵٸ� ���� �������� �����ݴϴ�.
            if (dialogueLines[currentLineIndex].showChoices)
            {
                choicesParent.SetActive(true);
                SetupChoices();
            }
        }
        else
        {
            // ���� �������� Ȱ��ȭ�Ǿ� ���� ���� ���� ���� �������� �����մϴ�.
            // �������� Ȱ��ȭ�Ǿ� �ִٸ� �÷��̾ ������ �ؾ� �մϴ�.
            if (!dialogueLines[currentLineIndex].showChoices)
            {
                GoToNextLine();
            }
            else
            {
                Debug.Log("�������� Ȱ��ȭ�Ǿ� �ֽ��ϴ�. �÷��̾�� ������ �ؾ� �մϴ�.");
            }
        }
    }

    private void GoToNextLine()
    {
        string nextID = dialogueLines[currentLineIndex].nextDialogueID;
        if (!string.IsNullOrEmpty(nextID))
            GoToID(nextID);
        else
            Debug.Log("��ȭ�� ���̰ų� ���� ID�� �����ϴ�.");
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
            Debug.LogWarning($"Dialogue ID�� ã�� �� �����ϴ�: {id}");
        }
    }

    private void SetupChoices()
    {
        HideAllChoices();

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i >= 7) break; // ������ġ: 7�������� ó��

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
                        Debug.Log("Ž��");
                    });
                    activateButton = true;
                    break;
                case 1:
                    btn.onClick.AddListener(() =>
                    {
                        Dealer.ShowLine("Choice2_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Dealer);
                        Debug.Log("����");
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
                        Debug.Log("������");
                    });
                    activateButton = true;
                    break;
                case 4:
                    btn.onClick.AddListener(() =>
                    {
                        Prophet.ShowLine("Choice5_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Prophet);
                        Debug.Log("������");
                    });
                    activateButton = true;
                    break;
                case 5:
                    btn.onClick.AddListener(() =>
                    {
                        SoundEngineer.ShowLine("Choice6_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(SoundEngineer);
                        Debug.Log("���� �����Ͼ�");
                    });
                    activateButton = true;
                    break;
                case 6:
                    btn.onClick.AddListener(() =>
                    {
                        Artist.ShowLine("Choice7_01");
                        choicesParent.SetActive(false);
                        SetupNextButton(Artist);
                        Debug.Log("ȭ��");
                    });
                    activateButton = true;
                    break;
            }

            btn.gameObject.SetActive(activateButton);
        }

        // �����: Ȱ��ȭ�� ��ư ���
        int activeCount = 0;
        for (int i = 0; i < choicesParent.transform.childCount; i++)
        {
            GameObject child = choicesParent.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                activeCount++;
                Debug.Log($"[Ȱ��ȭ] ��ư {i}: {child.name}");
            }
            else
            {
                Debug.Log($"[��Ȱ��ȭ] ��ư {i}: {child.name}");
            }
        }
        Debug.Log($"�� Ȱ��ȭ�� ��ư ��: {activeCount}");
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
                Debug.LogWarning("nextButtonObj�� Button ������Ʈ�� �����ϴ�.");
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
        const string spriteSheetName = "11�� ����";
        Sprite[] sprites = Resources.LoadAll<Sprite>("Character/" + spriteSheetName);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("��������Ʈ ��Ʈ�� �ҷ����� ���߽��ϴ�! ���: Character/" + spriteSheetName);
            return;
        }

        Sprite found = System.Array.Find(sprites, s => s.name == targetSpriteName);

        if (found != null && characterImage != null)
        {
            characterImage.sprite = found;
            Debug.Log("��������Ʈ �ҷ����� ����: " + found.name);
        }
        else
        {
            Debug.LogError(targetSpriteName + " ��������Ʈ�� ã�� ���߽��ϴ�.");
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        //  \n ���ڿ��� ���� �ٹٲ� ���ڷ� ��ȯ
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
            if (i >= 7) break; // ������ġ: 7�������� ó��

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

        // �����: Ȱ��ȭ�� ��ư ���
        int activeCount = 0;
        for (int i = 0; i < choicesParent.transform.childCount; i++)
        {
            GameObject child = choicesParent.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                activeCount++;
                Debug.Log($"[Ȱ��ȭ] ��ư {i}: {child.name}");
            }
            else
            {
                Debug.Log($"[��Ȱ��ȭ] ��ư {i}: {child.name}");
            }
        }
        Debug.Log($"�� Ȱ��ȭ�� ��ư ��: {activeCount}");
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
