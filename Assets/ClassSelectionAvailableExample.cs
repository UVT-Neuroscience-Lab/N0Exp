using Gtec.UnityInterface;
using System;
using TMPro;
using UnityEngine;
using static Gtec.UnityInterface.BCIManager;

public class ClassSelectionAvailableExample : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI accuracyText;

    private uint _selectedClass = 0;
    private bool _update = false;

    
    void Start()
    {
        // Attach to class selection event
        
        BCIManager.Instance.ClassSelectionAvailable += OnClassSelectionAvailable;

        //ERPBCIManager.Instance.ClassifierCalculated += OnClassifierCalculated;

       // UpdateAccuracyText();
    }
/*
    void OnApplicationQuit()
    {
        BCIManager.Instance.ClassSelectionAvailable -= OnClassSelectionAvailable;
        ERPBCIManager.Instance.ClassifierCalculated -= OnClassifierCalculated;
    }*/

    void Update()
    {
        if (_update)
        {
            switch (_selectedClass)
            {
                case 0:
                    Debug.Log("Nothing");
                    break;
                case 1:
                    player.TurnLeft();
                    break;
                case 2:
                    player.TurnRight();
                    break;
                case 3:
                    player.MoveForward();
                    break;
                case 4:
                    player.Interact();
                    break;
            }
            _update = false;
        }
    }

    private void OnClassSelectionAvailable(object sender, EventArgs e)
    {
        ClassSelectionAvailableEventArgs ea = (ClassSelectionAvailableEventArgs)e;
        _selectedClass = ea.Class;
        _update = true;
        Debug.Log(string.Format("Selected class: {0}", ea.Class));
    }

    private void OnClassifierCalculated(object sender, EventArgs e)
    {
        UpdateAccuracyText();
    }

    private void UpdateAccuracyText()
    {
        if (accuracyText == null || !ERPBCIManager.Instance.Initialized)
            return;

        var accuracy = ERPBCIManager.Instance.Accuracy();
        int selectedAverage = ERPBCIManager.Instance.NumberOfAverages;

        string text = $"Selected Averages: {selectedAverage}\n";
        foreach (var kvp in accuracy)
        {
            text += $"Avg {kvp.Key}: {kvp.Value.Mean:F2}%\n";
        }

        accuracyText.text = text;
    }
}
