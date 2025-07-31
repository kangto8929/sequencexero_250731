using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AstronomerDialogue : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public GameObject characterImageObject;
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButtonObj;
    public GameObject dialogueArrow;

    [Header("CSV Data")]
    public TextAsset csvFile;

    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    [System.Serializable]
    public class DialogueLine
    {
        public string id;
        public string speakerName;
        public string dialogueText;
        public string characterImage;
        public bool showCharacterImage;
        public bool showDialogueBox;
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
            nextDialogueID = values[6];
            waitPlayerAction = values[7].ToLower() == "true";
            actionToTrigger = values[8];
            disableNextButton = values[9].ToLower() == "true";
            effectTrigger = values[10];
            arrowActive = values[11].ToLower() == "true";
            isTutorialEnd = values[12].ToLower() == "true";
        }
    }


    private void Start()
    {
        LoadCSV();
    }

    private void LoadSprite(string targetSpriteName)
    {
        // Resources/Character ���� �� ��������Ʈ ��Ʈ �̸�(������ �Ǵ� ��Ʈ��)�� ������ �����ϼ���.
        string spriteSheetName = "11�� ����";

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

    private void LoadCSV()
    {
        dialogueLines.Clear();
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 13)
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

    public void StartFromBeginning()
    {
        currentLineIndex = 0;
        ShowDialogue(dialogueLines[currentLineIndex]);
    }

    public void ShowLine(string id)
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

    private void ShowDialogue(DialogueLine line)
    {
        characterImageObject.SetActive(line.showCharacterImage);
        dialogueBox.SetActive(line.showDialogueBox);
        speakerNameText.text = line.speakerName;

        // ��������Ʈ �ҷ����� ȣ��
        if (line.showCharacterImage && !string.IsNullOrEmpty(line.characterImage))
        {
            LoadSprite(line.characterImage);
        }
        else
        {
            // ĳ���� �̹��� ����ų� �⺻ �̹����� ���� ����
            characterImage.sprite = null;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.dialogueText));

        if (nextButtonObj != null)
            nextButtonObj.SetActive(!line.disableNextButton);

        if (dialogueArrow != null)
            dialogueArrow.SetActive(line.arrowActive);

        if (!string.IsNullOrEmpty(line.actionToTrigger))
        {
            Debug.Log($"Trigger Event: {line.actionToTrigger}");
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


    public void OnNextButtonClicked()
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
        {
            ShowLine(nextID);
        }
        else
        {
            Debug.Log("End of dialogue or no next ID.");
        }
    }
}
