using System.Collections.Generic;
using PlayerGrid;
using UIHandlers;
using Unity.Mathematics;
using UnityEngine;
using Utilities.Timer;

public class NavalMineHandler : MonoBehaviour
{
    [SerializeField] private DashboardHandler dashboardHandler;
    [SerializeField] private NavalMineBehaviour minePrefab;

    private List<MineData> _mines;

    private void Awake()
    {
        GridHandler.instance.OnMineSet.AddListener(AddMine);
        dashboardHandler.onGameStart.AddListener(PlaceMines);
    }

    public void AddMine(GridCell _cell)
    {
        _mines.Add(new MineData(_cell.position, _cell.worldPosition));
    }

    public void PlaceMines()
    {
        foreach (var mineData in _mines)
        {
            Instantiate(minePrefab, mineData.worldPosition, Quaternion.identity);
        }
    }
}
