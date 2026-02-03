using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SimpleDialogueIntro : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Speaker")]
        public string speakerName;

        [Header("Text")]
        [TextArea(2, 6)]
        public string text;

        [Tooltip("Tiempo entre letras. Si es -1 usa el default global.")]
        public float letterDelay = -1f;

        [Header("Animation (optional)")]
        public AnimationClip animationClip;
    }

    [Header("UI")]
    [Tooltip("Contenedor del nombre (panel/GO). Se oculta si speakerName está vacío.")]
    [SerializeField] private GameObject speakerContainer;

    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogues")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Typing Settings")]
    [SerializeField] private float defaultLetterDelay = 0.03f;

    [Tooltip("Caracter que genera pausa y NO se muestra")]
    [SerializeField] private char pauseChar = '>';

    [Tooltip("Cuánto dura cada pauseChar encontrado")]
    [SerializeField] private float pauseDurationPerChar = 0.5f;

    [Header("Input")]
    [Tooltip("Nombre de acción para saltar/avanzar diálogo (ControllerManager).")]
    [SerializeField] private string skipActionName = "Submit";

    [Header("Dialogue Line Animator (optional)")]
    [SerializeField] private Animator dialogueAnimator;

    [Header("Dialogue Container Animator (optional)")]
    [Tooltip("Animator del contenedor/panel del diálogo (para mostrar/ocultar).")]
    [SerializeField] private Animator dialogueContainerAnimator;

    [Tooltip("Bool del Animator del contenedor (ej: 'Visible').")]
    [SerializeField] private string containerVisibleBool = "Visible";

    [Header("Continue Prompt Animator (optional)")]
    [SerializeField] private Animator continuePromptAnimator;

    [Tooltip("Nombre del bool que se activa cuando el texto está completo.")]
    [SerializeField] private string continueBoolName = "Show";

    [Header("Typewriter SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<AudioClip> typeClips = new List<AudioClip>();

    [Tooltip("Tiempo mínimo entre sonidos, para evitar spam.")]
    [SerializeField] private float minTimeBetweenSounds = 0.03f;

    [Tooltip("Si está activo, no suena en espacios, puntuación ni símbolos.")]
    [SerializeField] private bool ignoreNonSpeechCharacters = true;

    // ✅ NUEVO: evita el salto feo de palabras al escribir letra por letra
    [Header("Layout")]
    [Tooltip("Si está activo, pre-calcula saltos de línea para que las palabras no 'salten' mientras se escribe.")]
    [SerializeField] private bool preventWordJumping = true;

    [Header("Events")]
    public UnityEvent onDialogueStarted;
    public UnityEvent onDialogueFinished;

    private int currentLineIndex = -1;
    private Coroutine typingCoroutine;

    private bool isTyping = false;
    private bool skipTypingRequested = false;
    private bool isRunning = false;

    private float lastSfxTime = -999f;

    private void Awake()
    {
        if (dialogueText != null) dialogueText.text = "";
        if (speakerText != null) speakerText.text = "";

        SetSpeakerVisible(false);
        SetContinuePrompt(false);
        SetContainerVisible(false);
    }

    private void Update()
    {
        if (!isRunning)
            return;

        // Usamos tu ControllerManager centralizado
        if (!string.IsNullOrEmpty(skipActionName) && ControllerManager.GetActionWasPressed(skipActionName))
        {
            SkipOrNext();
        }
    }

    // -------------------------
    // PUBLIC API
    // -------------------------

    public void BeginDialogue()
    {
        if (lines == null || lines.Count == 0)
            return;

        if (isRunning)
            return;

        isRunning = true;
        currentLineIndex = -1;

        if (dialogueText != null) dialogueText.text = "";
        if (speakerText != null) speakerText.text = "";

        SetSpeakerVisible(false);
        SetContinuePrompt(false);
        SetContainerVisible(true);

        onDialogueStarted?.Invoke();
        NextLine();
    }

    public void SkipOrNext()
    {
        if (!isRunning)
            return;

        if (lines == null || lines.Count == 0)
            return;

        if (isTyping)
        {
            // Completa instantáneo
            skipTypingRequested = true;
        }
        else
        {
            // Avanza
            NextLine();
        }
    }

    // -------------------------
    // INTERNAL
    // -------------------------

    private void NextLine()
    {
        SetContinuePrompt(false);

        currentLineIndex++;

        if (currentLineIndex >= lines.Count)
        {
            FinishDialogue();
            return;
        }

        var line = lines[currentLineIndex];

        // Speaker instantáneo + ocultar contenedor si no hay nombre
        string speaker = (line.speakerName ?? "").Trim();

        if (string.IsNullOrEmpty(speaker))
        {
            SetSpeakerVisible(false);
        }
        else
        {
            SetSpeakerVisible(true);
            if (speakerText != null)
                speakerText.text = speaker;
        }

        // Animación opcional por línea
        if (dialogueAnimator != null && line.animationClip != null)
        {
            dialogueAnimator.Play(line.animationClip.name, 0, 0f);
        }

        // Arranca typewriter
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        if (dialogueText == null)
            yield break;

        isTyping = true;
        skipTypingRequested = false;

        dialogueText.text = "";

        string raw = line.text ?? "";

        // ✅ NUEVO: pre-wrap para que no salte palabra de línea
        if (preventWordJumping)
        {
            raw = PreWrapTextKeepingPauses(raw);
        }

        float delay = line.letterDelay;
        if (delay < 0f) delay = defaultLetterDelay;
        delay = Mathf.Max(0f, delay);

        if (delay <= 0f)
        {
            dialogueText.text = RemovePauseChars(raw);
            isTyping = false;
            SetContinuePrompt(true);
            yield break;
        }

        for (int i = 0; i < raw.Length; i++)
        {
            if (skipTypingRequested)
            {
                dialogueText.text = RemovePauseChars(raw);
                break;
            }

            char c = raw[i];

            // Pausas con >
            if (c == pauseChar)
            {
                yield return new WaitForSeconds(pauseDurationPerChar);
                continue;
            }

            dialogueText.text += c;
            TryPlayTypeSfx(c);

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        SetContinuePrompt(true);
    }

    private void TryPlayTypeSfx(char c)
    {
        if (sfxSource == null) return;
        if (typeClips == null || typeClips.Count == 0) return;

        if (ignoreNonSpeechCharacters)
        {
            // Solo suena con letras o números (más natural)
            if (!char.IsLetterOrDigit(c))
                return;
        }

        if (Time.time - lastSfxTime < minTimeBetweenSounds)
            return;

        lastSfxTime = Time.time;

        var clip = typeClips[Random.Range(0, typeClips.Count)];
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private string RemovePauseChars(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        return input.Replace(pauseChar.ToString(), "");
    }

    private void SetSpeakerVisible(bool visible)
    {
        if (speakerContainer != null)
            speakerContainer.SetActive(visible);
    }

    private void SetContinuePrompt(bool visible)
    {
        if (continuePromptAnimator == null) return;
        if (string.IsNullOrEmpty(continueBoolName)) return;

        continuePromptAnimator.SetBool(continueBoolName, visible);
    }

    private void SetContainerVisible(bool visible)
    {
        if (dialogueContainerAnimator == null) return;
        if (string.IsNullOrEmpty(containerVisibleBool)) return;

        dialogueContainerAnimator.SetBool(containerVisibleBool, visible);
    }

    private void FinishDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        skipTypingRequested = false;
        isRunning = false;

        SetContinuePrompt(false);
        SetSpeakerVisible(false);
        SetContainerVisible(false);

        onDialogueFinished?.Invoke();
    }

    // ============================================================
    // ✅ NUEVO: Pre-wrap para evitar salto de palabra en TMP
    // ============================================================

    private string PreWrapText(string raw)
    {
        if (dialogueText == null)
            return raw;

        // Para medir correctamente ignoramos los pauseChar (no son visibles)
        string clean = RemovePauseChars(raw);

        // Si no hay espacios, no hay nada que "salte"
        if (!clean.Contains(" "))
            return raw;

        float maxWidth = dialogueText.rectTransform.rect.width;

        if (maxWidth <= 0.01f)
            return raw;

        // Importante: aseguramos que TMP tenga su font/size aplicado antes de medir
        // (en general ya lo tiene, pero esto ayuda)
        dialogueText.ForceMeshUpdate();

        string[] words = clean.Split(' ');

        string result = "";
        string currentLine = "";

        for (int i = 0; i < words.Length; i++)
        {
            string w = words[i];

            string candidateLine = string.IsNullOrEmpty(currentLine) ? w : (currentLine + " " + w);

            Vector2 size = dialogueText.GetPreferredValues(candidateLine);
            bool fits = size.x <= maxWidth;

            if (fits)
            {
                currentLine = candidateLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(result))
                    result += "\n";

                // Si currentLine está vacío (palabra muy larga), la ponemos igual
                if (string.IsNullOrEmpty(currentLine))
                {
                    result += w;
                    currentLine = "";
                }
                else
                {
                    result += currentLine;
                    currentLine = w;
                }
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            if (!string.IsNullOrEmpty(result))
                result += "\n";

            result += currentLine;
        }

        // Devolvemos el texto ya “pre-wrappeado” (sin pauses).
        // Las pausas siguen funcionando porque se procesan en el loop,
        // pero acá ya las removimos: eso está bien porque no deben afectar el layout.
        return result;
    }

    private string PreWrapTextKeepingPauses(string rawWithPauses)
    {
        if (dialogueText == null)
            return rawWithPauses;

        if (string.IsNullOrEmpty(rawWithPauses))
            return rawWithPauses;

        // Texto visible para medir (sin pauses)
        string clean = RemovePauseChars(rawWithPauses);

        if (!clean.Contains(" "))
            return rawWithPauses;

        float maxWidth = dialogueText.rectTransform.rect.width;
        if (maxWidth <= 0.01f)
            return rawWithPauses;

        dialogueText.ForceMeshUpdate();

        // Vamos a reconstruir el texto insertando saltos de línea en el ORIGINAL,
        // pero decidiendo el wrap mirando el texto limpio.
        // Para eso necesitamos mapear índices: cleanIndex -> rawIndex
        List<int> cleanToRaw = new List<int>(clean.Length);

        int cleanCursor = 0;
        for (int rawIndex = 0; rawIndex < rawWithPauses.Length; rawIndex++)
        {
            char c = rawWithPauses[rawIndex];
            if (c == pauseChar)
                continue;

            // Este char existe en clean, entonces guardamos su rawIndex
            cleanToRaw.Add(rawIndex);
            cleanCursor++;
        }

        // Split por palabras pero manteniendo posiciones
        // Vamos a recorrer clean y decidir dónde van los \n.
        int start = 0;
        string resultRaw = "";
        string currentLineClean = "";

        // Helper: agrega un segmento del raw original [rawStart..rawEndInclusive]
        void AppendRawSegment(int rawStart, int rawEndInclusive)
        {
            if (rawStart < 0 || rawEndInclusive < rawStart) return;
            resultRaw += rawWithPauses.Substring(rawStart, rawEndInclusive - rawStart + 1);
        }

        // Recorremos clean char por char, detectando palabras
        // y armando líneas por palabras completas.
        int rawSegmentStart = 0;

        // Extraemos palabras del clean con sus rangos [a..b]
        List<(int a, int b)> wordRanges = new List<(int a, int b)>();
        int i = 0;
        while (i < clean.Length)
        {
            while (i < clean.Length && clean[i] == ' ') i++;
            if (i >= clean.Length) break;

            int a = i;
            while (i < clean.Length && clean[i] != ' ') i++;
            int b = i - 1;

            wordRanges.Add((a, b));
        }

        int lastRawCopiedIndex = -1;

        for (int w = 0; w < wordRanges.Count; w++)
        {
            var (a, b) = wordRanges[w];
            string wordClean = clean.Substring(a, b - a + 1);

            string candidateLine = string.IsNullOrEmpty(currentLineClean)
                ? wordClean
                : (currentLineClean + " " + wordClean);

            Vector2 size = dialogueText.GetPreferredValues(candidateLine);

            if (size.x <= maxWidth)
            {
                // La palabra entra en la línea actual
                currentLineClean = candidateLine;
                continue;
            }

            // No entra: hay que cortar antes de esta palabra
            // Insertamos \n en el raw justo antes de la palabra (en clean[a])
            if (!string.IsNullOrEmpty(resultRaw))
                resultRaw += "\n";

            // Copiamos desde donde quedamos hasta el final de la palabra anterior
            // En clean, la palabra anterior termina en wordRanges[w-1].b
            if (w > 0)
            {
                int prevCleanEnd = wordRanges[w - 1].b;
                int prevRawEnd = cleanToRaw[prevCleanEnd];

                int rawStart = lastRawCopiedIndex + 1;
                int rawEnd = prevRawEnd;

                AppendRawSegment(rawStart, rawEnd);
                lastRawCopiedIndex = rawEnd;
            }

            // Saltamos espacios entre palabras en el raw original
            // (no queremos copiar el espacio que “rompió” la línea)
            // Ahora la nueva línea arranca con esta palabra
            currentLineClean = wordClean;
        }

        // Copiamos lo que falta del raw
        if (!string.IsNullOrEmpty(resultRaw))
            resultRaw += "\n";

        int rawStartFinal = lastRawCopiedIndex + 1;
        int rawEndFinal = rawWithPauses.Length - 1;
        AppendRawSegment(rawStartFinal, rawEndFinal);

        // Limpieza: si quedaron líneas vacías por espacios raros, las reducimos
        resultRaw = resultRaw.Replace("\n ", "\n");

        return resultRaw;
    }
}
