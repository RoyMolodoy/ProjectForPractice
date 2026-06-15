using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Sections")]
    public Section startSection;
    public Section finishSection;
    public Section FinishWithBoss;
    public Section[] middleSections;

    public bool GenerateWithBoss;

    [Header("Settings")]
    [Range(1, 100)]
    public int middleSectionCount = 10;

    private Transform currentExit;
    private int lastSectionIndex = -1;

    private void Start()
    {
        LevelSystem levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem != null)
        {
            middleSectionCount = levelSystem.LevelNumber;
            if (levelSystem.LevelNumber % 5 == 0)
            {
                GenerateWithBoss = true;
            }

        }
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        if (startSection == null)
        {
            Debug.LogError("Start Section не назначена!");
            return;
        }

        if (finishSection == null)
        {
            Debug.LogError("Finish Section не назначена!");
            return;
        }

        if (middleSections == null || middleSections.Length == 0)
        {
            Debug.LogError("Нет обычных секций в Middle Sections!");
            return;
        }

        // Создаем стартовую секцию
        Section start = Instantiate(
            startSection,
            Vector3.zero,
            Quaternion.identity);

        currentExit = start.exitPoint;

        // Генерируем случайные секции
        for (int i = 0; i < middleSectionCount; i++)
        {
            SpawnRandomSection();
        }

        // Создаем финиш
        if(GenerateWithBoss)
        {
            SpawnSection(FinishWithBoss);
        }
        else
            SpawnSection(finishSection);
    }

    private void SpawnRandomSection()
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, middleSections.Length);
        }
        while (
            middleSections.Length > 1 &&
            randomIndex == lastSectionIndex
        );

        lastSectionIndex = randomIndex;

        SpawnSection(middleSections[randomIndex]);
    }

    private void SpawnSection(Section prefab)
    {
        Section section = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // Совмещаем EnterPoint новой секции
        // с ExitPoint предыдущей
        Vector3 offset = section.transform.position - section.enterPoint.position;

        section.transform.position = currentExit.position + offset;

        currentExit = section.exitPoint;
    }
}